# CogniLink
CogniLink is designed as a multi-platform system to share arbitrary, but spatially-synched, information. CogniLink exhanges spatial anchors between two or more AR devices (right now, full functionality is only for HoloLens 2 devices, but other devices are being added). An arbitray data object is exchanged and cyncronized between the devices, which can be modified to fit the needs of the specific applications. 


# Getting Started
This Unity plugin is currently being developed using Unity 2022.3.45f1 LTS, but can work on older versions. 
It's best to start from a new project using Unity Hub using the default 3D (Built-In render Pipeline) 
project.

### Clone repo
1) Open Git Bash
2) navigate to parent directory for where you want the repo
3) git clone --recurse

## CogniLink Unity Hub Install Instructions
1) Install Unity
2) Make New Project
    - Unity > New Project 
        - Select 3D Built-in-render Pipeline        
        - Fillout fields: Project Name | Folder location | Organization
        - Create Project
3) Add CogniLink library to /Assets/ Folder in Unity Project

### Device Configuration
To set the IP address, add a device configuration in the Assets folder by right-clicking in the Assets 
folder and clicking Create > CogniLink > Device Configuration.  Multiple configurations can be created 
so that devices can be swapped at ease. The IP address can be changed in the Unity inspector tab. Once 
created, go to the scene builder window and select the configuration. The current IP will be reflected in 
Device Window. Once a configuration is selected, the plugin will attempt to connect to the companion app.


## Connecting to the Companion CogniLink App
The IP address of the device(s) needs to be known for a connection to occur. 

### HoloLens 2 Companion App Install Instructions
1) Install Unity
2) Make New Project using [https://learn.microsoft.com/en-us/windows/mixed-reality/mrtk-unity/mrtk3-overview/getting-started/setting-up/setup-new-project]
    - Unity > New Project 
        - Select 3D Built-in-render Pipeline        
        - Fillout fields: Project Name | Folder location | Organization
        - Create Project
3) Install <a href="[url](https://www.microsoft.com/en-us/download/details.aspx?id=102778)">Mixed Reality Feature Tool</a>
4) See below for MRFT specific instructions
5) Add CogniLink Folder to Asset Folder
6) Add a new generic GameObject (Right click under Scene > Create Empty)
	- Add SocketClient.cs
7) Add another generic GameObject and name it "AnchorParent"
 	- Add ARAnchor.cs
8) Create a prefab assets to be created as a test, and add to appropriate field


## Mixed Reality Feature Tool
For more information and to download see:  
[https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool](https://learn.microsoft.com/en-us/windows/mixed-reality/develop/unity/welcome-to-mr-feature-tool)  
- For the Project Path, navigate to the location of the Unity project
- Click Discover Features
- On the Discover Features page, select 
	o Platform Support > Mixed Reality Scene Understanding
	o Platform Support > OpenXR
	o MRTK3 > Click Button to Select All Features
- Click Get Features
- Click Import
- Click Approve
- Click Exit (optional)
The required libraries will load into the Unity project.

Open the Unity Project for the companion app and finish and steps to prepare the project for install on an HL2




