using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Starlight.Weapons
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] internal Rigidbody rb;
        Vector3 direction;
        [SerializeField] float startSpeed;
        [SerializeField] float acceleration;
        [SerializeField] GameObject impactPrefab;
        [SerializeField] float gravityModifier;
        private void Start()
        {
            direction = transform.forward;
            rb.velocity = direction * startSpeed;
            //Instantiate the prefab, then assign the new prefab to the impactPrefab. We don't need the prefab anymore once its instanitated.
            var go = Instantiate(impactPrefab, Vector3.zero, Quaternion.identity, transform.parent);
            impactPrefab = go;
            go.SetActive(false);
            rb.useGravity = gravityModifier == 1;
        }
        private void FixedUpdate()
        {
            rb.AddForce(direction * acceleration);
            if(gravityModifier != 1)
            {
                rb.AddForce(Physics.gravity * gravityModifier);
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"{name} collided with {collision.gameObject.name}", collision.gameObject);
            Destroy(gameObject, 0.01f);
        }
        private void OnDestroy()
        {
            impactPrefab.transform.SetParent(null);
            impactPrefab.SetActive(true);
            Destroy(impactPrefab, 10f);

        }
    }
}