using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Transactions;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SledsController : MonoBehaviour
{
    private const int SEGS_COUNT = 50;

    public bool DirectedRight => rb.velocity.x >= 0;
    public Vector3 LastCheckpoint => lastCheckpoint;

    [SerializeField] private MeshFilter coverMeshFilter;
    [SerializeField] private MeshFilter sidesMeshFilter;
    [SerializeField] private CircleCollider2D startCol;
    [SerializeField] private CircleCollider2D endCol;
    [SerializeField] private CapsuleCollider capsuleCollider;
    [SerializeField] private Rigidbody capsuleRigidbody;
    [SerializeField] private float width;
    [SerializeField] private float thickness;
    [SerializeField] private PhysicsMaterial2D sledsPhysicsMat;
    [SerializeField] private SimpleRagdollController ragdollFab;
    [SerializeField] private TextMeshPro nicknameFab;

    private Rigidbody2D rb;
    private Mesh coverMesh;
    private Mesh sidesMesh;
    private SimpleRagdollController ragdoll;
    private bool isPlayer;
    private TextMeshPro nickname;
    
    //for mesh generation
    private Vector3[] coverVerts;
    private int[] coverTris;
    private Vector3[] sidesVerts;
    private int[] sidesTris;
    private CircleCollider2D[] colliders;
    private Vector2 massCenter;
    private bool firstDrawn;
    private Vector3 lastCheckpoint;
    private Coroutine speedRoutine;
    private Vector2 speedDir;
    private bool turnedRight;
    private bool endAtRight => endCol.transform.position.x > startCol.transform.position.x;

    private Vector3[] positions;
    private Vector3[] startPositions;
    private Vector3[] targetPositions;

    private float morph = 0f;

    public void StartRace(int layerIndex, string nick = "")
    {
        if (firstDrawn) return;
        
        gameObject.layer = layerIndex;
        startCol.gameObject.layer = layerIndex;
        endCol.gameObject.layer = layerIndex;
        capsuleCollider.gameObject.layer = layerIndex;
        rb.isKinematic = false;
        ragdoll = Instantiate(ragdollFab, transform);
        ragdoll.Init(rb, layerIndex);

        isPlayer = layerIndex == LayerMask.NameToLayer("Player"); 
        if (isPlayer)
            ragdoll.Head.onHeadBanged = GameController.Restart;
        else
            ragdoll.Head.onHeadBanged = ResetToLastCheckpoint;

        if (!isPlayer)
        {
            nickname = Instantiate(nicknameFab);
            nickname.text = nick;
        }
    }

    public void FinishReached()
    {
        //rb.drag = 1f;
        GameController.Finished(isPlayer);
    }

    public void SetCheckpoint(Vector3 position)
    {
        lastCheckpoint = position;
    }

    public void ResetToLastCheckpoint()
    {
        rb.simulated = false;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        ragdoll.MoveToDefault(.5f, turnedRight);
        transform.DOScale(Vector3.zero, .25f).SetEase(Ease.OutBack).OnComplete(() =>
        {
            rb.position = lastCheckpoint;
            transform.position = lastCheckpoint;
            transform.eulerAngles = Vector3.zero;
            transform.DOScale(Vector3.one, .25f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                rb.simulated = true;
            });
        });
    }

    public void SpeedUp(Vector2 direction)
    {
        speedDir = direction;
        if (speedRoutine != null) StopCoroutine(speedRoutine);
        speedRoutine = StartCoroutine(SpeedUpRoutine());
    }

    public void SetDrawnData(Vector2[] posArray, float w, float h, bool toRight)
    {
        turnedRight = toRight;
        
        float mass = 0;
        for (int i = 0; i < posArray.Length - 1; i++)
            mass += Vector3.Distance(posArray[i], posArray[i + 1]);
        rb.mass = 10 + mass * 20;
        massCenter = Vector2.up * ((-h + thickness) / 2f);

        Vector3[] points = GetSmoothedPoints(posArray, SEGS_COUNT);
        startPositions = positions;
        targetPositions = points;
        
        capsuleCollider.height = w;
        capsuleCollider.radius = h;
        capsuleCollider.transform.localEulerAngles = -Vector3.forward * transform.localEulerAngles.z;
        
        if (!firstDrawn)
        {
            startPositions = targetPositions;
            Rebuild();
            firstDrawn = true;
        }
        else
        {
            morph = 0f;
            DOTween.To(() => morph, val =>
            {
                morph = val;
                Rebuild();
            }, 1f, 1f).SetEase(Ease.OutExpo);
            float endRot = 0f;
            if (rb.rotation > 180f) endRot = 360f;
            rb.DORotate(endRot, 1f).SetEase(Ease.OutExpo);
        }
        
        ragdoll.MoveToDefault(.5f, toRight);
        UpdateRagdoll();
    }

    private void Rebuild()
    {
        for (int i = 0; i < positions.Length; i++)
            positions[i] = Vector3.Lerp(startPositions[i], targetPositions[i], morph);

        int lastPos = positions.Length - 1;
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 pos = positions[i];
            
            var dif = i < positions.Length - 1 ?
                positions[i+1] - positions[i] :
                positions[i] - positions[i-1];

            var dir = Vector3.forward;
            if (i == 0) dir = positions[i + 2] - positions[i];
            if (i > 0 && i < positions.Length - 1) dir = positions[i + 1] - positions[i - 1];
            if (i == positions.Length - 1) dir = positions[i] - positions[i - 2];
            
            Vector3 firstCross = pos + new Vector3(-dir.y, dir.x).normalized * thickness;
            Vector3 secondCross = pos + new Vector3(dir.y, -dir.x).normalized * thickness;
            
            if (i != lastPos && i > 0)
                colliders[i].offset = pos;

            //-=-=-=-=-=-=-=-=-= FILL COVER -=-=-=-=-=-=-=-=-=-=-
            //First two verts reserved for tail
            if (i == 0)
            {
                coverVerts[0] = pos - dif.normalized * thickness + Vector3.back * width;
                coverVerts[1] = pos - dif.normalized * thickness + Vector3.forward * width;
                coverTris[0] = 0;
                coverTris[1] = 1;
                coverTris[2] = 2;
                coverTris[3] = 2;
                coverTris[4] = 1;
                coverTris[5] = 3;
                coverTris[coverTris.Length - 1] = 1;
                coverTris[coverTris.Length - 2] = coverVerts.Length - 1;
                coverTris[coverTris.Length - 3] = 0;
                coverTris[coverTris.Length - 4] = 0;
                coverTris[coverTris.Length - 5] = coverVerts.Length - 1;
                coverTris[coverTris.Length - 6] = coverVerts.Length - 2;
            }
            
            //Last two for tail too
            if (i == lastPos)
            {
                coverVerts[4 + lastPos * 2] = pos + dif.normalized * thickness + Vector3.back * width;
                coverVerts[5 + lastPos * 2] = pos + dif.normalized * thickness + Vector3.forward * width;
                coverTris[6 + lastPos * 6] = 2 + lastPos * 2;
                coverTris[7 + lastPos * 6] = 3 + lastPos * 2;
                coverTris[8 + lastPos * 6] = 4 + lastPos * 2;
                coverTris[9 + lastPos * 6] = 4 + lastPos * 2;
                coverTris[10 + lastPos * 6] = 3 + lastPos * 2;
                coverTris[11 + lastPos * 6] = 5 + lastPos * 2;

                coverTris[12 + lastPos * 6] = 4 + lastPos * 2;
                coverTris[13 + lastPos * 6] = 5 + lastPos * 2;
                coverTris[14 + lastPos * 6] = 6 + lastPos * 2;
                coverTris[15 + lastPos * 6] = 6 + lastPos * 2;
                coverTris[16 + lastPos * 6] = 5 + lastPos * 2;
                coverTris[17 + lastPos * 6] = 7 + lastPos * 2;
            }
            
            coverVerts[2+i*2] = firstCross + Vector3.back * width;
            coverVerts[3+i*2] = firstCross + Vector3.forward * width;
            coverTris[6 + i*6] = 2 + i*2;
            coverTris[7 + i*6] = 3 + i*2;
            coverTris[8 + i*6] = 4 + i*2;
            coverTris[9 + i*6] = 4 + i*2;
            coverTris[10 + i*6] = 3 + i*2;
            coverTris[11 + i*6] = 5 + i*2;
            
            coverVerts[coverVerts.Length-i*2-2] = secondCross + Vector3.back * width;
            coverVerts[coverVerts.Length-i*2-1] = secondCross + Vector3.forward * width;
            coverTris[coverTris.Length - 12 - i * 6] = coverVerts.Length - 4 - i*2;
            coverTris[coverTris.Length - 11 - i * 6] = coverVerts.Length - 3 - i*2;
            coverTris[coverTris.Length - 10 - i * 6] = coverVerts.Length - 2 - i*2;
            coverTris[coverTris.Length - 9 - i * 6] = coverVerts.Length - 2 - i*2;
            coverTris[coverTris.Length - 8 - i * 6] = coverVerts.Length - 3 - i*2;
            coverTris[coverTris.Length - 7 - i * 6] = coverVerts.Length - 1 - i*2;
            //-=-=-=-=-=-=-=-=-= END FILL COVER -=-=-=-=-=-=-=-=-=-=-
            
            //-=-=-=-=-=-=-=-=-= FILL SIDES -=-=-=-=-=-=-=-=-=-=-
            if (i == 0)
            {
                sidesVerts[0] = pos - dif.normalized * thickness;
                sidesTris[0] = 0;
                sidesTris[1] = 1;
                sidesTris[2] = 2;
            }

            if (i == lastPos)
            {
                sidesVerts[sidesVerts.Length - 1] = pos + dif.normalized * thickness;
                sidesTris[sidesTris.Length - 3] = sidesVerts.Length - 2;
                sidesTris[sidesTris.Length - 2] = sidesVerts.Length - 3;
                sidesTris[sidesTris.Length - 1] = sidesVerts.Length - 1;
            }
            else
            {
                sidesTris[3 + i * 6] = 2 + i * 2;
                sidesTris[4 + i * 6] = 1 + i * 2;
                sidesTris[5 + i * 6] = 4 + i * 2;
                sidesTris[6 + i * 6] = 1 + i * 2;
                sidesTris[7 + i * 6] = 3 + i * 2;
                sidesTris[8 + i * 6] = 4 + i * 2;
            }
            sidesVerts[1 + i * 2] = firstCross;
            sidesVerts[2 + i * 2] = secondCross;
        }

        startCol.transform.localPosition = positions[0];
        endCol.transform.localPosition = positions[positions.Length - 1];
        massCenter = new Vector2(0f, massCenter.y);
        rb.centerOfMass = massCenter;
        
        UpdateRagdoll();

        coverMesh.Clear();
        coverMesh.vertices = coverVerts;
        coverMesh.triangles = coverTris;
        coverMesh.RecalculateNormals();
        sidesMesh.Clear();
        sidesMesh.vertices = sidesVerts;
        sidesMesh.triangles = sidesTris;
        sidesMesh.RecalculateNormals();
    }

    private void UpdateRagdoll()
    {
        ragdoll.SledsEnd = (turnedRight ? endAtRight ? endCol : startCol : endAtRight ? startCol : endCol).transform;
        ragdoll.SetAnchor(new Vector2(
            (turnedRight ? endAtRight ? endCol : startCol : endAtRight ? startCol : endCol)
            .transform.localPosition.x + (turnedRight ? -1.2f : 1.2f),
            massCenter.y), ragdoll.SledsEnd.localPosition, turnedRight);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;

        coverMesh = new Mesh();
        coverMesh.MarkDynamic();
        sidesMesh = new Mesh();
        sidesMesh.MarkDynamic();
        coverMeshFilter.mesh = coverMesh;
        sidesMeshFilter.mesh = sidesMesh;
        
        coverVerts = new Vector3[SEGS_COUNT * 4 + 4];
        coverTris = new int[(SEGS_COUNT - 1) * 12 + 24];
        sidesVerts = new Vector3[SEGS_COUNT * 2 + 2];
        sidesTris = new int[(SEGS_COUNT - 1) * 6 + 6];
        
        positions = new Vector3[SEGS_COUNT];
        
        sidesMeshFilter.transform.localPosition = Vector3.back * width;
        startCol.radius = endCol.radius =  thickness - .01f;
        
        colliders = new CircleCollider2D[SEGS_COUNT];
        for (int i = 0; i < SEGS_COUNT; i++)
        {
            colliders[i] = gameObject.AddComponent<CircleCollider2D>();
            colliders[i].radius = thickness;
            colliders[i].sharedMaterial = sledsPhysicsMat;
        }
    }

    private void FixedUpdate()
    {
        capsuleRigidbody.MovePosition((Vector3)rb.position + Vector3.forward * transform.position.z);
        capsuleRigidbody.MoveRotation(Quaternion.Euler(Vector3.forward * rb.rotation));
        capsuleRigidbody.velocity = rb.velocity;
        capsuleRigidbody.angularVelocity = Vector3.forward * rb.angularVelocity;

        if (speedRoutine != null)
            rb.AddForce(speedDir * rb.mass / 2f, ForceMode2D.Impulse);
    }

    private void Update()
    {
        if (nickname != null)
            nickname.transform.position = transform.position + Vector3.up * 2;
    }

    private IEnumerator SpeedUpRoutine()
    {
        yield return new WaitForSeconds(.5f);
        speedRoutine = null;
    }

    #region math
    private static Vector2 VecFromAngle(float angle)
    {
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    
    private static Vector3 VecFromAngle(float angle, float z)
    {
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), z);
    }

    private static Vector3[] GetSmoothedPoints(Vector2[] points, int resolution)
    {
        float[] lengths = new float[points.Length - 1];
        float lineLen = 0f;
        for (int i = 0; i < lengths.Length; i++)
            lineLen += lengths[i] = Vector2.Distance(points[i], points[i + 1]);
        Vector3[] result = new Vector3[resolution];

        for (int i = 0; i < resolution; i++)
        {
            float pos = lineLen * ((float)i / resolution);
            float currentPos = lengths[0];
            int currentPoint = 0;
            while (currentPos < pos && currentPoint < points.Length - 1)
            {
                currentPoint++;
                currentPos += lengths[currentPoint];
            }

            float splineLen = 0f;
            currentPos = currentPoint > 0 ? currentPos - lengths[currentPoint] : 0;
            splineLen = lengths[currentPoint];
            
            result [i] = GetCatmullRomPosition((pos - currentPos) / splineLen,
                points[currentPoint > 0 ? currentPoint - 1 : 0], points[currentPoint],
                points[currentPoint + 1], points[currentPoint + (currentPoint >= points.Length - 2 ? 1 : 2)]);
        }

        return result;
    }
    
    private static Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        //The coefficients of the cubic polynomial (except the 0.5f * which I added later for performance)
        Vector3 a = 2f * p1;
        Vector3 b = p2 - p0;
        Vector3 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
        Vector3 d = -p0 + 3f * p1 - 3f * p2 + p3;

        //The cubic polynomial: a + b * t + c * t^2 + d * t^3
        Vector3 pos = 0.5f * (a + (b * t) + (t * t * c) + (t * t * t * d));

        return pos;
    }
    #endregion
}
