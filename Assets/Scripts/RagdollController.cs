using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class RagdollController : MonoBehaviour
{
    public RagdollHeadController Head;
    public Rigidbody2D Hand;
    public HingeJoint2D Sit;
    
    [SerializeField] private Rigidbody2D hip;
    [SerializeField] private Rigidbody2D head;
    [SerializeField] private Rigidbody2D arm1;
    [SerializeField] private Rigidbody2D arm2;
    [SerializeField] private Rigidbody2D leg1;
    [SerializeField] private Rigidbody2D leg2;
    [SerializeField] private Transform targetHip;
    [SerializeField] private Transform targetHead;
    [SerializeField] private Transform arm1_back;
    [SerializeField] private Transform arm1_front;
    [SerializeField] private Transform arm2_back;
    [SerializeField] private Transform arm2_front;
    [SerializeField] private Transform leg1_back;
    [SerializeField] private Transform leg1_front;
    [SerializeField] private Transform leg2_back;
    [SerializeField] private Transform leg2_front;

    private Vector3 prevPos;
    private Vector3 currPos;

    private Vector2 hipStartPos;
    private float hipStartRot;
    private Vector2 headStartPos;
    private float headStartRot;
    private Vector2 arm1StartPos;
    private float arm1StartRot;
    private Vector2 arm2StartPos;
    private float arm2StartRot;
    private Vector2 leg1StartPos;
    private float leg1StartRot;
    private Vector2 leg2StartPos;
    private float leg2StartRot;

    public void SetLayer(int layer)
    {
        hip.gameObject.layer = layer;
        head.gameObject.layer = layer;
        arm1.gameObject.layer = layer;
        arm2.gameObject.layer = layer;
        //leg1.gameObject.layer = layer;
        //leg2.gameObject.layer = layer;
        
        hipStartPos = hip.transform.localPosition;
        hipStartRot = hip.transform.localEulerAngles.z;
        headStartPos = head.transform.localPosition;
        headStartRot = head.transform.localEulerAngles.z;
        arm1StartPos = arm1.transform.localPosition;
        arm1StartRot = arm1.transform.localEulerAngles.z;
        arm2StartPos = arm2.transform.localPosition;
        arm2StartRot = arm2.transform.localEulerAngles.z;
        leg1StartPos = leg1.transform.localPosition;
        leg1StartRot = leg1.transform.localEulerAngles.z;
        leg2StartPos = leg2.transform.localPosition;
        leg2StartRot = leg2.transform.localEulerAngles.z;
    }

    public void MoveToDefault(float time, bool toRight)
    {
        hip.gameObject.SetActive(false);

        transform.DOScale(new Vector3(toRight ? -1 : 1, 1, 1), time).SetEase(Ease.OutExpo);
        hip.transform.DOLocalMove(hipStartPos, time);
        hip.transform.DOLocalRotate(Vector3.forward * hipStartRot, time);
        head.transform.DOLocalMove(headStartPos, time);
        head.transform.DOLocalRotate(Vector3.forward * headStartRot, time);
        arm1.transform.DOLocalMove(arm1StartPos, time);
        arm1.transform.DOLocalRotate(Vector3.forward * arm1StartRot, time);
        arm2.transform.DOLocalMove(arm2StartPos, time);
        arm2.transform.DOLocalRotate(Vector3.forward * arm2StartRot, time);
        leg1.transform.DOLocalMove(leg1StartPos, time);
        leg1.transform.DOLocalRotate(Vector3.forward * leg1StartRot, time);
        leg2.transform.DOLocalMove(leg2StartPos, time);
        leg2.transform.DOLocalRotate(Vector3.forward * leg2StartRot, time).OnComplete(() =>
        {
            hip.gameObject.SetActive(true);
        });
    }
    
    private void Update()
    {
        SetPos(hip, targetHip, false, Vector2.zero);
        SetPos(head, targetHead, false, Vector2.zero);
        SetPos(arm1, arm1_back, true, new Vector2(-20, 90));
        SetPos(arm1, arm1_front, false, new Vector2(-20, 90));
        SetPos(arm2, arm2_back, true, Vector2.zero);
        SetPos(arm2, arm2_front, false, Vector2.zero);
        SetPos(leg1, leg1_back, true, Vector2.right * 180);
        SetPos(leg1, leg1_front, true, Vector2.right * 180);
        SetPos(leg2, leg2_back, true, Vector2.zero);
        SetPos(leg2, leg2_front, true, Vector2.zero);
    }

    private void SetPos(Rigidbody2D from, Transform to, bool reverseRot, Vector2 rot)
    {
        prevPos = to.position;
        currPos = new Vector3(from.position.x, from.position.y, prevPos.z);
        to.position = Vector3.Lerp(prevPos, currPos, Time.deltaTime * 5f);
        prevPos = to.localEulerAngles;
        Quaternion prevRot = to.localRotation;
        Quaternion newRot = Quaternion.Euler(new Vector3(rot.x, rot.y, 
                                                 reverseRot ? -1 : 1) * from.transform.localEulerAngles.z);
        
        to.localRotation = Quaternion.Lerp(prevRot, newRot, Time.deltaTime * 7f);
    }
}
