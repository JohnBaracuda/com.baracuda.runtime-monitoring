// Copyright (c) 2022 Jonathan Lang

using Baracuda.Monitoring;
using System.Collections;
using System.Globalization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Baracuda.Example.Scripts
{
    [MGroupOrder(-100)]
    public class ShootingTarget : MonitoredBehaviour
    {
        #region Inspector ---

        [SerializeField] private float health = 200;
        [SerializeField] private float recoverCooldownMin = 1f;
        [SerializeField] private float recoverCooldownMax = 4f;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Fields ---

        private bool _isAlive = true;
        [Monitor] [MShowIf(Condition.NotZero)]
        private float _currentHealth;
        [Monitor] [MShowIf(Condition.NotZero)]
        private float _cooldown = 0f;
        private Animator _animator;

        #endregion

        //--------------------------------------------------------------------------------------------------------------

        #region Animator ---

        private static readonly int knockdown = Animator.StringToHash("knockdown");
        private static readonly int recover = Animator.StringToHash("recover");

        #endregion

        #region --- Setup ---

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _currentHealth = health;
            GameManager.Current.GameStateChanged += OnGameStateChanged;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (GameManager.TryGetCurrent(out var current))
            {
                current.GameStateChanged -= OnGameStateChanged;
            }
        }

        private void OnGameStateChanged(GameState gameState)
        {
            if (gameState == GameState.Respawning)
            {
                Reset();
            }
        }

        private void Reset()
        {
            StopAllCoroutines();
            _cooldown = 0;
            _animator.ResetTrigger(knockdown);
            _animator.SetTrigger(recover);
            _currentHealth = health;
            _isAlive = true;
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

            _animator.ResetTrigger(recover);
            _animator.SetTrigger(knockdown);
            _cooldown = Random.Range(recoverCooldownMin, recoverCooldownMax);

            Debug.Log($"{name} destroyed! starting {_cooldown.ToString(CultureInfo.InvariantCulture)}s cooldown.");
            while (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                yield return null;
            }

            Reset();
        }

        #endregion

    }
}
