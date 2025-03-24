using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System;
using System.Net;
using static UnityEngine.GraphicsBuffer;

#if UNITY_EDITOR
public class CogniLinkEditorWindow : EditorWindow
{
    public static CogniLinkEditorWindow Instance { get; private set; }
    public DeviceConfigurationScriptableObject Configuration;
    private bool sceneSyncCancel = true;
    private byte[] primarySpatialAnchor;
    private SerializableDictionary<string, TargetStruct> currentScene;

    public static void Log(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);
        Debug.Log(message);
    }

    private void OnEnable()
    {
        

    }

    private void OnDisable()
    {


    }
    private void OnDestroy()
    {
        Debug.Log("Scene builder destroy");

    }

    public CogniLinkEditorWindow()
    {
        Instance = this;
    }

    [MenuItem("CogniLink/Show Window")]
    public static void ShowWindow()
    {
        if (Instance == null)
        {
            Instance = EditorWindow.GetWindow<CogniLinkEditorWindow>("CogniLink");
            Instance.Show();
        }
        else
        {
            Instance.Focus();
        }
    }

    private void OnValidate()
    {

    }

    private void OnGUI()
    {

        ShowDeviceConfiguration();
        ShowActionTools();

    }

    private void ShowExtraActions()
    {
        /*if (GUILayout.Button("Start Device Sync", GUILayout.Width(125)))
        {

        }*/
    }

    private void ShowActionTools()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Start Scene Sync", GUILayout.Width(125)))
        {
            StartSceneSync();
            
        }

        if (GUILayout.Button("End Scene Sync", GUILayout.Width(125)))
        {
            EndSceneSync();
        }

        EditorGUILayout.EndHorizontal();
    }

    private async void StartSceneSync()
    {
        Configuration = (DeviceConfigurationScriptableObject)EditorGUILayout.ObjectField(Configuration, typeof(DeviceConfigurationScriptableObject), true);
        sceneSyncCancel = false;
        //receive "primary" anchor from first device in device list
        primarySpatialAnchor = await SocketServer.AnchorReceive(Configuration.DeviceIPAddresses[0]);
        
        //send primary anchor to all devices
        if (Configuration.DeviceIPAddresses.Count > 1)
        {          
            for (int i = 1; i < Configuration.DeviceIPAddresses.Count; i++)
            {
                await SocketServer.AnchorSend(primarySpatialAnchor, Configuration.DeviceIPAddresses[i]);
            }
        }
        while (!sceneSyncCancel)
        {
            //query each device for updated device-local scene
            foreach (string IPAddress in Configuration.DeviceIPAddresses)
            {
                var newScene = await SocketServer.SceneReceive(IPAddress);
                if (newScene != null)
                {
                    foreach (var target in newScene.Keys)
                    {
                        if (GameObject.Find(target))
                        {
                            var existingTarget = GameObject.Find(target);
                            //Debug.Log(newScene[target.name].targetLocation);
                            existingTarget.transform.position = newScene[target].targetLocation;
                            existingTarget.transform.rotation = newScene[target].targetRotation;
                        }
                        else
                        {
                            var newTarget = UnityEngine.Object.Instantiate(Configuration.targetPrefab, newScene[target].targetLocation, newScene[target].targetRotation);
                            newTarget.name = newScene[target].targetGUID;
                            Debug.Log("Adding new target to sceneList");
                        }
                    }

                }

            }

            GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Target");
            currentScene = new SerializableDictionary<string, TargetStruct>();
            //sync currentScene after updates from all devices
            foreach (GameObject target in gameObjects)
            {
                currentScene.Add(target.name, new TargetStruct(target.transform.position, target.transform.rotation, target.name));
            }

            //send current scene to devices
            foreach (string IPAddress in Configuration.DeviceIPAddresses)
            {
                Debug.Log("Sending Scene to device at: " + IPAddress);
                await SocketServer.SceneSend(IPAddress, currentScene);

            }
        }


    }

    private void EndSceneSync()
    {
        sceneSyncCancel = true;
        //when scene sync cancelled, clear tracked objects

        foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
        {
            GameObject.DestroyImmediate(target);
        }
    }

    private void AddTriggerZone()
    {
        
    }

    private void AddHologram()
    {

    }


    private void ShowDeviceConfiguration()
    {
        Configuration = (DeviceConfigurationScriptableObject)EditorGUILayout.ObjectField(Configuration, typeof(DeviceConfigurationScriptableObject), true);
        if (Configuration == null)
        {
            EditorGUILayout.LabelField($"Device: ", "[none]");
        }
        else
        {
            foreach(string IPAddress in Configuration.DeviceIPAddresses)
            {
                EditorGUILayout.LabelField($"Device: ", IPAddress);
            }
            
        }

        EditorGUILayout.LabelField("Syncing Scene:", !sceneSyncCancel ? "Yes" : "No"); 

      /*  if (!ActiveConnection.Started)
        {
            Task.Run(() =>
            {
                ActiveConnection.Start(IPAddress);
            });
        }
        else if (ActiveConnection.CurrentHostName != IPAddress)
        {
            ActiveConnection.Stop();
        }

        EditorGUILayout.LabelField("Connected:", ActiveConnection.IsConnected() ? "Yes" : "No");*/
    }
}
#endif


