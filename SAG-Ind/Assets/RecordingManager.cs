// using System;
// using UnityEngine;
// using UnityEngine.UI; 

// public class RecordingManager : MonoBehaviour
// {
//     int recorderId = 1; // Example recorder ID
//     string directory = "D:/Project2/Recordings";
//     string recordName = "Session1";

//     public Button startRecordingButton;
//     public Button stopRecordingButton;
//     public Button startReplayButton;
//     public Button stopReplayButton;

//     private void Start()
//     {

//         startRecordingButton.onClick.AddListener(BeginRecording);
//         stopRecordingButton.onClick.AddListener(StopRecording);
//         startReplayButton.onClick.AddListener(BeginReplay);
//         stopReplayButton.onClick.AddListener(StopReplay);
//         ConfigureParameters();
//         //BeginRecording();
//         //StopRecording();
//         // BeginReplay();
//         // StopReplay();
//     }

//     private void ConfigureParameters()
//     {
//         RecordingPluginWrapper.SetRecordingMaxBufferSize(recorderId, 1024);
//         RecordingPluginWrapper.SetSoundRecordingMaxBufferSize(recorderId, 1024);
//         RecordingPluginWrapper.SetReplayBufferNumber(recorderId, 5);
//         RecordingPluginWrapper.SetReplayBufferStoredTimeInterval(recorderId, 0.5f);
//         Debug.Log("Recording parameters configured.");
//     }

//     private void BeginRecording()
//     {
//         if (RecordingPluginWrapper.CreateNewRecordingFile(recorderId, directory, directory.Length, recordName, recordName.Length))
//         {
//             Debug.Log("Recording started successfully.");
//             // Example of recording an object
//             float[] matrixDTO = new float[16]; // Assume a 4x4 matrix
//             int[] infoDTO = new int[2];       // Placeholder for extra information
//             RecordingPluginWrapper.RecordObjectAtTimestamp(recorderId, "ObjectName", "ObjectName".Length, 123, matrixDTO, Time.time, infoDTO);
//         }
//         else
//         {
//             Debug.LogError("Failed to start recording.");
//         }
//     }

//     private void StopRecording()
//     {
//         if (RecordingPluginWrapper.StopRecording(recorderId))
//         {
//             Debug.Log("Recording stopped successfully.");
//         }
//         else
//         {
//             Debug.LogError("Failed to stop recording.");
//         }
//     }

//     private void BeginReplay()
//     {
//         if (RecordingPluginWrapper.OpenExistingRecordingFile(recorderId, directory, directory.Length, recordName, recordName.Length))
//         {
//             Debug.Log("Replay started successfully.");
            
//             // Example: Fetch object transform and info
//             float[] matrixDTO = new float[20];
//             int[] infoDTO = new int[2];
//             unsafe
//             {
//                 fixed (float* matrixPtr = matrixDTO)
//                 fixed (int* infoPtr = infoDTO)
//                 {
//                     RecordingPluginWrapper.GetTransformAndInformationAtTime(
//                         recorderId,
//                         "ObjectName",
//                         "ObjectName".Length,
//                         123,
//                         Time.time,
//                         (IntPtr)matrixPtr,
//                         (IntPtr)infoPtr
//                     );
//                 }
//             }

//             // Example: Fetch sound chunk
//             float[] soundDTO = new float[4800];
//             unsafe
//             {
//                 fixed (float* soundPtr = soundDTO)
//                 {
//                     RecordingPluginWrapper.GetSoundChunkForTime(recorderId, 0, Time.time, (IntPtr)soundPtr);
//                 }
//             }

//             Debug.Log("Replay data fetched.");
//         }
//         else
//         {
//             Debug.LogError("Failed to start replay.");
//         }
//     }

//     private void StopReplay()
//     {
//         if (RecordingPluginWrapper.StopReplay(recorderId))
//         {
//             Debug.Log("Replay stopped successfully.");
//         }
//         else
//         {
//             Debug.LogError("Failed to stop replay.");
//         }
//     }

// }
