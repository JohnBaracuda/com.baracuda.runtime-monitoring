using System.Collections;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class Target : MonitoredBehaviour, IDamageable
    {
        #region --- Inspector ---

        [SerializeField] private float health = 200;
        [SerializeField] private float recoverCooldown = 5f;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Fields ---

        private bool _isAlive = true;
        private float _cooldown = 0f;
        private float _currentHealth;
        
        private Animator _animator;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Fields: Static ---
        
        private static readonly int knockdown = Animator.StringToHash("knockdown");
        private static readonly int recover = Animator.StringToHash("recover");
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Setup ---

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _currentHealth = health;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Damage Handling ---

        public void TakeDamage(float damage)
        {
            if (_isAlive)
            {
                _currentHealth -= damage;
                if (_currentHealth > 0) return;
                _currentHealth = 0;
                StartCoroutine(CooldownCoroutine());
            }
        }

        private IEnumerator CooldownCoroutine()
        {
            _isAlive = false;
            _animator.SetTrigger(knockdown);
            _cooldown = recoverCooldown;
            while (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                yield return null;
            }
            _animator.SetTrigger(recover);
            _currentHealth = health;
            _isAlive = true;
        }
        
        #endregion
        
    }
}
