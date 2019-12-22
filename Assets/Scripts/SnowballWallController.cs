using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SnowballWallController : MonoBehaviour
{
    private Rigidbody[] childBodies;

    private void Start()
    {
        childBodies = GetComponentsInChildren<Rigidbody>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Array.ForEach(childBodies, c =>
        {
            c.useGravity = true;
            c.AddExplosionForce(
                other.attachedRigidbody.velocity.magnitude,
                other.transform.position, 20f, 0f);
        });
    }
}
