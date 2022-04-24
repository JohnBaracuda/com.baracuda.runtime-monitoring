using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class HitBox : MonoBehaviour, IDamageable
    {
        [SerializeField] private float damageMultiplier = 1f;
        private Target _target;
        
        private void Awake()
        {
            _target = GetComponentInParent<Target>();
        }

        public void TakeDamage(float damage)
        {
            _target.TakeDamage(damage * damageMultiplier);
        }
    }
}