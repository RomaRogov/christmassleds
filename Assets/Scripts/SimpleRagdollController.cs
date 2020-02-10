using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleRagdollController : MonoBehaviour
{
    public RagdollHeadController Head => head.GetComponent<RagdollHeadController>();
    public Transform SledsEnd;

    [SerializeField] private Transform body;
    [SerializeField] private Transform head;
    [SerializeField] private Transform hands;
    [SerializeField] private Transform legs;
    [SerializeField] private GameObject legsLimiter;
    [SerializeField] private LineRenderer rigForward;
    [SerializeField] private LineRenderer rigBack;
    [SerializeField] private Transform palm;
    [SerializeField] private LineRenderer scarf;
    [SerializeField] private Transform[] scarfPoints;
    private FixedJoint2D assJoint;
    private SpringJoint2D handJoint;

    private Vector3 bodyStartPos;
    private Vector3 headStartPos;
    private Vector3 handsStartPos;
    private Vector3 legsStartPos;

    private Rigidbody2D bodyRB;
    private Rigidbody2D headRB;
    private Rigidbody2D handsRB;
    private Rigidbody2D legsRB;

    private Rigidbody2D sledsRB;
    
    public void Init(Rigidbody2D parent, int layer)
    {
        assJoint = legs.GetComponent<FixedJoint2D>();
        handJoint = hands.GetComponent<SpringJoint2D>();
        sledsRB = parent;

        bool isPlayer = layer == LayerMask.NameToLayer("Player");
        int ragrollLayer = isPlayer ? LayerMask.NameToLayer("RagdollPlayer") : LayerMask.NameToLayer("RagdollFoe");
        int legLimiterLayer = isPlayer ? 
            LayerMask.NameToLayer("LegsLimiterPlayer") : 
            LayerMask.NameToLayer("LegsLimiterFoe");
        
        body.gameObject.layer = ragrollLayer;
        head.gameObject.layer = ragrollLayer;
        hands.gameObject.layer = ragrollLayer;
        legs.gameObject.layer = layer;
        legsLimiter.layer = legLimiterLayer;

        headStartPos = head.localPosition;
        handsStartPos = hands.localPosition;
        legsStartPos = legs.localPosition;

        bodyRB = body.GetComponent<Rigidbody2D>();
        headRB = head.GetComponent<Rigidbody2D>();
        handsRB = hands.GetComponent<Rigidbody2D>();
        legsRB = legs.GetComponent<Rigidbody2D>();

        scarf.material.color = isPlayer ? GameController.ScarfColor : Random.ColorHSV();
    }

    public void MoveToDefault(float time, bool isRight)
    {
        SwitchRigidbodies(false);

        body.DOLocalMove(bodyStartPos, time).SetEase(Ease.OutExpo);
        head.DOLocalMove(headStartPos, time).SetEase(Ease.OutExpo);
        hands.DOLocalMove(handsStartPos, time).SetEase(Ease.OutExpo);
        legs.DOLocalMove(legsStartPos, time).SetEase(Ease.OutExpo);
        
        body.DORotate(Vector3.zero, time).SetEase(Ease.OutExpo);
        head.DORotate(Vector3.zero, time).SetEase(Ease.OutExpo);
        hands.DORotate(Vector3.zero, time).SetEase(Ease.OutExpo);
        legs.DORotate(Vector3.zero, time).SetEase(Ease.OutExpo);
        
        transform.DOScale(new Vector3(isRight ? 1f : -1f, 1f, 1f), time)
            .SetEase(Ease.OutExpo).OnComplete(() =>
        {
            SwitchRigidbodies(true);
        });
    }

    public void SetAnchor(Vector2 assPos, Vector2 handsPos, bool turnedRight)
    {
        if (!bodyRB.simulated)
            transform.localPosition = assPos;
        
        assJoint.connectedBody = sledsRB;
        assJoint.connectedAnchor = assPos;// + (turnedRight ? Vector2.right : Vector2.left) * assJoint.anchor.x;
        //assJoint.anchor = Vector2.zero;

        handJoint.connectedBody = sledsRB;
        handJoint.connectedAnchor = handsPos;
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
    
    private void SwitchRigidbodies(bool enabled)
    {
        bodyRB.simulated = enabled;
        headRB.simulated = enabled;
        handsRB.simulated = enabled;
        legsRB.simulated = enabled;
        
        //Array.ForEach(GetComponentsInChildren<Joint2D>(), joint => joint.enabled = enabled);

        if (!enabled)
        {
            bodyRB.velocity = headRB.velocity = handsRB.velocity = 
                legsRB.velocity = Vector2.zero;
        }
    }
}
