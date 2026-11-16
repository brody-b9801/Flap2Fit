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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void ApplyForce(float force)
    {
        if (!dead)
            rb.AddForce(force * Vector3.up);
    }

    void OnCollisionEnter(Collision collision)
    {
        dead = true;
        rb.isKinematic = true;
    }
}
