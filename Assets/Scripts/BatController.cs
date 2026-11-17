using UnityEngine;

public class BatController : MonoBehaviour
{
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private GameObject armL;
    [SerializeField] private GameObject armR;
    [Tooltip("Downward acceleration (m/s^2) applied to the bat. Earth is 9.81")]
    [SerializeField] private float gravity = 9.81f;
    [Tooltip("Maximum upward speed (m/s) the bat can reach from flapping")]
    [SerializeField] private float maxRiseSpeed = 6f;
    [Tooltip("Maximum downward speed (m/s) the bat can fall at")]
    [SerializeField] private float maxFallSpeed = 8f;
    [Tooltip("On a flap, downward speed (m/s) is capped to this so a flap always stops a fall")]
    [SerializeField] private float flapFallCancelSpeed = 2f;
    [Tooltip("Axis in the bat's own space the wings flap around (forward = flight direction)")]
    [SerializeField] private Vector3 flapAxis = Vector3.forward;
    [Tooltip("Set to -1 if wings move opposite to your arms")]
    [SerializeField] private float flapDirection = 1f;
    [Tooltip("Arm angle (degrees from body) that maps to the wing's default flat pose")]
    [SerializeField] private float flatArmAngle = 90f;
    [SerializeField] private float minArmAngle = 0f;
    [SerializeField] private float maxArmAngle = 180f;
    private Quaternion armLRest;
    private Quaternion armRRest;
    private Vector3 armLAxis;
    private Vector3 armRAxis;
    private float armLSide;
    private float armRSide;
    private Rigidbody rb;
    private bool dead = false;

    public bool isDead() => dead;
    public void respawn() {
        dead = false;
        rb.isKinematic = false;
    }

    private Transform cam;

    void Awake()
    {
        armLRest = armL.transform.localRotation;
        armRRest = armR.transform.localRotation;
        Vector3 worldAxis = transform.TransformDirection(flapAxis);
        armLAxis = armL.transform.parent.InverseTransformDirection(worldAxis);
        armRAxis = armR.transform.parent.InverseTransformDirection(worldAxis);
        Vector3 side = Vector3.Cross(Vector3.up, worldAxis);
        armLSide = Mathf.Sign(Vector3.Dot(armL.transform.position - transform.position, side));
        armRSide = Mathf.Sign(Vector3.Dot(armR.transform.position - transform.position, side));
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        if (Camera.main) cam = Camera.main.transform;
    }

    void FixedUpdate()
    {
        if (rb.isKinematic) return;
        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
        Vector3 v = rb.linearVelocity;
        float clamped = Mathf.Clamp(v.y, -maxFallSpeed, maxRiseSpeed);
        if (clamped != v.y)
        {
            v.y = clamped;
            rb.linearVelocity = v;
        }
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
        if (dead) return;
        Vector3 vel = rb.linearVelocity;
        if (vel.y < -flapFallCancelSpeed)
        {
            vel.y = -flapFallCancelSpeed;
            rb.linearVelocity = vel;
        }
        rb.AddForce(force * Vector3.up, mode);
    }

    public void SetWingAngle(float leftArmAngle, float rightArmAngle)
    {
        if (dead) return;
        float l = (Mathf.Clamp(leftArmAngle, minArmAngle, maxArmAngle) - flatArmAngle) * flapDirection;
        float r = (Mathf.Clamp(rightArmAngle, minArmAngle, maxArmAngle) - flatArmAngle) * flapDirection;
        armL.transform.localRotation = Quaternion.AngleAxis(l * armLSide, armLAxis) * armLRest;
        armR.transform.localRotation = Quaternion.AngleAxis(r * armRSide, armRAxis) * armRRest;
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
