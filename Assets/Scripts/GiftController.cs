using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GiftController : MonoBehaviour
{
    [SerializeField] private MeshRenderer boxRenderer;
    [SerializeField] private MeshRenderer tapeRenderer;
    private Rigidbody2D rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        GameController.GameStarted += () => { rb.isKinematic = false; };
        float randColor = Random.value;
        
        Material boxMat = new Material(boxRenderer.sharedMaterial);
        boxMat.color = Color.HSVToRGB(randColor, 1f, 1f);
        boxRenderer.material = boxMat;
        
        Material tapeMat = new Material(tapeRenderer.sharedMaterial);
        tapeMat.color = Color.HSVToRGB(1f - randColor, 1f, 1f);
        tapeRenderer.material = tapeMat;
    }
}
