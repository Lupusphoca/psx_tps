namespace PSX_VerticalSlice
{
    using UnityEngine;

    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] PlayerSettings ps;
        [SerializeField] PlayerInputValues piv;

        private void Start()
        {
            ps.cinemachineTargetYaw = ps.cinemachineCameraTarget.transform.rotation.eulerAngles.y;

            ps.hasAnimator = TryGetComponent(out ps.animator);

            AssignAnimationIDs();

            // Reset our timeouts on start
            ps.jumpTimeoutDelta = ps.jumpTimeout;
            ps.fallTimeoutDelta = ps.fallTimeout;
        }

        private void Update()
        {
            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            ps.animIDSpeed = Animator.StringToHash("Speed");
            ps.animIDGrounded = Animator.StringToHash("Grounded");
            ps.animIDJump = Animator.StringToHash("Jump");
            ps.animIDFreeFall = Animator.StringToHash("FreeFall");
            ps.animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            ps.animIDAim = Animator.StringToHash("Aiming");
        }

        private void GroundedCheck()
        {
            // Set sphere position, with offset
            var spherePosition = new Vector3(transform.position.x, transform.position.y - ps.groundedOffset, transform.position.z);
            ps.grounded = Physics.CheckSphere(spherePosition, ps.groundedRadius, ps.groundLayers, QueryTriggerInteraction.Ignore);

            // Update animator if using character
            if (ps.hasAnimator)
            {
                ps.animator.SetBool(ps.animIDGrounded, ps.grounded);
            }
        }

        private void CameraRotation()
        {
            // If there is an input and camera position is not fixed
            if (piv.look.sqrMagnitude >= ps.cameraRotationThreshold && !ps.lockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                var deltaTimeMultiplier = ps.IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                ps.cinemachineTargetYaw += piv.look.x * deltaTimeMultiplier * ps.Sensitivity;
                ps.cinemachineTargetPitch += piv.look.y * deltaTimeMultiplier * ps.Sensitivity;
            }

            // Clamp our rotations so our values are limited 360 degrees
            ps.cinemachineTargetYaw = ClampAngle(ps.cinemachineTargetYaw, float.MinValue, float.MaxValue);
            ps.cinemachineTargetPitch = ClampAngle(ps.cinemachineTargetPitch, ps.bottomClamp, ps.topClamp);

            // Cinemachine will follow this target
            ps.cinemachineCameraTarget.transform.rotation = Quaternion.Euler(ps.cinemachineTargetPitch + ps.cameraAngleOverride, ps.cinemachineTargetYaw, 0.0f);
        }

        private void Move()
        {
            // Set target speed based on move speed, sprint speed and if sprint is pressed
            var targetSpeed = piv.sprint ? ps.sprintSpeed : ps.moveSpeed;

            // A simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // Note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // If there is no input, set the target speed to 0
            if (piv.move == Vector2.zero) { targetSpeed = 0.0f; }

            // A reference to the players current horizontal velocity
            var currentHorizontalSpeed = new Vector3(ps.characterController.velocity.x, 0.0f, ps.characterController.velocity.z).magnitude;

            var speedOffset = 0.1f;
            var inputMagnitude = piv.analogMovement ? piv.move.magnitude : 1f;

            // Accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // Creates curved result rather than a linear one giving a more organic speed change
                // Note T in Lerp is clamped, so we don't need to clamp our speed
                ps.speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * ps.speedChangeRate);

                // Round speed to 3 decimal places
                ps.speed = Mathf.Round(ps.speed * 1000f) / 1000f;
            }
            else
            {
                ps.speed = targetSpeed;
            }

            ps.animationBlend = Mathf.Lerp(ps.animationBlend, targetSpeed, Time.deltaTime * ps.speedChangeRate);
            if (ps.animationBlend < 0.01f) { ps.animationBlend = 0f; }

            // Normalise input direction
            var inputDirection = new Vector3(piv.move.x, 0.0f, piv.move.y).normalized;

            // Note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // If there is a move input rotate player when the player is moving
            if (piv.move != Vector2.zero)
            {
                ps.targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + ps.mainCamera.transform.eulerAngles.y;
                var rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, ps.targetRotation, ref ps.rotationVelocity, ps.rotationSmoothTime);

                if (!piv.aim)
                {
                    // Rotate to face input direction relative to camera position
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }


            var targetDirection = Quaternion.Euler(0.0f, ps.targetRotation, 0.0f) * Vector3.forward;

            // Move the player
            ps.characterController.Move(targetDirection.normalized * (ps.speed * Time.deltaTime) + new Vector3(0.0f, ps.verticalVelocity, 0.0f) * Time.deltaTime);

            // Update animator if using character
            if (ps.hasAnimator)
            {
                ps.animator.SetFloat(ps.animIDSpeed, ps.animationBlend);
                ps.animator.SetFloat(ps.animIDMotionSpeed, inputMagnitude);
            }
        }

        private void JumpAndGravity()
        {
            if (ps.grounded)
            {
                // Reset the fall timeout timer
                ps.fallTimeoutDelta = ps.fallTimeout;

                // Update animator if using character
                if (ps.hasAnimator)
                {
                    ps.animator.SetBool(ps.animIDJump, false);
                    ps.animator.SetBool(ps.animIDFreeFall, false);
                }

                // Stop our velocity dropping infinitely when grounded
                if (ps.verticalVelocity < 0.0f)
                {
                    ps.verticalVelocity = -2f;
                }

                // Jump
                if (piv.jump && ps.jumpTimeoutDelta <= 0.0f)
                {
                    // The square root of H * -2 * G = how much velocity needed to reach desired height
                    ps.verticalVelocity = Mathf.Sqrt(ps.jumpHeight * -2f * ps.gravity);

                    // Update animator if using character
                    if (ps.hasAnimator)
                    {
                        ps.animator.SetBool(ps.animIDJump, true);
                    }
                }

                // Jump timeout
                if (ps.jumpTimeoutDelta >= 0.0f)
                {
                    ps.jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // Reset the jump timeout timer
                ps.jumpTimeoutDelta = ps.jumpTimeout;

                // Fall timeout
                if (ps.fallTimeoutDelta >= 0.0f)
                {
                    ps.fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // Update animator if using character
                    if (ps.hasAnimator)
                    {
                        ps.animator.SetBool(ps.animIDFreeFall, true);
                    }
                }

                // If we are not grounded, do not jump
                piv.jump = false;
            }

            // Apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (ps.verticalVelocity < ps.terminalVelocity)
            {
                ps.verticalVelocity += ps.gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) { lfAngle += 360f; }
            if (lfAngle > 360f) { lfAngle -= 360f; }
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (ps.grounded) { Gizmos.color = transparentGreen; }
            else Gizmos.color = transparentRed;

            // When selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - ps.groundedOffset, transform.position.z), ps.groundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (ps.footstepSounds.Length > 0)
                {
                    var index = Random.Range(0, ps.footstepSounds.Length);
                    AudioSource.PlayClipAtPoint(ps.footstepSounds[index], transform.TransformPoint(ps.characterController.center), ps.footstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                AudioSource.PlayClipAtPoint(ps.landingSound, transform.TransformPoint(ps.characterController.center), ps.footstepAudioVolume);
            }
        }
    }
}