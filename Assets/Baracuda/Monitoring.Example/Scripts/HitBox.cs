// Copyright (c) 2022 Jonathan Lang
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class HitBox : MonoBehaviour, IDamageable
    {
        [SerializeField] private float damageMultiplier = 1f;
        private ShootingTarget _shootingTarget;
        
        private void Awake()
        {
            _shootingTarget = GetComponentInParent<ShootingTarget>();
        }

        public void TakeDamage(float damage)
        {
            _shootingTarget.TakeDamage(damage * damageMultiplier);
        }
    }
}