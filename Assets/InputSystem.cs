using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    private BirdController bc;
    private bool flapDown = false;
    [SerializeField] private bool testingWithoutMotion;

    void Start()
    {
        bc = GetComponent<BirdController>();
    }

    public void OnFlap(InputAction.CallbackContext context)
    {
        if (!testingWithoutMotion) return;
        if (context.started || context.performed) flapDown = true;
        else if (context.canceled) flapDown = false;
    }



    void Update()
    {
        if (flapDown) bc.ApplyForce(5f);
    }
}
