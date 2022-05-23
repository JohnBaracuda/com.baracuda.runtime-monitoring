// Copyright (c) 2022 Jonathan Lang
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class ShootingTarget : MonitoredBehaviour
    {
        #region --- Inspector ---

        [SerializeField] private float health = 200;
        [SerializeField] private float recoverCooldownMin = 1f;
        [SerializeField] private float recoverCooldownMax = 4f;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------
        
        #region --- Fields ---
        
        [Monitor] 
        private bool _isAlive = true;
        [Monitor] 
        private float _currentHealth;
        private float _cooldown = 0f;
        private Animator _animator;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- Animator ---
        
        private static readonly int knockdown = Animator.StringToHash("knockdown");
        private static readonly int recover = Animator.StringToHash("recover");
        
        #endregion
        
        #region --- Setup ---

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _currentHealth = health;
        }
        
        #endregion
        
        #region --- Damage Handling ---

        public void TakeDamage(float damage)
        {
            if (_isAlive)
            {
                _currentHealth -= damage;
                if (_currentHealth > 0)
                {
                    return;
                }

                _currentHealth = 0;
                StartCoroutine(CooldownCoroutine());
            }
        }

        private IEnumerator CooldownCoroutine()
        {
            _isAlive = false;
            _animator.SetTrigger(knockdown);
            _cooldown = Random.Range(recoverCooldownMin, recoverCooldownMax);
            while (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                yield return null;
            }
            _cooldown = 0;
            _animator.SetTrigger(recover);
            _currentHealth = health;
            _isAlive = true;
        }
        
        #endregion
    }
}
