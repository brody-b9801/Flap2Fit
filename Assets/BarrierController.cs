using UnityEngine;
using UnityEngine.Serialization;

public class BarrierController : MonoBehaviour
{
    [Tooltip("Number of barriers to render at a time")]
    [Range(2, 10)]
    [SerializeField, FormerlySerializedAs("numPipes")] private int numBarriers;
    [Tooltip("z value, distance in camera forward direction")]
    [SerializeField, FormerlySerializedAs("distanceBetweenPipes")] private float distanceBetweenBarriers;
    [Tooltip("Vertical gap")]
    [SerializeField, FormerlySerializedAs("gapBetweenPipes")] private float gapBetweenBarriers;
    [Tooltip("z value, assumes player starts at (0,0,0)")]
    [SerializeField, FormerlySerializedAs("pipeHeight")] private float barrierHeight;
    [Tooltip("the min/max height of barriers")]
    [SerializeField, FormerlySerializedAs("pipeHeightDistribution")] private float barrierHeightDistribution;
    [SerializeField] private float startDistFromPlayer;
    [SerializeField, FormerlySerializedAs("pipePrefab")] private GameObject barrierPrefab;
    [SerializeField, FormerlySerializedAs("pipeInitMoveSpeed")] private float barrierInitMoveSpeed;
    [SerializeField] private GameObject playerPrefab;

    struct barrierInfo
    {
        public Transform barrier;
        public Vector3 currPose;
        public Vector3 prevPose;

        public barrierInfo(Transform barrier, Vector3 currPose)
        {
            this.barrier = barrier;
            this.currPose = currPose;
            prevPose = Vector3.negativeInfinity;
        }
    }

    private barrierInfo[] barriers;
    private Vector3 initialBarrierPose;
    private Transform bird;
    private Transform cam;
    private float barrierMoveSpeed;
    private float initialBarrierOffset;

    private float getRandHeight() => Random.Range(-barrierHeightDistribution, barrierHeightDistribution);
    void Awake()
    {
        cam = Camera.main.transform;

        barriers = new barrierInfo[numBarriers];
        initialBarrierPose = Vector3.forward * startDistFromPlayer;
        for (uint i = 0; i < numBarriers; i++)
        {
            Vector3 curr;
            if (i == 0) {
                initialBarrierOffset = getRandHeight();
                curr = initialBarrierPose + Vector3.forward * (distanceBetweenBarriers * i) + Vector3.up * initialBarrierOffset;
            } else {
                curr = initialBarrierPose + Vector3.forward * (distanceBetweenBarriers * i) + Vector3.up * getRandHeight();
            }

            barriers[i] = new barrierInfo(
                Instantiate(barrierPrefab, curr, Quaternion.identity).transform, 
                curr
            );            
            Transform top = barriers[i].barrier.GetChild(0).transform;
            top.position = top.position + Vector3.up * (barrierHeight + gapBetweenBarriers);
            top.localScale = new Vector3(top.localScale.x, barrierHeight, top.localScale.z); 

            Transform bottom = barriers[i].barrier.GetChild(1).transform;
            bottom.localScale = new Vector3(bottom.localScale.x, barrierHeight, bottom.localScale.z);
        }
        barrierMoveSpeed = barrierInitMoveSpeed;
        Renderer topR = barriers[0].barrier.GetChild(0).GetComponent<Renderer>();
        Renderer bottomR = barriers[0].barrier.GetChild(1).GetComponent<Renderer>();
        float gapCenter = (bottomR.bounds.max.y + topR.bounds.min.y) * 0.5f;

        bird = Instantiate(playerPrefab, Vector3.up * gapCenter, Quaternion.identity).transform;
     }

    void Update()
    {
        float t = (Time.time - Time.fixedTime) / Time.fixedDeltaTime;
        for (int i = 0; i < numBarriers; i++)
        {
            ref barrierInfo currBarrier = ref barriers[i];
            currBarrier.barrier.position = Vector3.Lerp(currBarrier.prevPose, currBarrier.currPose, t);

            if (currBarrier.barrier.position.z < -distanceBetweenBarriers)
            {
                int lastBarrierIndex = (i - 1 + numBarriers) % numBarriers;
                Vector3 newPos = barriers[lastBarrierIndex].barrier.position + Vector3.forward * distanceBetweenBarriers;
                newPos.y = getRandHeight();
                currBarrier.barrier.position = newPos;
                currBarrier.currPose = newPos;
                currBarrier.prevPose = newPos;
            }
        }
    }

    void FixedUpdate()
    {
        cam.position = bird.position - Vector3.forward * distanceBetweenBarriers * 1.5f;
        for (uint i = 0; i < numBarriers; i++)
        {
            ref barrierInfo currBarrier = ref barriers[i];
            currBarrier.prevPose = currBarrier.currPose;
            currBarrier.currPose += Vector3.forward * (-barrierMoveSpeed * Time.fixedDeltaTime);
        }
    }
}
