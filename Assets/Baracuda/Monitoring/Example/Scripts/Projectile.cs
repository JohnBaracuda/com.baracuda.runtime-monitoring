// Copyright (c) 2022 Jonathan Lang

using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Baracuda.Example.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class Projectile : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private float _damage;
        private Transform _transform;
        private static readonly WaitForSeconds waitForSeconds = new WaitForSeconds(.05f);

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _transform = transform;
        }

        public void Setup(Vector3 position, Quaternion rotation, float damage, Vector3 force, float spread = 100f)
        {
            gameObject.SetActive(true);
            _transform.rotation = rotation;
            _transform.position = position;
            _damage = damage;
            _rigidbody.velocity = Vector3.zero;
            var randomX = Random.Range(-spread, spread);
            var randomY = Random.Range(-spread, spread);
            var randomZ = Random.Range(-spread, spread);
            var randomVector = new Vector3(randomX, randomY, randomZ);
            _rigidbody.AddForce(force + randomVector);
            StartCoroutine(SecondForce(force));
        }

        private IEnumerator SecondForce(Vector3 force)
        {
            yield return waitForSeconds;
            _rigidbody.AddForce(force);
        }


        private void OnCollisionEnter(Collision collision)
        {
            var component = collision.gameObject.GetComponent<IDamageable>();
            component?.TakeDamage(_damage);
            gameObject.SetActive(false);
        }
    }
}