using System.Text;
using Baracuda.Monitoring.Attributes;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class PlayerWeapon : MonitoredBehaviour
    {
        [Header("Primary")]
        [SerializeField] private float damage = 100f;
        [SerializeField] private float shotsPerSecond = 7.5f;
        [SerializeField] private int bulletsPerShot = 3;
        [SerializeField] private float bulletSpread = 50f;
        [SerializeField] private float bulletForce;
        [SerializeField] private int ammunition = 15;
        [SerializeField] private LayerMask layerMask;
        [SerializeField] private Transform projectileSpawnPosition;
        [SerializeField] private ProjectilePool projectilePool;
        
        [Header("Field Of View")]
        [SerializeField] private float defaultFOV = 90f;
        [SerializeField] private float zoomFOV = 40f;
        [SerializeField] private float fovSharpness = 10f;
        
        private IPlayerInput _input;
        private Camera _camera;
        private float _lastFireTime;

        [Monitor]
        [ValueProcessor(nameof(CurrentAmmunitionValueProcessor))]
        [Format(FontSize = 46, GroupElement = false, Position = UIPosition.BottomLeft)]
        [ProgressBar(0, 45, 45)]
        private int _currentAmmunition;
        private float _targetFOV;

        private string CurrentAmmunitionValueProcessor(int currentAmmunition)
        {
            var sb = new StringBuilder();
            sb.Append("Ammo: ");
            sb.Append(currentAmmunition.ToString("00"));
            sb.Append(" / ");
            sb.Append(ammunition.ToString("00"));
            sb.Append(' ');
            for (var i = 0; i < currentAmmunition; i++)
            {
                sb.Append('|');
            }
            return sb.ToString();
        }

        protected override void Awake()
        {
            base.Awake();
            _input = GetComponent<IPlayerInput>();
            _camera = GetComponentInChildren<Camera>();
            _currentAmmunition = ammunition;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            PreformRaycast();
            
            _targetFOV = _input.SecondaryFirePressed ? zoomFOV : defaultFOV;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, fovSharpness * deltaTime);
            var time = Time.time;
            
            if (_input.PrimaryFirePressed && _currentAmmunition > 0 && time - _lastFireTime > 1 / shotsPerSecond)
            {
                _lastFireTime = time;
                for (var i = 0; i < bulletsPerShot; i++)
                {
                    _currentAmmunition--;
                    var projectile = projectilePool.GetProjectileFromPool();
                    projectile.Setup(
                        position: projectileSpawnPosition.position,
                        rotation: projectileSpawnPosition.rotation,
                        damage: damage, 
                        force: projectileSpawnPosition.forward * bulletForce,
                        spread: bulletSpread);
                    
                    if (_currentAmmunition <= 0)
                    {
                        break;
                    }
                }
            }
        }

        private void PreformRaycast()
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            if (Physics.Raycast(ray, out var hit, layerMask))
            {
                projectileSpawnPosition.LookAt(hit.point);
            }
            else
            {
                projectileSpawnPosition.localRotation = Quaternion.identity;
            }
        }

        public void ReplenishAmmunition()
        {
            _currentAmmunition = ammunition;
        }
    }
}