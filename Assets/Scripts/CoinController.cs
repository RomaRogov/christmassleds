using UnityEngine;

public class CoinController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    private MeshRenderer renderer;
    private CircleCollider2D col;
    private bool particlesPlaying = false;
    
    void Start()
    {
        renderer = GetComponent<MeshRenderer>();
        col = GetComponent<CircleCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        col.enabled = false;
        renderer.enabled = false;
        particles.Play();
        particlesPlaying = true;
        if (gameObject.layer != LayerMask.NameToLayer("CollidesWithPlayer")) return;
        GameController.AddCoin();
    }

    private void Update()
    {
        if (particlesPlaying && !particles.IsAlive())
            Destroy(gameObject);
        transform.Rotate(Vector3.up * 3f);
    }
}
