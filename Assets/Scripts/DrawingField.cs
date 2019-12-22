using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DrawingField : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Action DrawingStarted;
    
    [SerializeField] private LineRenderer lineRenderer;
    private bool drawing = false;
    private int pointerId;
    private Vector2 currentDrawPos;
    private Vector2 prevDrawPos;
    private List<Vector2> positions;
    private RectTransform rt;
    private float direction;

    private Vector2 scale;

    public void OnPointerDown(PointerEventData eventData)
    {
        drawing = true;
        lineRenderer.positionCount = 1;
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (rt, eventData.position, eventData.pressEventCamera, out prevDrawPos);
        ClampVector(ref prevDrawPos);
        lineRenderer.SetPosition(0, prevDrawPos);
        positions = new List<Vector2> { prevDrawPos * scale };
        direction = 0;
        pointerId = eventData.pointerId;
        DrawingStarted?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!drawing || pointerId != eventData.pointerId) return;
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle
            (rt, eventData.position, eventData.pressEventCamera, out currentDrawPos);
        ClampVector(ref currentDrawPos);
        if ((currentDrawPos - prevDrawPos).sqrMagnitude > 10f)
        {
            int lastPosInd = lineRenderer.positionCount++;
            lineRenderer.SetPosition(lastPosInd, currentDrawPos);
            direction += (currentDrawPos - prevDrawPos).x;
            prevDrawPos = currentDrawPos;
            positions.Add(currentDrawPos * scale);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData) { StopDrawingAndCreateSleds(); }

    private void StopDrawingAndCreateSleds()
    {
        if (positions != null && positions.Count > 4)
        {
            if (direction < 0)
                positions.Reverse();

            Rect borders = new Rect(positions[0], Vector2.zero);
            positions.ForEach(p =>
            {
                borders.xMin = Mathf.Min(borders.xMin, p.x);
                borders.xMax = Mathf.Max(borders.xMax, p.x);
                borders.yMin = Mathf.Min(borders.yMin, p.y);
                borders.yMax = Mathf.Max(borders.yMax, p.y);
            });
            Vector2[] posArr = new Vector2[positions.Count];
            for (int i = 0; i < positions.Count; i++)
                posArr[i] = positions[i] - borders.center;
            
            GameController.ApplyDrawing(posArr, borders.width, borders.height, direction < 0);
        }
        drawing = false;
        lineRenderer.positionCount = 0;
    }

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        scale = Vector2.one * (4f / rt.rect.height);
    }

    private void ClampVector(ref Vector2 vec)
    {
        float w = rt.rect.width / 2f;
        float h = rt.rect.height / 2f;
        vec = new Vector2(Mathf.Clamp(vec.x, -w, w),
                          Mathf.Clamp(vec.y, -h, h));
    }
}
