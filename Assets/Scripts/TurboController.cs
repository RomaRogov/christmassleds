using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class TurboController : MonoBehaviour
{
    private Vector3 startPos;
    private bool checkedIn = false;
    
    void Start()
    {
        startPos = transform.position;
    }
    
    void Update()
    {
        transform.position = startPos + Vector3.right * (Mathf.Cos(Time.time * 10f) * .2f);
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        SledsController sleds = other.GetComponent<SledsController>();
        if (!checkedIn && sleds)
        {
            checkedIn = true;
            sleds.SpeedUp(transform.right);
            transform.DOScale(Vector3.zero, 1f).SetEase(Ease.OutBack).OnComplete(() => Destroy(gameObject));
        }
    }
}
