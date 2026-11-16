using UnityEngine;

public class BirdController : MonoBehaviour
{
    [SerializeField] private GameObject impactPrefab;
    private Rigidbody rb;
    private bool dead = false;

    public bool isDead() => dead;
    public void respawn() {
        dead = false;
        rb.isKinematic = false;
    }

    private Transform cam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (Camera.main) cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!cam) return;
        Vector3 p = cam.position;
        p.y = transform.position.y;
        cam.position = p;
    }

    public void ApplyForce(float force)
    {
        if (!dead)
            rb.AddForce(force * Vector3.up);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!dead) { 
            Die();
            Instantiate(impactPrefab, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));        
        }
    }

    public void Die()
    {
        dead = true;
        rb.isKinematic = true;
        ScoreIncrementer.Die();
    }
}
