import { FilesetResolver, PoseLandmarker } from "https://cdn.jsdelivr.net/npm/@mediapipe/tasks-vision@0.10.14/vision_bundle.mjs";

const video = document.getElementById('webcam');
const startCamButton = document.getElementById('startButton');

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
        video.addEventListener("loadeddata", () => {
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
        ResultToUnity(result, video.currentTime)
    }

    requestAnimationFrame(detectLoop);
}

function ResultToUnity(result, time) {
    const pose = result.landmarks[0];

    const ResultJson = {};
    ResultJson.currentTime = time;
    ResultJson.personDetected = result.landmarks.length === 0;
    ResultJson.landmarks = [];
    if (result.landmarks.length !== 0) {
        ResultJson.landmarks.push({ name: "l_wrist", ...pose[15] });
        ResultJson.landmarks.push({ name: "r_wrist", ...pose[16] });
        ResultJson.landmarks.push({ name: "l_elbow", ...pose[13] });
        ResultJson.landmarks.push({ name: "r_elbow", ...pose[14] });
        ResultJson.landmarks.push({ name: "l_shoulder", ...pose[11] });
        ResultJson.landmarks.push({ name: "r_shoulder", ...pose[12] });
        ResultJson.landmarks.push({ name: "l_hip", ...pose[23] });
        ResultJson.landmarks.push({ name: "r_hip", ...pose[24] });
    }
    
    unityInstance.SendMessage("InputSystem", "JsonReceiver", JSON.stringify(result));
}

startCamButton.addEventListener("click", startWebcam);
