using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;

public class UIScarf : MonoBehaviour
{
    public Color Color
    {
        get => lr.color;
        set => lr.color = value;
    }
    
    [SerializeField] private float width;
    [SerializeField] private float height;
    [Range(0f, 1f)][SerializeField] private float wave;
    
    private UILineRenderer lr;
    private float waver;
    private float randCoef;
    
    void Awake()
    {
        lr = GetComponent<UILineRenderer>();
        lr.Points = new Vector2[20];
        randCoef = Random.Range(7f, 10f);
        
        //Hacky move to enable comparing between editor's color and hex value from GameController
        int hexColor = Mathf.RoundToInt(Color.r * 0xFF) << 16 | 
                       Mathf.RoundToInt(Color.g * 0xFF) << 8 |
                       Mathf.RoundToInt(Color.b * 0xFF);
        float r = (hexColor >> 16) & 0xFF;
        float g = (hexColor >> 8) & 0xFF;
        float b = hexColor & 0xFF;
        Color = new Color(r / 255f, g / 255f, b / 255f);
    }
    
    void Update()
    {
        waver += Time.deltaTime * randCoef * (.5f + (Mathf.Sin(Time.time) + 1f) / 4f);
        for (int i = 0; i < 20; i++)
        {
            float xpos = -width / 2f + i * width / 20f;
            float f = i * wave + waver;
            float pos = 1f - i / 20f;
            lr.Points[i] = new Vector2(xpos, Mathf.Sin(pos * Mathf.PI / 2f) * Mathf.Sin(f) * height / 2f);
        }
        lr.SetAllDirty();
    }
}
