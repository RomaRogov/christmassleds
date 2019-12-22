using DG.Tweening;
using UnityEngine;

public class CheckpointController : MonoBehaviour
{
    public Vector3 SpawnPoint => spawnPoint.position;
    
    [SerializeField] private Transform flag;
    [SerializeField] private Transform spawnPoint;
    private bool checkedIn = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!checkedIn)
        {
            SledsController sleds = other.GetComponent<SledsController>();
            if (sleds == null) return;
            
            flag.DOLocalRotate(Vector3.zero, 1f).SetEase(Ease.OutBack);
            sleds.SetCheckpoint(SpawnPoint);
            checkedIn = true;
        }
    }
}
