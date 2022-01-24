using System.Collections;
using Baracuda.Monitoring.Attributes;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class Target : MonitoredBehaviour, IDamageable
    {
        #region --- [INSPECTOR] ---

        [SerializeField] private float health = 200;
        [SerializeField] private float recoverCooldown = 5f;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FIELDS] ---

        [Monitor] [Format(GroupElement = true, Position = UIPosition.BottomRight)] 
        private bool _isAlive = true;
        [Monitor] [Format("0.0", GroupElement = true)]
        private float _cooldown = 0f;
        [Monitor] [Format(FontSize = 16 , GroupElement = true)]
        private float _currentHealth;
        
        private Animator _animator;
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [FIELDS: STATIC] ---

        [Monitor]
        [Format(FontSize = 36, GroupElement = false)]
        [ValueProcessor(nameof(TargetsDestroyedValueProcessor))]
        private static int _targetsDestroyed;
        
        private static readonly int _knockdown = Animator.StringToHash("knockdown");
        private static readonly int _recover = Animator.StringToHash("recover");
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [VALUE PROCESSOR] ---

        private static string TargetsDestroyedValueProcessor(int targetsDestroyed)
        {
            return $"Score: [{targetsDestroyed.ToString("00")}]";
        }
        
        #endregion

        //--------------------------------------------------------------------------------------------------------------
        
        #region --- [SETUP] ---

        protected override void Awake()
        {
            base.Awake();
            _animator = GetComponent<Animator>();
            _currentHealth = health;
        }
        
        #endregion
        
        //--------------------------------------------------------------------------------------------------------------

        #region --- [DAMAGE HANDLING] ---

        public void TakeDamage(float damage)
        {
            if (_isAlive)
            {
                _currentHealth -= damage;
                if (_currentHealth > 0) return;
                _currentHealth = 0;
                _targetsDestroyed++;
                StartCoroutine(CooldownCoroutine());
            }
        }

        private IEnumerator CooldownCoroutine()
        {
            _isAlive = false;
            _animator.SetTrigger(_knockdown);
            _cooldown = recoverCooldown;
            while (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                yield return null;
            }
            _animator.SetTrigger(_recover);
            _currentHealth = health;
            _isAlive = true;
        }
        
        #endregion
        
    }
}
