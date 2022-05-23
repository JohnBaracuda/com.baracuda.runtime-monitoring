// Copyright (c) 2022 Jonathan Lang
using System.Collections;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class AmmunitionPack : MonitoredBehaviour
    {
        [SerializeField] private float cooldown = 10f;
        [SerializeField] private GameObject ammunitionMesh;
        
        private bool _isActive = true;
        private float _cooldown = 0;

        private void OnTriggerStay(Collider other)
        {
            if (_isActive)
            {
                if (other.TryGetComponent<PlayerWeapon>(out var playerWeapon))
                {
                    playerWeapon.ReplenishAmmunition();
                    StartCoroutine(CooldownCoroutine());
                }
            }
        }

        private IEnumerator CooldownCoroutine()
        {
            _isActive = false;
            ammunitionMesh.SetActive(false);
            _cooldown = cooldown;
            while (_cooldown > 0)
            {
                _cooldown -= Time.deltaTime;
                yield return null;
            }
            _cooldown = 0;
            ammunitionMesh.SetActive(true);
            _isActive = true;
        }

        private void Update()
        {
            ammunitionMesh.transform.Rotate(new Vector3(0, 30, 0) * Time.deltaTime);
        }
    }
}