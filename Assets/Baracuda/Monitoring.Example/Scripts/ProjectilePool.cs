// Copyright (c) 2022 Jonathan Lang
using System.Collections.Generic;
using UnityEngine;

namespace Baracuda.Monitoring.Example.Scripts
{
    public class ProjectilePool : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private int projectileCount = 100;

        private readonly Queue<Projectile> _projectilePool = new Queue<Projectile>();

        private void Awake()
        {
            for (var i = 0; i < projectileCount; i++)
            {
                var projectile = Instantiate(projectilePrefab, Vector3.zero, Quaternion.identity);
                projectile.gameObject.SetActive(false);
                _projectilePool.Enqueue(projectile);
            }
        }

        public Projectile GetProjectileFromPool()
        {
            var projectile = _projectilePool.Dequeue();
            _projectilePool.Enqueue(projectile);
            return projectile;
        }
        
    }
}
