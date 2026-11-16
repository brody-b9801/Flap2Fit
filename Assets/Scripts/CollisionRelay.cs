using UnityEngine;

public class CollisionRelay : MonoBehaviour
{
    [SerializeField] private BirdController bc;
    void OnCollisionEnter(Collision collision)
    {
        bc.Die();
    }
}
