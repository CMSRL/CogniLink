using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using System;
using System.Net;
using static UnityEngine.GraphicsBuffer;
using System.Linq;
using System.Collections.Generic;

using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
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
     //  receive "primary" anchor from first device in device list
        // primarySpatialAnchor = await SocketServer.AnchorReceive(Configuration.DeviceIPAddresses[0]);

        // byte checksum = ComputeAdditionChecksum(primarySpatialAnchor);
        // int checksumInt = checksum;
        // Debug.Log($"Anchor Checksum: {checksumInt}");

        // //send primary anchor to all devices
        // if (Configuration.DeviceIPAddresses.Count > 1)
        // {          
        //     for (int i = 1; i < Configuration.DeviceIPAddresses.Count; i++)
        //     {
        //         await SocketServer.AnchorSend(primarySpatialAnchor, Configuration.DeviceIPAddresses[i]);
        //     }
        // }
        while (!sceneSyncCancel)
        {
            //query each device for updated device-local scene
            int j =0;
            foreach (string IPAddress in Configuration.DeviceIPAddresses)
            {

                var newScene = await SocketServer.SceneReceive(IPAddress);
                if (newScene != null)
                {
                    foreach (var target in newScene.Keys)
                    {
                      
                        if (GameObject.Find(target))
                        {

                            Debug.Log("Target found");
                            GameBoard.Instance.name = newScene[target].targetGUID;
                            GameBoard.Instance.transform.localPosition = newScene[target].targetLocation;
                            GameBoard.Instance.transform.localRotation = newScene[target].targetRotation;
                            GameBoard.Instance.transform.localScale = newScene[target].targetScale;

                            
                        if( newScene[target].gazeInfo.Count != 0)
                        {
                            // if (GameBoard.Instance.gazeInfo.Count<j)
                            // {
                            //     Debug.Log("Adding "+ j + "element to gaze info");
                            //     GameBoard.Instance.gazeInfo.Add(newScene[target].gazeInfo[j]);
                            // }
                            // else
                            // {
                                GazeInfo gazeReceived = newScene[target].gazeInfo[j];
                                Debug.Log ("Gaze recvd: " +  gazeReceived.gazePos + gazeReceived.gazeDir + gazeReceived.address);
                                GameBoard.Instance.gazeInfo[j] = gazeReceived;
                            // }
                        }
                        // if(gazeReceived.address == false)
                        //    {    
                        //         GameBoard.Instance.gazePos = newScene[target].gazePos;
                        //         GameBoard.Instance.gazeDir = newScene[target].gazeDir;
                        //    }

                            

                            if(GameBoard.Instance.gameBoardChanged <  newScene[target].changesVersion)
                            {
                                GameBoard.Instance.gridObjects = newScene[target].gridObjects;
                                GameBoard.Instance.gameBoardChanged = newScene[target].changesVersion;
                                GridGenerator gridGenerator = GameBoard.Instance.GetComponentInChildren<GridGenerator>();
                                await gridGenerator.UpdateGridChanges();
                                
                            }
                        
                        }
                        else
                        {
                            var parent = GameObject.Find("AnchorParent")?.transform; 
                            var newTarget = UnityEngine.Object.Instantiate(Configuration.targetPrefab, newScene[target].targetLocation, newScene[target].targetRotation, parent);
                            newTarget.transform.localPosition = newScene[target].targetLocation;
                            newTarget.transform.localRotation = newScene[target].targetRotation;
                            newTarget.transform.localScale = newScene[target].targetScale;
                            newTarget.name = newScene[target].targetGUID;
                           
                            

                            
                        }
                    }

                }
                j++;
            }

            //send current scene to devices
            int playerNumber = GameManager.Instance.player;
            for (int i = 0; i < Configuration.DeviceIPAddresses.Count; i++)               
            {
                GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Target");
                currentScene = new SerializableDictionary<string, TargetStruct>();
                string IPAddress = Configuration.DeviceIPAddresses[i];
                bool currentPlayer = false;
                if(playerNumber == 1 && i == 0)
                {
                   
                    currentPlayer = true;
                }
                else if(playerNumber == 2 && i == 1)
                {
                    
                    currentPlayer = true;
                }

                foreach (GameObject target in gameObjects)
                {
                    Debug.Log("IP Address " + i + " is being sent currentPlayer as "  + currentPlayer);
                    
                    currentScene.Add(target.name, new TargetStruct(target.transform.localPosition, target.transform.localRotation, target.transform.localScale, target.name, GameBoard.Instance.gameBoardChanged, GameBoard.Instance.gridObjects, GameBoard.Instance.target, currentPlayer, GameBoard.Instance.gazeInfo));
                }
                
                Debug.Log("Sending Scene to device at: " + IPAddress);
                await SocketServer.SceneSend(IPAddress, currentScene);


            }
           
        }


    }


    private void EndSceneSync()
    {
        sceneSyncCancel = true;
        //when scene sync cancelled, clear tracked objects

        // foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
        // {
        //     GameObject.DestroyImmediate(target);
        // }
    }

    private void AddTriggerZone()
    {
        
    }

    private void AddHologram()
    {

    }

    public static byte ComputeAdditionChecksum(byte[] data)
    {
        long longSum = data.Sum(x => (long)x);
        return unchecked((byte)longSum);
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


