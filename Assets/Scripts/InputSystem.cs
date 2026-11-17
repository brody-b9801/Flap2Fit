using System.Collections.Generic;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    [SerializeField] private bool testingWithoutMotion;
    [Tooltip("Upward force added to bird per degree decrease in angle between arm and body")]
    [SerializeField] private float forcePerDegree;
    [Tooltip("Minimum angle decrease (degrees) per frame before force is applied; filters out landmark jitter")]
    [SerializeField] private float flapDeadZone = 1.5f;
    [Tooltip("Wrists closer than this fraction of shoulder width count as a clap (respawn)")]
    [SerializeField] private float clapThreshold = 0.4f;

    private BarrierController barrierController;
    private BirdController cachedBird;
    public static bool clap;

    private BirdController bc
    {
        get
        {
            if (cachedBird == null) cachedBird = barrierController.getBird().GetComponent<BirdController>();
            return cachedBird;
        }
    }
    private bool flapDown = false;
    private bool paused = false;
    private static UIManager uiManager;
    private float avg_prev;

    [System.Serializable]
    private struct landmark
    {
        public float x;
        public float y;
        public float visibility;
        private const float visibilityThreshold = 0.5f;

        public bool visible => visibility >= visibilityThreshold;
    }

    [System.Serializable]
    private struct landmarkContainer
    {
        public bool personDetected;
        public List<landmark> landmarks;
    }

    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        DontDestroyOnLoad(this);
        barrierController = GetComponent<BarrierController>();
    }

    public void OnFlap(InputAction.CallbackContext context)
    {
        if (!testingWithoutMotion) return;
        if (context.started || context.performed) flapDown = true;
        else if (context.canceled) flapDown = false;
    }

    public void JsonReceiver(string input)
    {
        landmarkContainer parsedJson = JsonUtility.FromJson<landmarkContainer>(input);
        ApplyMotionInput(parsedJson);
    }

    private void ApplyMotionInput(landmarkContainer input)
    {
        if (!input.personDetected)
        {
            if (!paused) Pause(true);
            return;
        }         
        landmark l_wrist = input.landmarks[0];
        landmark r_wrist = input.landmarks[1];
        landmark l_elbow = input.landmarks[2];
        landmark r_elbow = input.landmarks[3];
        landmark l_shoulder = input.landmarks[4];
        landmark r_shoulder = input.landmarks[5];
        landmark l_hip = input.landmarks[6];
        landmark r_hip = input.landmarks[7];

        if (!l_shoulder.visible || !l_hip.visible || !l_wrist.visible || !l_elbow.visible
            || !r_shoulder.visible || !r_hip.visible || !r_wrist.visible || !r_elbow.visible)
        {
            if (!paused) Pause(true);
            return;
        }
        
        bool justResumed = paused;
        if (paused) Pause(false);
        Vector2 l_shoulderVec = new Vector2(l_shoulder.x, l_shoulder.y);
        Vector2 r_shoulderVec = new Vector2(r_shoulder.x, r_shoulder.y);

        Vector2 l_shoulderToWrist = new Vector2(l_wrist.x, l_wrist.y) - l_shoulderVec;
        Vector2 r_shoulderToWrist = new Vector2(r_wrist.x, r_wrist.y) - r_shoulderVec;
        Vector2 l_shoulderToElbow = new Vector2(l_elbow.x, l_elbow.y) - l_shoulderVec;
        Vector2 r_shoulderToElbow = new Vector2(r_elbow.x, r_elbow.y) - r_shoulderVec;
        Vector2 l_shoulderToHip = new Vector2(l_hip.x, l_hip.y) - l_shoulderVec;
        Vector2 r_shoulderToHip = new Vector2(r_hip.x, r_hip.y) - r_shoulderVec;

        float l_wristAngle = Vector2.Angle(l_shoulderToWrist, l_shoulderToHip);
        float r_wristAngle = Vector2.Angle(r_shoulderToWrist, r_shoulderToHip);
        float l_elbowAngle = Vector2.Angle(l_shoulderToElbow, l_shoulderToHip);
        float r_elbowAngle = Vector2.Angle(r_shoulderToElbow, r_shoulderToHip);

        float l_angle = (l_wristAngle + l_elbowAngle) / 2;
        float r_angle = (r_wristAngle + r_elbowAngle) / 2;
        float avg_angle = (l_angle + r_angle) / 2;

        if (justResumed) avg_prev = avg_angle;

        if (avg_prev - avg_angle > flapDeadZone)
        {
            float force = forcePerDegree * -(avg_angle - avg_prev);
            bc.ApplyForce(force, ForceMode.Impulse);
        }

        float shoulderWidth = (l_shoulderVec - r_shoulderVec).magnitude;
        float wristDist = (new Vector2(l_wrist.x, l_wrist.y) - new Vector2(r_wrist.x, r_wrist.y)).magnitude;
        clap = wristDist < shoulderWidth * clapThreshold;

        bc.SetWingAngle(avg_angle);
        avg_prev = avg_angle;
    }

    private void Pause(bool pause)
    {
        paused = pause;
        uiManager.Pause(pause);
    }

    void Update()
    {
        if (testingWithoutMotion && flapDown) bc.ApplyForce(20f * Time.deltaTime * 60f);
    }
}
