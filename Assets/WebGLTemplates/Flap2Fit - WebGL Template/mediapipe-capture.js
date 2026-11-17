import { FilesetResolver, PoseLandmarker, DrawingUtils } from "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.14/vision_bundle.mjs";

const video = document.getElementById('webcam');
const startCamButton = document.getElementById('startButton');
const overlay = document.getElementById('overlay');
const overlayCtx = overlay.getContext('2d');
const drawingUtils = new DrawingUtils(overlayCtx);

const TRACKED = new Set([11, 12, 13, 14, 15, 16, 23, 24]);

let webcamRunning = false;
let lastVideoTime = -1;

const vision = await FilesetResolver.forVisionTasks(
    "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.14/wasm"
);

const poseLandmarker = await PoseLandmarker.createFromOptions(vision, {
    baseOptions: {
        modelAssetPath: "https://storage.googleapis.com/mediapipe-models/pose_landmarker/pose_landmarker_lite/float16/1/pose_landmarker_lite.task",
        delegate: "GPU",
    },
    runningMode: "VIDEO",
    numPoses: 1,
});

async function startWebcam() {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
        video.srcObject = stream;
        startCamButton.style.display = "none";
        document.getElementById("unity-canvas").style.visibility = "visible";
        video.addEventListener("loadeddata", () => {
            overlay.width = video.videoWidth;
            overlay.height = video.videoHeight;
            webcamRunning = true;
            requestAnimationFrame(detectLoop);
        }, { once: true });
    } catch (error) {
        console.error(error);
        alert('Could not access the webcam. Please check permissions and try again.');
    }
}

function detectLoop() {
    if (!webcamRunning) return;

    if (video.currentTime !== lastVideoTime) {
        lastVideoTime = video.currentTime;
        const result = poseLandmarker.detectForVideo(video, performance.now());
        DrawPose(result);
        ResultToUnity(result)
    }

    requestAnimationFrame(detectLoop);
}

function DrawPose(result) {
    overlayCtx.clearRect(0, 0, overlay.width, overlay.height);
    for (const pose of result.landmarks) {
        drawingUtils.drawConnectors(pose, PoseLandmarker.POSE_CONNECTIONS, { color: "#ffffff88", lineWidth: 2 });
        drawingUtils.drawLandmarks(pose, {
            color: (data) => TRACKED.has(data.index) ? "#00ff88" : "#ffffff88",
            radius: (data) => TRACKED.has(data.index) ? 5 : 2,
        });
    }
}

function Landmark(name, p) {
    const aspect = video.videoWidth / video.videoHeight;
    return { name, x: p.x * aspect, y: p.y, visibility: p.visibility };
}

function ResultToUnity(result) {
    // Unity may still be loading if the webcam was started early
    if (!window.unityInstance) return;

    const pose = result.landmarks[0];

    const ResultJson = {};
    ResultJson.personDetected = result.landmarks.length > 0;
    ResultJson.landmarks = [];
    if (result.landmarks.length !== 0) {
        ResultJson.landmarks.push(Landmark("l_wrist", pose[15]));
        ResultJson.landmarks.push(Landmark("r_wrist", pose[16]));
        ResultJson.landmarks.push(Landmark("l_elbow", pose[13]));
        ResultJson.landmarks.push(Landmark("r_elbow", pose[14]));
        ResultJson.landmarks.push(Landmark("l_shoulder", pose[11]));
        ResultJson.landmarks.push(Landmark("r_shoulder", pose[12]));
        ResultJson.landmarks.push(Landmark("l_hip", pose[23]));
        ResultJson.landmarks.push(Landmark("r_hip", pose[24]));
    }
    
    unityInstance.SendMessage("GameManager", "JsonReceiver", JSON.stringify(ResultJson));
}

startCamButton.addEventListener("click", startWebcam);
