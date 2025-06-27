using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Serialization.Binary;
using System.Linq;
using TMPro;


public class ReplayManager : MonoBehaviour
{
    //public string replayFilePath = @"D:\maha\Project2\SituationalAwarenessGame\recorded_scene.bin";
    public string replayFileName = "recorded_scene.bin";
    private string replayFilePath;
    private List<FrameData> replayData;
    private Dictionary<string, GameObject> replayObjects = new Dictionary<string, GameObject>();
    private float replayTime = 0f;
    private int currentFrameIndex = 0;
    public bool isPlaying = false;
    public Dictionary<string, GameObject> prefabDictionary =new Dictionary<string, GameObject>();
    private Vector3 sceneOffset = Vector3.zero;

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

        void Awake()
    {
        replayFilePath = Path.Combine(Application.persistentDataPath, "recorded_scene.bin");
    }
    void Start()
    {
        PopulatePrefabDictionary();
        LoadReplayData();
        InstantiateReplayObjects();
        StartReplay();
      
    }

    void Update()
    {
        if (isPlaying)
        {
            replayTime += Time.deltaTime;
            UpdateReplayObjects();
        }
    }

    void LoadReplayData()
    {
        if (!File.Exists(replayFilePath))
        {
            Debug.LogError("Replay file not found!");
            return;
        }

        try
        {
            byte[] bytes = File.ReadAllBytes(replayFilePath);
            unsafe
            {
                fixed (byte* ptr = bytes)
                {
                    var reader = new UnsafeAppendBuffer.Reader(ptr, bytes.Length);
                    replayData = BinarySerialization.FromBinary<List<FrameData>>(&reader);
                }
            }
            Debug.Log($"Loaded {replayData.Count} frames of replay data.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading replay data: {e.Message}");
        }
    }

    

void InstantiateReplayObjects()
{
    List<ObjectState> childObjects  = new List<ObjectState>();
    if (replayData == null || replayData.Count == 0) return;

    foreach (var objectState in replayData[0].objectStates)
    {
        string baseName = objectState.objectName.Split('(')[0];
        
    
        
        
        if (prefabDictionary.TryGetValue(baseName, out GameObject prefab) && objectState.objectParent == null)
        {
            GameObject obj = Instantiate(prefab);
            obj.name = objectState.objectName;
            obj.transform.position = objectState.position;
            obj.transform.rotation = objectState.rotation;
            obj.transform.localScale = objectState.scale;
            obj.SetActive(objectState.isVisible);
            replayObjects[objectState.objectName] = obj;
        }
         if (objectState.objectParent != null )
        {
            childObjects.Add(objectState);
           
        }
        
    }

     foreach (var objectState in childObjects)
    {
         GameObject parentObj = GameObject.Find(objectState.objectParent);
            Debug.Log(childObjects);
            if(parentObj != null)
            {
                Debug.Log("here");
                GameObject obj = parentObj.transform.GetChild(0).gameObject;
                Debug.Log(objectState.objectName);
                obj.name = objectState.objectName;
                obj.transform.position = objectState.position;
                obj.transform.rotation = objectState.rotation;
                obj.transform.localScale = objectState.scale;
                obj.SetActive(objectState.isVisible);
                replayObjects[objectState.objectName] = obj;
            }
    }

   
}


    void PopulatePrefabDictionary()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        foreach (GameObject prefab in prefabs)
        {
            string baseName = prefab.name.Split('(')[0];
            prefabDictionary[baseName] = prefab;
        }
    }


    void UpdateReplayObjects()
    {
     

        if (currentFrameIndex >= replayData.Count)
        {
            isPlaying = false;
            Debug.Log("Replay finished.");
            return;
        }

        FrameData currentFrame = replayData[currentFrameIndex];
        FrameData nextFrame = (currentFrameIndex < replayData.Count - 1) ? replayData[currentFrameIndex + 1] : null;

        if (nextFrame != null && replayTime >= nextFrame.timestamp)
        {
            currentFrameIndex++;
            currentFrame = nextFrame;
            nextFrame = (currentFrameIndex < replayData.Count - 1) ? replayData[currentFrameIndex + 1] : null;
        }

        float interpolationFactor = (nextFrame != null) 
            ? (replayTime - currentFrame.timestamp) / (nextFrame.timestamp - currentFrame.timestamp)
            : 0.1f;

        //HashSet<string> objectsInCurrentFrame = new HashSet<string>(currentFrame.objectStates.Select(s => s.objectName));
        
        foreach (var objectState in currentFrame.objectStates)
        {

            if (replayObjects.TryGetValue(objectState.objectName, out GameObject obj))
            {
                if (!obj.activeSelf && objectState.isVisible)
                {
                    obj.SetActive(true);
                }

                Vector3 targetPosition = objectState.position - sceneOffset;
                Quaternion targetRotation = objectState.rotation;
                Vector3 targetScale = objectState.scale;

                if (nextFrame != null)
                {
                    var nextObjectState = nextFrame.objectStates.Find(s => s.objectName == objectState.objectName);

                    // if(objectState.objectName == "11091_FemaleHead_V001")
                    // {
                    //     Debug.Log(objectState.position);
                    // }
                    
                    if (nextObjectState != null)
                    {
                        
                        obj.transform.position = objectState.position;
                        obj.transform.rotation = objectState.rotation;
                        obj.transform.localScale = objectState.scale;
                        obj.transform.eulerAngles = objectState.rotation.eulerAngles;
                        obj.SetActive(objectState.isVisible);

                    }
                     else
                    {
                    Debug.LogWarning($"Object {objectState.objectName} not found in next frame");
                    obj.SetActive(false); // to check
                    }

                 
                }
                else
                {
                    obj.transform.position = objectState.position;
                    obj.transform.rotation = objectState.rotation;
                    obj.transform.localScale = objectState.scale;
                    obj.SetActive(objectState.isVisible);
                }
                
                if (objectState.text != null)
                {
                    obj.GetComponent<TextMeshPro>().text = objectState.text;
                }
                Vector3 currentVelocity = Vector3.zero;
               
            }
             else
            {
                Debug.LogWarning($"Object {objectState.objectName} not found in replay objects. Attempting to instantiate.");
                InstantiateReplayObject(objectState);
            }
           

            
            
        }

    }


    void InstantiateReplayObject(ObjectState objectState)
    {
        string baseName = objectState.objectName.Split('(')[0];
        if (prefabDictionary.TryGetValue(baseName, out GameObject prefab))
        {
            GameObject obj = Instantiate(prefab);
            obj.name = objectState.objectName;
            obj.transform.position = objectState.position;
            obj.transform.rotation = objectState.rotation;
            obj.transform.localScale = objectState.scale;
            obj.SetActive(objectState.isVisible);
            replayObjects[objectState.objectName] = obj;
        }
        else
        {
            Debug.LogWarning($"Prefab for {baseName} not found in prefabDictionary, Adding prefab to dictionary");
            GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
            foreach (GameObject prefabInstance in prefabs)
            {
                if (prefabInstance.name == baseName)
                {
                    prefabDictionary[baseName] = prefabInstance;
                }
            }
            
        }
    }
    public void StartReplay()
    {
        replayTime = 0.1f;
        currentFrameIndex = 0;
        isPlaying = true;
    }

    public void PauseReplay()
    {
        isPlaying = false;
    }

    public void ResumeReplay()
    {
        isPlaying = true;
    }

    public void SeekReplay(float time)
    {
        replayTime = Mathf.Clamp(time, 0f, replayData[replayData.Count - 1].timestamp);
        currentFrameIndex = replayData.FindIndex(frame => frame.timestamp > replayTime) - 1;
        if (currentFrameIndex < 0) currentFrameIndex = 0;
        UpdateReplayObjects();
    }
}
