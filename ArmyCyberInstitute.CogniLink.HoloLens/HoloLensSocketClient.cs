using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Sockets;

#if ENABLE_WINMD_SUPPORT
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.Perception.Spatial;
using Windows.Storage;
using Windows.System;
using Windows.Storage.Streams;
using Microsoft.MixedReality.OpenXR;
using Windows.Perception.Spatial.Preview;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;


#endif

#if !UNITY_EDITOR
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Specialized;
#endif

#if ENABLE_WINMD_SUPPORT
[RequireComponent(typeof(ARAnchorManager))]
#endif

public class HoloLensSocketClient : MonoBehaviour
{

    byte[] serializedAnchors = null;
    public GameObject prefabTarget;
#if ENABLE_WINMD_SUPPORT
    StreamSocketListener anchorSendListener = new StreamSocketListener();
    StreamSocketListener sceneSendListener = new StreamSocketListener();
    StreamSocketListener sceneReceiveListener = new StreamSocketListener();
    StreamSocketListener anchorReceiveListener = new StreamSocketListener();

#endif

    SerializableDictionary<string, TargetStruct> sceneList = new SerializableDictionary<string, TargetStruct>();
    // Use this for initialization
    async void Start()
    {
#if ENABLE_WINMD_SUPPORT
        while (serializedAnchors == null)
        {
            serializedAnchors = await SpatialAnchors.tryAddLocalAnchor();
        }
#endif


#if ENABLE_WINMD_SUPPORT

        anchorSendListener.ConnectionReceived += Listener_Anchor_Send_ConnectionReceived;
        anchorSendListener.Control.KeepAlive = true;
        await Anchor_Send_Listener_Start();

        sceneSendListener.ConnectionReceived += Listener_Scene_Send_ConnectionReceived;
        sceneSendListener.Control.KeepAlive = true;
        await Scene_Send_Listener_Start();

        sceneReceiveListener.ConnectionReceived += Listener_Scene_Receive_ConnectionReceived;
        sceneReceiveListener.Control.KeepAlive = true;
        await Scene_Receive_Listener_Start();

        anchorReceiveListener.ConnectionReceived += Listener_Anchor_Receive_ConnectionReceived;
        anchorReceiveListener.Control.KeepAlive = true;
        await Anchor_Receive_Listener_Start();

#endif
        var newObject = Instantiate(prefabTarget);
        newObject.name = GetLocalIPAddress() + " - Target1";
        newObject.GetComponent<TargetEntity>().isOwner = true;
        newObject.transform.parent = GameObject.Find("AnchorParent").transform;
        newObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        sceneList[newObject.name] = new TargetStruct(newObject.transform.localPosition, newObject.transform.rotation, newObject.name);
    }

#if ENABLE_WINMD_SUPPORT
    private async Task<bool> Anchor_Send_Listener_Start()
    {
        //Debug.Log("Listener started");
        try
        {
            await anchorSendListener.BindServiceNameAsync("11314");
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }

        Debug.Log("Listening for Anchor Send Requests");

        return true;
    }

    private async Task<bool> Scene_Send_Listener_Start()
    {
        //Debug.Log("Listener started");
        try
        {
            await sceneSendListener.BindServiceNameAsync("11315");
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }

        Debug.Log("Listening for Scene Send Requests");

        return true;
    }

    private async Task<bool> Scene_Receive_Listener_Start()
    {
        //Debug.Log("Listener started");
        try
        {
            await sceneReceiveListener.BindServiceNameAsync("11317");
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }

        Debug.Log("Listening for Scene Receive Requests");

        return true;
    }

    private async Task<bool> Anchor_Receive_Listener_Start()
    {
        //Debug.Log("Listener started");
        try
        {
            await anchorReceiveListener.BindServiceNameAsync("11316");
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e.Message);
        }

        Debug.Log("Listening for Anchor Receive Requests");

        return true;
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void Listener_Anchor_Send_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        //var spatialAnchor = SpatialAnchors.CreateWorldSpatialAnchor();
        ////Dictionary<string, SpatialAnchor> anchorDict = new Dictionary<string, SpatialAnchor>();
        //anchorDict.Add("Test", spatialAnchor);
        //var serializedAnchor = await SpatialAnchors.SerializeSpatialAnchors(anchorDict);
        await trysendAnchor(serializedAnchors, args.Socket);
        Debug.Log("Anchor Sent");

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void Listener_Anchor_Receive_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        //Receive anchor from primary anchor device
        var serializedAnchor = await tryReceiveAnchor(args.Socket);
        await tryImportAnchor(serializedAnchor);

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void Listener_Scene_Send_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {

        await trySendScene(args.Socket);
        //Debug.Log("Scene Sent");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void Listener_Scene_Receive_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        var serializedScene = await tryReceiveScene(args.Socket);
        var newScene = SerializationTools.DeserializeSceneStructFromStream(new MemoryStream(serializedScene));
       //Debug.Log("Scene Received");

        if (newScene != null)
        {
            foreach (var target in newScene.Keys)
            {
                if (GameObject.Find(target))
                {
                    //if target is not owned (i.e., the target is from a peer device), update it position
                    if (!GameObject.Find(target).GetComponent<TargetEntity>().isOwner)
                    {
                        var existingTarget = GameObject.Find(target);
                        existingTarget.transform.localPosition = newScene[target].targetLocation;
                        existingTarget.transform.rotation = newScene[target].targetRotation;
                    }

                }
                //if the target is brand new and from a peer device, add to scene
                else
                {
                    var newTarget = UnityEngine.Object.Instantiate(prefabTarget, newScene[target].targetLocation, newScene[target].targetRotation);
                    newTarget.name = newScene[target].targetGUID;
                    newTarget.transform.parent = GameObject.Find("AnchorParent").transform;
                    newTarget.transform.localPosition = newScene[target].targetLocation;
                    //Debug.Log("Adding new target to sceneList");
                }
            }

        }
    }



#else

#endif

    async void Update()
    {


    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }

#if ENABLE_WINMD_SUPPORT
    /// <summary>
    /// 
    /// </summary>
    /// <param name="serializedAnchors"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    async Task<bool> trysendAnchor(byte[] serializedAnchors, StreamSocket client)
    {

        try
        {
            
            using (Stream outputStream = client.OutputStream.AsStreamForWrite())
            {
                // Send size of data
                UnityEngine.Debug.Log($"Sending anchor size: {serializedAnchors.Length}");
                await outputStream.WriteAsync(BitConverter.GetBytes(serializedAnchors.Length));
                await outputStream.FlushAsync();

                // Send data
                UnityEngine.Debug.Log("Sending anchors (from device)");
                await outputStream.WriteAsync(serializedAnchors);
                await outputStream.FlushAsync();
                UnityEngine.Debug.Log("Anchors sent");

                using (Stream inputStream = client.InputStream.AsStreamForRead())
                {
                    // Wait for confirmation from server
                    UnityEngine.Debug.Log("Waiting for confirmation");
                    byte[] buffer = new byte[1];
                    await inputStream.ReadAsync(buffer);
                    UnityEngine.Debug.Log("Sending anchors complete");
                }


                return true;
            }

        }
        catch (Exception e)
        {
            Debug.Log("Anchor transfer failed");
            Debug.Log(e.Message);
            return false;
            throw;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public async static Task<byte[]> tryReceiveAnchor(StreamSocket tcpClient)
    {

        try
        {
            // Receive the spatial anchor(s) data
            using (var stream = tcpClient.InputStream.AsStreamForRead())
            {
                Debug.Log($"Opening Stream");
                // read size
                byte[] sizeBytes = new byte[sizeof(int)];
                await stream.ReadAsync(sizeBytes, 0, sizeof(int));
                await stream.FlushAsync();
                int size = BitConverter.ToInt32(sizeBytes, 0);
                Debug.Log($"Attempting to download Anchor of size {size}");

                // read data
                Debug.Log("reading...");
                byte[] buffer = new byte[size];
                await stream.ReadAsync(buffer, 0, size);
                await stream.FlushAsync();
                Debug.Log("finished reading");

                using (Stream inputStream = tcpClient.OutputStream.AsStreamForWrite())
                {
                    // Send complete confirmation
                    Debug.Log("Sending confirmation");
                    inputStream.WriteByte(1);
                    await inputStream.FlushAsync();
                    Debug.Log("Confirmation sent");
                }


                return buffer;

            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    async Task<bool> tryImportAnchor(byte[] serializedAnchor)
    {
        //UnityEngine.WSA.Application.InvokeOnAppThread(async () =>
        //{
        //}, true);

        try
        {

            var importSuccess = await SpatialAnchors.tryImportLocalAnchor(serializedAnchor);
            while (!importSuccess)
            {
                importSuccess = await SpatialAnchors.tryImportLocalAnchor(serializedAnchor);
            }
            Debug.Log("Anchor Received and Loaded");
            return true;

        }
        catch (Exception e)
        {
            Debug.Log("Anchor import failed");
            Debug.Log(e.Message);
            return false;
            throw;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="client"></param>
    /// <returns></returns>
    public async static Task<byte[]> tryReceiveScene(StreamSocket tcpClient)
    {

        try
        {
            // Receive the spatial anchor(s) data
            using (var stream = tcpClient.InputStream.AsStreamForRead())
            {
                Debug.Log($"Opening Stream");
                // read size
                byte[] sizeBytes = new byte[sizeof(int)];
                await stream.ReadAsync(sizeBytes, 0, sizeof(int));
                await stream.FlushAsync();
                int size = BitConverter.ToInt32(sizeBytes, 0);
                Debug.Log($"Attempting to download Anchor of size {size}");

                // read data
                Debug.Log("reading...");
                byte[] buffer = new byte[size];
                await stream.ReadAsync(buffer, 0, size);
                await stream.FlushAsync();
                Debug.Log("finished reading");

                using (Stream inputStream = tcpClient.OutputStream.AsStreamForWrite())
                {
                    // Send complete confirmation
                    Debug.Log("Sending confirmation");
                    inputStream.WriteByte(1);
                    await inputStream.FlushAsync();
                    Debug.Log("Confirmation sent");
                }


                return buffer;

            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="streamArray"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    async Task<bool> trySendScene(StreamSocket client)
    {
        try 
        {
            foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
            {
                if (sceneList.ContainsKey(target.name))
                {
                    //Debug.Log("Update target position");
                    sceneList[target.name] = new TargetStruct(target.transform.localPosition, target.transform.rotation, target.name);
                }

            }

            

        }
        catch (Exception ex)
        {
            Debug.Log("Failed to create/serialize targets");
            Debug.Log(ex);
            return false;
            throw;
        }

        //Serialize current (local, user owned) scene objects
        var streamArray = SerializationTools.SerializeSceneStructToStream(sceneList);

        try
        {

            using (Stream outputStream = client.OutputStream.AsStreamForWrite())
            {
                // Send size of data
                //UnityEngine.Debug.Log($"Sending SceneStruct size: {streamArray.Length}");
                await outputStream.WriteAsync(BitConverter.GetBytes(streamArray.Length));
                await outputStream.FlushAsync();

                // Send data
                //UnityEngine.Debug.Log("Sending SceneStruct (from device)");
                await outputStream.WriteAsync(streamArray);
                await outputStream.FlushAsync();
                //UnityEngine.Debug.Log("SceneStruct sent");

                using (Stream inputStream = client.InputStream.AsStreamForRead())
                {
                    // Wait for confirmation from server
                    //UnityEngine.Debug.Log("Waiting for confirmation");
                    byte[] buffer = new byte[1];
                    await inputStream.ReadAsync(buffer);
                    //UnityEngine.Debug.Log("Sending SceneStruct complete");
                }


                return true;

            }
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to send detections to client");
            Debug.Log(ex);
            return false;
            throw;
        }

    }
#endif


}
