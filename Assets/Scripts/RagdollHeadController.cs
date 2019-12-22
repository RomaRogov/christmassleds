using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class RagdollHeadController : MonoBehaviour
{
    public Action onHeadBanged;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Level"))
            onHeadBanged?.Invoke();
    }
}
