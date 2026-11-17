using UnityEngine;

public class BirdController : MonoBehaviour
{
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private GameObject armL;
    [SerializeField] private GameObject armR;
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

    public void ApplyForce(float force, ForceMode mode = ForceMode.Force)
    {
        if (!dead)
            rb.AddForce(force * Vector3.up, mode);
    }

    public void SetWingAngle(float angle)
    {
        if (!dead)
        {
            armL.transform.localRotation = Quaternion.Euler(new Vector3(-67.942f, angle - 40, 66.544f));
            armR.transform.localRotation = Quaternion.Euler(new Vector3(-67.942f, angle - 90, -66.544f));
        }

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
