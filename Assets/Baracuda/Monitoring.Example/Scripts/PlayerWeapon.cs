using System.Text;
using Baracuda.Monitoring.Attributes;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Transform projectileSpawnPosition;
        [SerializeField] private ProjectilePool projectilePool;
        [SerializeField] private Image crossHair;
        [SerializeField] private Color defaultCrossHairColor = Color.white;
        [SerializeField] private Color targetCrossHairColor = Color.red;
        
        private IPlayerInput _input;
        private Camera _camera;
        private float _lastFireTime;

        [Monitor]
        [ValueProcessor(nameof(CurrentAmmunitionValueProcessor))]
        [Format(FontSize = 46, Position = UIPosition.BottomLeft)]
        private int _currentAmmunition;

        private static string CurrentAmmunitionValueProcessor(int currentAmmunition)
        {
            var sb = new StringBuilder();
            sb.Append("Ammunition: ");
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
            PreformRaycast();
            
            _camera.fieldOfView = _input.SecondaryFirePressed ? 40f : 90f;
            
            var time = Time.time;
            
            if (_input.PrimaryFirePressed && _currentAmmunition > 0 && time - _lastFireTime > 1 / shotsPerSecond)
            {
                _lastFireTime = time;
                _currentAmmunition--;
                for (var i = 0; i < bulletsPerShot; i++)
                {
                    var projectile = projectilePool.GetProjectileFromPool();
                    projectile.Setup(
                        position: projectileSpawnPosition.position,
                        rotation: projectileSpawnPosition.rotation,
                        damage: damage, 
                        force: projectileSpawnPosition.forward * bulletForce,
                        spread: bulletSpread);   
                }
            }
        }

        private void PreformRaycast()
        {
            var ray = _camera.ViewportPointToRay(new Vector3(0.5F, 0.5F, 0));
            if (Physics.Raycast(ray, out var hit))
            {
                var component = hit.collider.GetComponentInParent<IDamageable>();
                crossHair.color = component != null
                    ? targetCrossHairColor 
                    : defaultCrossHairColor;
                
                projectileSpawnPosition.LookAt(hit.point);
            }
            else
            {
                crossHair.color = defaultCrossHairColor;
                projectileSpawnPosition.localRotation = Quaternion.identity;
            }
        }

        public void ReplenishAmmunition()
        {
            _currentAmmunition = ammunition;
        }
    }
}