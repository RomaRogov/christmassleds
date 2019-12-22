using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class AddSafeZone : MonoBehaviour
{
    [SerializeField] private bool isLayoutElement;
    
    void Start()
    {
        float scale = GetComponentInParent<CanvasScaler>().referenceResolution.x / Screen.width;
        if (isLayoutElement)
            GetComponent<LayoutElement>().minHeight += Screen.safeArea.y * scale;
        else
            GetComponent<RectTransform>().anchoredPosition += Vector2.down * (Screen.safeArea.y * scale);
    }
}
