using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamController : MonoBehaviour
{
    private static CamController instance;
    private Vector3 targetPos;

    public static void SetPosition(Vector3 pos, bool lerp = true)
    {
        instance.targetPos = pos;
        if (!lerp)
            instance.transform.position = instance.targetPos;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
    }

    private void Start()
    {
        instance = this;
    }
}
