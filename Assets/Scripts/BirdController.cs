using UnityEngine;

public class BirdController : MonoBehaviour
{
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

    // LateUpdate runs after physics/movement, so the camera never lags a frame behind.
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
        if (!dead) Die();
    }

    public void Die()
    {
        dead = true;
        rb.isKinematic = true;
        ScoreIncrementer.Die();
    }
}
