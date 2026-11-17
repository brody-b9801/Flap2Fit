# Flap2Fit

A motion-controller Unity/C# game with a custom WebGL template and MediaPipe pose tracking

<a href="https://brody-b9801.itch.io/flap2fit">
  <img src="https://img.shields.io/badge/Play%20on-itch.io-FA5C5C?style=for-the-badge&logo=itchdotio&logoColor=white" alt="Play on itch.io" width="280">
</a>

---

## Gameplay Demo

<p align="center">
  
  https://github.com/user-attachments/assets/3391f45c-27e1-40bb-90be-c8c643206e88
  
</p>

## About

Flap2Fit is an endless arcade game where your body is the controller. Your bat flies through a cave of barriers, and the only way to stay in the air is to flap your arms. The longer you survive, the faster it gets.

## Controls

- **Flap your arms** to make the bat flap and fly up, avoiding barriers
- **Clap** to start the game and to respawn

## How It Works

- **Pose tracking:** [MediaPipe Pose Landmarker](https://ai.google.dev/edge/mediapipe/solutions/vision/pose_landmarker) runs in the browser and tracks your shoulders, elbows, wrists and hips.
- **Custom WebGL template:** the page captures the webcam, draws the tracked landmarks over the video and streams them to Unity as JSON.
- **Motion input:** Unity turns arm angles into wing poses. It smooths out tracking jitter and converts each downstroke into a capped physics impulse.
- **Gestures:** bringing your wrists together is detected as a clap for menu navigation.

## The Stack

- Unity (C#)
- MediaPipe Pose Detection (JavaScript)
- HTML / CSS / JS (Custom WebGL template)

## Running Locally

1. Open the project in Unity and make a Web build using the **Flap2Fit - WebGL Template**.
2. Serve the build folder over HTTP:
   ```bash
   python -m http.server 8000
   ```
3. Open `http://localhost:8000`, click **Start Webcam**, and clap to begin.



