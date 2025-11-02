# Situational Awareness Game (Collaborative MR Prototype)

This repository contains a collaborative mixed reality (MR) game designed for the Microsoft HoloLens 2.  
The prototype enables two users to share a synchronized game environment, allowing real-time interaction through CogniLink networking.


## Project Overview

This prototype requires two Unity projects to run:

| Project | Role | Platform |
|----------|------|-----------|
| **SituationalAwarenessGame** | Client game running on HoloLens 2 devices | HoloLens 2 |
| **NetworkingSA** | Server handling data synchronization | Local PC |

---

## NetworkingSA Setup

### 1. Open the Project
- Launch Unity and open the `NetworkingSA` project.  
- Load the **SampleScene**.

---

## Setting up CogniLink

### Getting Started
This Unity plugin has been tested with **Unity 2022.3.45f1 LTS**, but it can work with older LTS versions.  
For best results, start from a new project in Unity Hub using the **3D (Built-In Render Pipeline)** template.

### 1. Clone the Repository

### 2. Create a New Unity Project

### 3. Add the CogniLink Library

Copy the CogniLink folder into the /Assets/ directory of your Unity project.

Device Configuration

In the Unity Assets folder, right-click and select:
Create → CogniLink → Device Configuration.

Enter the IP addresses of both HoloLens 2 devices in the Inspector.

Open the CogniLink window and ensure that:

The Device Configuration you created is added and visible in the window.

Both HoloLens 2 device IP addresses are correctly listed.

In the Scene Builder window, select the configuration.
The plugin will attempt to connect to the companion app using this IP information.

## Scene Setup in NetworkingSA

Before launching the server, verify the following in the Unity hierarchy:

Ensure there is an AnchorParent GameObject in your scene.

The AnchorParent must have a child GameObject named GameBoard.
This ensures that the shared anchor and synchronized content are properly initialized.

## HoloLens 2 Companion App Setup

The CogniLink Companion App must be installed on your HoloLens 2 to enable communication.

### 1. Create a New Project

Follow Microsoft’s MRTK setup guide:
Setup MRTK3 Project → Microsoft Docs

Use:

3D (Built-In Render Pipeline)

The same organization and naming conventions as before.

### 2. Add Required Components

Add the CogniLink folder to the Assets directory.

Create a new Empty GameObject and attach SocketClient.cs.

Create another Empty GameObject, name it AnchorParent, and attach ARAnchor.cs.

Create test prefabs and assign them to the appropriate fields in the Inspector.

### 3. Install Mixed Reality Feature Tool (MRFT)

Follow Microsoft’s MRFT guide:
Mixed Reality Feature Tool → Microsoft Docs

Steps:

Launch MRFT and set the Project Path to your Unity project folder.

Click Discover Features.

Select:

Platform Support → Mixed Reality Scene Understanding

Platform Support → OpenXR

MRTK3 → Select All Features

Click Get Features, then Import, and Approve.

(Optional) Exit MRFT. The required libraries will be added automatically.

## SituationalAwarenessGame Setup

Open the SituationalAwarenessGame project in Unity.

Load the Game Scene.

Open GameBoard.cs and update the following line with your HoloLens 2 IP address:

_gazeInfoObject.address = "YOUR_HOLOLENS_IP";


Ensure that:

The PC server and both HoloLens 2 devices are connected to the same network or router.

## Gaze Configuration

To toggle between gaze-enabled and no-gaze versions:

Disable Peer Gaze Visualization

Navigate to:
Assets > Resources > Prefabs > GameBoard

Open the PeerGaze prefab, expand the Gaze Marker (Sphere), and disable its Mesh Renderer and Trail Renderer.

Disable Participant’s Own Gaze Cursor

Select the Gaze Cursor prefab.

Disable its Mesh Renderer.

## Game Launch Order

When starting the collaborative session, follow the exact launch order to ensure proper synchronization:

Start the applications on both HoloLens 2 devices.

On your PC, press the Play button in the Unity Editor for the NetworkingSA project.

In the CogniLink window (in the NetworkingSA project), press the "Start Scene Sync" button.

Ensure that both HoloLens 2 IP addresses are listed in the Device Configuration and that the configuration is included in the CogniLink window before starting synchronization.

## Game Play Instructions
### Collaborative Game Mode

Build and deploy the SituationalAwarenessGame project to both HoloLens 2 devices.

Stand facing the direction where you want the grid to appear.

Follow the Game Launch Order listed above.

Once synchronization completes, both players should see the shared, synchronized grid in their MR environment.

## Individual Game Mode

Open the SAG-Ind project in Unity.

Build the “game” scene to your HoloLens 2 device.

Launch and play individually.
