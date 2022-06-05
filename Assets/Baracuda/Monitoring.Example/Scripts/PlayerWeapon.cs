// Copyright (c) 2022 Jonathan Lang

using System;
using Baracuda.Pooling.Concretions;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class PlayerWeapon : MonitoredBehaviour
    {
        #region --- Fields ---

        /*
         *  Inspector Fields   
         */

        [Header("Primary")] [SerializeField] private float damage = 100f;
        [SerializeField] private bool fullAutomatic = true;
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

        /*
         *  Private Fields   
         */

        [Monitor]
        [MUpdateEvent(nameof(OnAmmoChanged))]
        [MFormatOptions(UIPosition.LowerLeft, FontSize = 20)]
        [MValueProcessor(nameof(CurrentAmmunitionProcessor))]
        private int _currentAmmunition;

        [Monitor]
        private event Action<int> OnAmmoChanged;
        
        private float _lastFireTime;
        private float _targetFOV;
        private IPlayerInput _input;
        private Camera _camera;
        private bool _canFireSemiAutomatic = true;

        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Weapon Logic ---
 
        /*
         * Value Processor   
         */

        private string CurrentAmmunitionProcessor(int current)
        {
            var sb = StringBuilderPool.Get();
            sb.Append("Ammunition: ");
            sb.Append(current);
            sb.Append(" / ");
            sb.Append(ammunition);
            return StringBuilderPool.Release(sb);
        }

        /*
         *  Logic   
         */

        protected override void Awake()
        {
            base.Awake();
            _input = GetComponent<IPlayerInput>();
            _camera = GetComponentInChildren<Camera>();
            _currentAmmunition = ammunition;
            OnAmmoChanged?.Invoke(_currentAmmunition);
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            PreformRaycast();
            
            _targetFOV = _input.SecondaryFirePressed ? zoomFOV : defaultFOV;
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, fovSharpness * deltaTime);
            var time = Time.time;
            
            if (_input.PrimaryFirePressed && (fullAutomatic || _canFireSemiAutomatic) && _currentAmmunition > 0 && time - _lastFireTime > 1 / shotsPerSecond)
            {
                _canFireSemiAutomatic = false;
                _lastFireTime = time;
                for (var i = 0; i < bulletsPerShot; i++)
                {
                    _currentAmmunition--;
                    OnAmmoChanged?.Invoke(_currentAmmunition);
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

            if (!_input.PrimaryFirePressed)
            {
                _canFireSemiAutomatic = true;
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
            OnAmmoChanged?.Invoke(_currentAmmunition);
        }
        
        #endregion
    }
}