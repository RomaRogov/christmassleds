using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;
using UnityEngine;

public class SimpleRagdollController : MonoBehaviour
{
    public RagdollHeadController Head => head.GetComponent<RagdollHeadController>();
    public Transform SledsEnd;

    [SerializeField] private GameObject head;
    [SerializeField] private GameObject hands;
    [SerializeField] private GameObject legs;
    [SerializeField] private Rigidbody2D handsHandle;
    [SerializeField] private LineRenderer rigForward;
    [SerializeField] private LineRenderer rigBack;
    [SerializeField] private Transform palm;
    [SerializeField] private LineRenderer scarf;
    [SerializeField] private Transform[] scarfPoints;
    private FixedJoint2D assJoint;
    private SpringJoint2D handJoint;

    private Vector3 headStartPos;
    private Vector3 handsStartPos;
    private Vector3 legsStartPos;

    private Rigidbody2D rb;
    private Rigidbody2D headRB;
    private Rigidbody2D handsRB;
    private Rigidbody2D legsRB;

    private Rigidbody2D sledsRB;
    
    public void Init(Rigidbody2D parent, int layer)
    {
        assJoint = GetComponent<FixedJoint2D>();
        assJoint.connectedBody = parent;
        handJoint = hands.GetComponent<SpringJoint2D>();
        sledsRB = parent;

        gameObject.layer = layer;
        head.layer = layer;
        hands.layer = layer;
        legs.layer = layer;

        headStartPos = head.transform.localPosition;
        handsStartPos = hands.transform.localPosition;
        legsStartPos = legs.transform.localPosition;

        rb = GetComponent<Rigidbody2D>();
        headRB = head.GetComponent<Rigidbody2D>();
        handsRB = hands.GetComponent<Rigidbody2D>();
        legsRB = legs.GetComponent<Rigidbody2D>();
    }

    public void MoveToDefault(float time, bool isRight)
    {
        //assJoint.connectedAnchor = sledsTrans.InverseTransformPoint(transform.TransformPoint(assJoint.anchor));
        SwitchRigidbodies(false);

        head.transform.DOLocalMove(headStartPos, time).SetEase(Ease.OutExpo);
        hands.transform.DOLocalMove(handsStartPos, time).SetEase(Ease.OutExpo);
        legs.transform.DOLocalMove(legsStartPos, time).SetEase(Ease.OutExpo);
        
        head.transform.DORotate(Vector3.zero, time).SetEase(Ease.OutExpo);
        hands.transform.DORotate(Vector3.zero, time).SetEase(Ease.OutExpo);
        legs.transform.DORotate(Vector3.zero, time).SetEase(Ease.OutExpo);
        
        transform.DOScale(new Vector3(isRight ? 1f : -1f, 1f, 1f), time).SetEase(Ease.OutExpo).OnComplete(() =>
        {
            //assJoint.connectedAnchor = sledsTrans.InverseTransformPoint(transform.TransformPoint(assJoint.anchor));
            SwitchRigidbodies(true);
        });
    }

    public void SetToDefault(bool isRight)
    {
        head.transform.localPosition = headStartPos;
        hands.transform.localPosition = handsStartPos;
        legs.transform.localPosition = legsStartPos;

        head.transform.localEulerAngles = Vector3.zero;
        hands.transform.localEulerAngles = Vector3.zero;
        legs.transform.localEulerAngles = Vector3.zero;
        
        transform.localScale = new Vector3(isRight ? 1f : -1f, 1f, 1f);

        headRB.position = head.transform.position;
        handsRB.position = hands.transform.position;
        legsRB.position = legs.transform.position;

        headRB.rotation = 0f;
        handsRB.rotation = 0f;
        legsRB.rotation = 0f;
    }

    public void SetAnchor(Vector2 pos)
    {
        if (!rb.simulated)
            transform.localPosition = pos;
        assJoint.connectedAnchor = pos;
        assJoint.anchor = Vector2.zero;
    }

    private void Update()
    {
        rigForward.SetPosition(0, new Vector3(palm.position.x, palm.position.y, transform.position.z -.7f));
        rigForward.SetPosition(1, new Vector3(SledsEnd.position.x, SledsEnd.position.y, transform.position.z -.7f));
        rigBack.SetPosition(0, new Vector3(palm.position.x, palm.position.y, transform.position.z + .7f));
        rigBack.SetPosition(1, new Vector3(SledsEnd.position.x, SledsEnd.position.y, transform.position.z + .7f));

        scarf.positionCount = scarfPoints.Length;
        scarf.SetPositions(Array.ConvertAll(scarfPoints, p => p.position));
    }

    private void FixedUpdate()
    {
        handJoint.connectedAnchor = SledsEnd.position;
        /*float fall = Mathf.Min(sledsRB.velocity.y / 20f, 2f);
        assJoint.anchor = Vector2.Lerp(assJoint.anchor, Vector2.up * fall, Time.fixedDeltaTime * 20f);*/
    }

    private void SwitchRigidbodies(bool enabled)
    {
        rb.simulated = enabled;
        headRB.simulated = enabled;
        handsRB.simulated = enabled;
        legsRB.simulated = enabled;
        
        //Array.ForEach(GetComponentsInChildren<Joint2D>(), joint => joint.enabled = enabled);

        if (!enabled)
        {
            rb.velocity = headRB.velocity = handsRB.velocity = 
                legsRB.velocity = Vector2.zero;
        }
    }
}
