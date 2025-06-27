using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary;
using Unity.Collections.LowLevel.Unsafe.NotBurstCompatible;
using TMPro;
using System.Threading.Tasks;

#if ENABLE_WINMD_SUPPORT
using Windows.Storage; // For UWP Storage API
#endif
using System;


public class SceneRecorder : MonoBehaviour
{

    
    [System.Serializable]
    public class ObjectState
    {
        public string objectName;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public bool isVisible;
        public string objectParent;
        public string text;
    }

    [System.Serializable]
    public class FrameData
    {
        public float timestamp;
        public List<ObjectState> objectStates = new List<ObjectState>();
    }

    public static List<FrameData> recordedFrames = new List<FrameData>();
    public float recordInterval = 0.1f; // Record every 0.1 seconds
    private float lastRecordTime = 0f;
    private static bool isSaving = false;


    public List<GameObject> objectsToRecord = new List<GameObject>();
    

    void Start()
    {
        List<GameObject> tempList = new List<GameObject>(objectsToRecord);
        foreach (GameObject parent in tempList)
        {
            AddParentAndChildren(parent);
        }
    }

    async void OnApplicationQuit()
    {
        
        // #if ENABLE_WINMD_SUPPORT
        // await SaveRecordedDataAsync();
        // await Task.Delay(500);
        // #else
        SaveRecordedData();
        // #endif
        
       
    }

    async void OnApplicationPause(bool pauseStatus)
    {
         Debug.Log("App is pausing (HoloLens suspending)");
         Debug.Log(pauseStatus);

         try
        {
        //     #if ENABLE_WINMD_SUPPORT
        //     await SaveRecordedDataAsync(); // Using async save method
        //     await Task.Delay(500);
        //     #else
            SaveRecordedData(); // Sync save method
            // #endif
        }
        catch (Exception e)
        {
            Debug.LogError($"Error during save: {e.Message}");
        }
    }


    void Update()
    {
    //     if (Time.time - lastRecordTime >= recordInterval)
    //     {
            RecordFrame();
            //lastRecordTime = Time.time;
        //}
    }

    public void AddParentAndChildren(GameObject parent)
    {
        AddObjectToRecord(parent);
        foreach (Transform child in parent.transform)
        {
            AddObjectToRecord(child.gameObject);
        }
    }


    public void AddObjectToRecord(GameObject obj)
    {
        if (!objectsToRecord.Contains(obj))
        {
            objectsToRecord.Add(obj);
            Debug.Log($"Added {obj.name} to recording list. Total objects: {objectsToRecord.Count}");
        }
    }


   void RecordFrame()
    {
        FrameData frame = new FrameData();
        frame.timestamp = Time.time;
        List<GameObject>  recordableObjects = new List<GameObject>();
        recordableObjects.AddRange(GameObject.FindGameObjectsWithTag("Recordable"));
        recordableObjects.AddRange(GameObject.FindGameObjectsWithTag("Cell"));
        // GameObject[] cells = GameObject.FindGameObjectsWithTag("Cell");
        // recordableObjects.AddRange(cells);

        foreach (GameObject obj in recordableObjects)
        {
    
            if (obj != null)
            {
                string parentName = null;
                 ObjectState state = new ObjectState();

                

            
                if (obj.transform.parent != null && (obj.transform.name == "11091_FemaleHead_V001" || obj.transform.name == "Paused Text" || obj.transform.name == "Resume"))
                {
                    parentName = obj.transform.parent.name;
                    //Debug.Log("parent " + parentName );
                
                state = new ObjectState
                {
                    
                    objectName = obj.name,
                    position = obj.transform.position,
                    rotation = obj.transform.rotation,
                    scale = obj.transform.localScale,
                    isVisible = obj.activeSelf,
                    objectParent = parentName
                    
                };
                }
        
                else{
                    state = new ObjectState
                    {
                        
                        objectName = obj.name,
                        position = obj.transform.position,
                        rotation = obj.transform.rotation,
                        scale = obj.transform.lossyScale,
                        isVisible = obj.activeSelf,
                        objectParent = null
                        
                    };
                }

                if (obj.GetComponent<TextMeshPro>() != null)
                {
                    state.text = obj.GetComponent<TextMeshPro>().text;
                }

                frame.objectStates.Add(state);

                // if(state.objectName == "11091_FemaleHead_V001")
                //     {
                //        // Debug.Log(state.position);
                //     }

                //  if(obj.name == "11091_FemaleHead_v4")
                //     {
                //         Debug.Log("headpos");
                //         Debug.Log( obj.transform.position);
                //         Debug.Log("pose" + state.position);
                //     }

                
                    
            }
        }

        recordedFrames.Add(frame);
       // Debug.Log($"Frame recorded. Total frames: {recordedFrames.Count}");
    }



    unsafe void SaveRecordedData()
    {
        Debug.Log($"Attempting to save {recordedFrames.Count} frames.");

        if (recordedFrames.Count == 0)
        {
            Debug.LogWarning("No frames recorded!");
            return;
        }

        UnsafeAppendBuffer stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp);
        try
        {
            BinarySerialization.ToBinary(&stream, recordedFrames);

            // Save file to the Downloads folder
            string downloadsPath = Path.Combine(Application.persistentDataPath, "recorded_scene.bin");
            
            File.WriteAllBytes(downloadsPath, stream.ToBytesNBC());

            Debug.Log($"Recorded data saved to: {downloadsPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving recorded data: {e.Message}");
        }
        finally
        {
            stream.Dispose();
        }
    }

  

    #if ENABLE_WINMD_SUPPORT
        // Async method to save serialized data to PicturesLibrary
        public static async Task SaveRecordedDataAsync()
        {
            if (isSaving) return;
            try
            {
                isSaving = true;
                // Serialization and saving logic here
            
            
                // Serialize the frames (this part is unsafe but handled synchronously)
                byte[] serializedData = SerializeRecordedFrames(recordedFrames);

                // Access the PicturesLibrary folder
                StorageFolder picturesFolder = KnownFolders.PicturesLibrary;

                // Define the file name
                string fileName = "recorded_scene.bin";

                // Create or replace the file
                Debug.Log($"Creating file: {fileName} in Pictures folder...");
                StorageFile file = await picturesFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                Debug.Log($"File created or replaced at: {file.Path}");
                // Write serialized data to the file
                await FileIO.WriteBytesAsync(file, serializedData);

                Debug.Log($"Recorded data saved to: {file.Path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error saving recorded data: {e}");
            }
            finally
            {
                isSaving = false;
            }
        }
    #endif

  // Synchronous method for unsafe serialization
    private static unsafe byte[] SerializeRecordedFrames(object recordedFrames)
    {
        if (recordedFrames == null)
        {
            throw new ArgumentNullException(nameof(recordedFrames));
        }

        UnsafeAppendBuffer stream = new UnsafeAppendBuffer(16, 8, Allocator.Temp);
        try
        {
            // Serialize data to binary
            BinarySerialization.ToBinary(&stream, recordedFrames);

            // Convert to byte array
            return stream.ToBytesNBC();
        }
        finally
        {
            // Dispose of the buffer
            stream.Dispose();
        }
    }

    private string GetPrefabPath(GameObject obj)
    {
        // Convert GameObject to resource path
        // Requires objects to be in Resources folder
        return "Prefabs/" + obj.name;  // Adjust based on your project structure
    }


}


