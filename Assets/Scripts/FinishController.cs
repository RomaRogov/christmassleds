using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class FinishController : MonoBehaviour
{
    [SerializeField] private Transform chest;
    [SerializeField] private Transform chestLid;
    [SerializeField] private Rigidbody[] coins;
    private bool checkedIn = false;
    private bool chestReached = false;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        SledsController sleds = other.GetComponent<SledsController>();
        if (!checkedIn && sleds)
        {
            checkedIn = true;
            sleds.FinishReached();
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        SledsController sleds = other.gameObject.GetComponent<SledsController>();
        if (!chestReached && sleds)
        {
            chestReached = true;
            chest.DOLocalRotate(new Vector3(-15, -90f, 0f), .3f).SetEase(Ease.OutExpo).SetLoops(2, LoopType.Yoyo);
            chestLid.DOLocalRotate(Vector3.left * 90f, .3f).SetEase(Ease.OutBack);
            
            Array.ForEach(coins, coin =>
            {
                coin.isKinematic = false;
                coin.AddForce(new Vector3(Random.Range(-2f, 2f), Random.Range(7f, 10f), Random.Range(-2f, 2f))
                    , ForceMode.Impulse);
                coin.AddTorque(Random.onUnitSphere * 10f);
            });
        }
    }
}
