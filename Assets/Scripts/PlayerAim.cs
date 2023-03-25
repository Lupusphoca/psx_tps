namespace PSX_VerticalSlice
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Cinemachine;

    public class PlayerAim : MonoBehaviour
    {
        [Header("Required Components")]
        [SerializeField] internal PlayerSettings ps;
        [SerializeField] internal PlayerInputValues piv;
        [SerializeField] internal Transform shootPoint;
        [SerializeField] internal CinemachineVirtualCamera aimVirtualCamera;
        [SerializeField] internal Transform aimDebugTransform;

        [Header("Settings")]
        [SerializeField] internal float range = 100f;
        [SerializeField] internal float fireRate = 0.2f;
        [SerializeField] internal LayerMask shootableLayer;
        [SerializeField] internal LayerMask aimColliderLayerMask = new LayerMask();

        internal Vector3 aimingLocalisation;
        internal Vector3 facingDirection;
        internal RaycastHit raycastHit;
        internal bool doesRaycastHit = false;

        internal float fireTimer = 0f;

        private void Update()
        {
            Aim();
        }

        private void Aim()
        {
            ps.animator.SetBool(ps.animIDAim, piv.aim);
            if (piv.aim)
            {
                aimVirtualCamera.gameObject.SetActive(true);

                // Turn player toward aiming direction
                aimingLocalisation = shootPoint.position + Camera.main.transform.forward * range;
                facingDirection = (new Vector3(aimingLocalisation.x, ps.transformPlayer.position.y, aimingLocalisation.z) - ps.transformPlayer.position).normalized;
                ps.transformPlayer.forward = Vector3.Lerp(ps.transformPlayer.forward, facingDirection, Time.deltaTime * 20f);

                var screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
                var ray = Camera.main.ScreenPointToRay(screenCenterPoint);
                doesRaycastHit = Physics.Raycast(ray, out raycastHit, range, aimColliderLayerMask);
                if (doesRaycastHit)
                {
                    aimDebugTransform.position = raycastHit.point;

                    Shoot(raycastHit.point);
                }
                else
                {
                    Shoot(aimingLocalisation);
                }
            }
            else
            {
                aimVirtualCamera.gameObject.SetActive(false);
            }
        }

        // Function called even if the Aim function touched nothig
        private void Shoot(Vector3 endingPoint)
        {
            Debug.DrawLine(Camera.main.transform.position, endingPoint, Color.yellow, 0.05f);
            Debug.DrawLine(shootPoint.position, endingPoint, Color.yellow, 0.05f);

            fireTimer += Time.deltaTime;

            if (piv.aim && piv.shoot && fireTimer >= fireRate)
            {
                if (doesRaycastHit)
                {
                    Touch();
                }
                fireTimer = 0f;
            }
        }

        // Function called only if the Aim function find a object at the end of his ray
        private void Touch()
        {
            Debug.Log($"Hit : " + raycastHit.collider.gameObject.name);
        }
    }
}
