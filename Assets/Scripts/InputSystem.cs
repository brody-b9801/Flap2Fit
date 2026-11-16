using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystem : MonoBehaviour
{
    [SerializeField] private bool testingWithoutMotion;
    [Tooltip("Upward force added to bird per degree decrease in angle between arm and body")]
    [SerializeField] private float forcePerDegree;

    private BirdController bc;
    private bool flapDown = false;
    private bool paused = false;
    private static UIManager uiManager;
    private float avg_prev;
    private float prev_time;

    private struct landmark
    {
        public float X;
        public float Y;
        public float Z;
        public bool visible;
        private const float visibilityThreshold = 0.5f;

        public landmark(float x, float y, float z, float visibility)
        {
            X = x;
            Y = y;
            Z = z;
            visible = visibility >= visibilityThreshold;
        }
    }

    private struct landmarkContainer
    {
        public float currTime;
        public bool playerVisible;
        public List<landmark> landmarks;
    }

    void Start()
    {
        uiManager = FindFirstObjectByType<UIManager>();
        DontDestroyOnLoad(this);
        bc = GetComponent<BirdController>();
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
        if (!input.playerVisible)
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
            Pause(true);
            return;
        }
        
        if (paused) Pause(false);
        Vector3 l_shoulderVec = new Vector3(l_shoulder.X, l_shoulder.Y, l_shoulder.Z);
        Vector3 r_shoulderVec = new Vector3(r_shoulder.X, r_shoulder.Y, r_shoulder.Z);

        Vector3 l_shoulderToWrist = new Vector3(l_wrist.X, l_wrist.Y, l_wrist.Z) - l_shoulderVec;
        Vector3 r_shoulderToWrist = new Vector3(r_wrist.X, r_wrist.Y, r_wrist.Z) - r_shoulderVec;
        Vector3 l_shoulderToElbow = new Vector3(l_elbow.X, l_elbow.Y, l_elbow.Z) - l_shoulderVec;
        Vector3 r_shoulderToElbow = new Vector3(r_elbow.X, r_elbow.Y, r_elbow.Z) - r_shoulderVec;
        Vector3 l_shoulderToHip = new Vector3(l_hip.X, l_hip.Y, l_hip.Z) - l_shoulderVec;
        Vector3 r_shoulderToHip = new Vector3(r_hip.X, r_hip.Y, r_hip.Z) - r_shoulderVec;

        float l_wristAngle = Vector3.Angle(l_shoulderToWrist, l_shoulderToHip);
        float r_wristAngle = Vector3.Angle(r_shoulderToWrist, r_shoulderToHip);
        float l_elbowAngle = Vector3.Angle(l_shoulderToElbow, l_shoulderToHip);
        float r_elbowAngle = Vector3.Angle(r_shoulderToElbow, r_shoulderToHip);

        float l_angle = (l_wristAngle + l_elbowAngle) / 2;
        float r_angle = (r_wristAngle + r_elbowAngle) / 2;
        float avg_angle = (l_angle + r_angle) / 2;

        if (avg_angle < avg_prev)
        {
            bc.ApplyForce(forcePerDegree * -(avg_angle - avg_prev) * 1/(input.currTime - prev_time));
        }

        avg_prev = avg_angle;
        prev_time = input.currTime;
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
