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
using Microsoft.MixedReality.OpenXR.ARFoundation;
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
    bool anchorAdded = false;
    bool anchorSerialized = false;
    //bool anchorReadyForImport = false;
    public GameObject prefabTarget;
    public SerializableDictionary<string, TargetStruct> sceneList = new SerializableDictionary<string, TargetStruct>();

#if ENABLE_WINMD_SUPPORT
    StreamSocketListener anchorSendListener = new StreamSocketListener();
    StreamSocketListener sceneSendListener = new StreamSocketListener();
    StreamSocketListener sceneReceiveListener = new StreamSocketListener();
    StreamSocketListener anchorReceiveListener = new StreamSocketListener();
    XRAnchorTransferBatch myAnchorTransferBatch = new XRAnchorTransferBatch();

#endif


    // Use this for initialization
    async void Start()
    {
        #if ENABLE_WINMD_SUPPORT

        StartCoroutine(CreateSerializedAnchorCoroutine());
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
        //var newObject = Instantiate(prefabTarget);
        //newObject.name = GetLocalIPAddress() + " - Target1";
        //newObject.GetComponent<TargetEntity>().isOwner = true;
        //newObject.transform.parent = GameObject.Find("AnchorParent").transform;
        //newObject.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        //sceneList[newObject.name] = new TargetStruct(newObject.transform.localPosition, newObject.transform.rotation, newObject.name);
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

        await trysendAnchor(serializedAnchors, args.Socket);
        byte checksum = ComputeAdditionChecksum(serializedAnchors);
        int checksumInt = checksum;
        
        UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log($"Anchor of size {serializedAnchors.Length} Sent"); }, true);
        UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log($"Checksum: {checksumInt}"); }, true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private async void Listener_Anchor_Receive_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args)
    {
        //Receive anchor from primary anchor device
        var serializedAnchorFromSocket = await attemptReceiveSpatialAnchor(args.Socket);

        byte checksum = ComputeAdditionChecksum(serializedAnchorFromSocket);
        int checksumInt = checksum;
        UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log($"Checksum: {checksumInt}"); }, true);
        
        StartCoroutine(ImportSerializedAnchorCoroutine(serializedAnchorFromSocket));
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
    }



#else

#endif

    async void Update()
    {

    }

#if ENABLE_WINMD_SUPPORT
    IEnumerator CreateSerializedAnchorCoroutine()
    {


        while (!anchorSerialized)
        {
            if (!anchorAdded)
            {
                Task addAnchorTask = tryAddLocalAnchor();
                yield return new WaitUntil(() => addAnchorTask.IsCompleted);

            }
            if (!anchorSerialized && anchorAdded)
            {
                Task addAnchorBatchTask = tryAddAnchorToBatch();
                yield return new WaitUntil(() => addAnchorBatchTask.IsCompleted);
            }

        }

    }

    IEnumerator ImportSerializedAnchorCoroutine(byte[] serializedRawAnchor)
    {

            Task anchorImportTask = tryImportLocalAnchor(serializedRawAnchor);
            yield return new WaitUntil(() => anchorImportTask.IsCompleted);


    }
#endif

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
    public async Task<byte[]> tryReceiveAnchor(StreamSocket tcpClient)
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
                UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log($"Attempting to download Anchor of size {size}"); }, true);
                

                // read data
                Debug.Log("reading...");
                byte[] buffer = new byte[size];
                var finished = await stream.ReadAsync(buffer, 0, size);
                await stream.FlushAsync();
  

                using (Stream outputStream = tcpClient.OutputStream.AsStreamForWrite())
                {
                    // Send complete confirmation
                    Debug.Log("Sending confirmation");
                    outputStream.WriteByte(1);
                    await outputStream.FlushAsync();
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

    public static byte ComputeAdditionChecksum(byte[] data)
    {
        long longSum = data.Sum(x => (long)x);
        return unchecked((byte)longSum);
    }

    private async Task<byte[]> attemptReceiveSpatialAnchor(StreamSocket tcpClient)
    {
        // Buffer to store the response bytes.
        int bytesRead = 0;
        int totalBytes = 0;
        int bufferSize = 32768;
        double progress = 0;
        int counter = 0;
        byte[] tempByteArray = null;
        int size = 0;

        try
        {
            using (var stream = tcpClient.InputStream.AsStreamForRead())
            {

                // read size
                byte[] sizeBytes = new byte[sizeof(int)];
                await stream.ReadAsync(sizeBytes, 0, sizeof(int));
                await stream.FlushAsync();
                size = BitConverter.ToInt32(sizeBytes, 0);
                UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log($"Attempting to download Anchor of size {size}"); }, true);


                byte[] myReadBuffer = new byte[size];
                tempByteArray = new byte[size];

                // Incoming message may be larger than the buffer size.
                do
                {
                    if (bufferSize > (size - totalBytes))
                    {
                        bufferSize = size - totalBytes;
                    }
                    bytesRead = await stream.ReadAsync(myReadBuffer, 0, bufferSize);
                    Array.Copy(myReadBuffer, 0, tempByteArray, totalBytes, bytesRead);
                    totalBytes += bytesRead;
                    counter += 1;
                    if (counter == 120)
                    {
                        progress = (Convert.ToDouble(totalBytes) / Convert.ToDouble(size)) * 100;
                        UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log("Recv'd " + progress + "% of Spatial Anchor"); }, true);
                        counter = 0;
                    }

                } while (totalBytes < size);

            }

            using (Stream outputStream = tcpClient.OutputStream.AsStreamForWrite())
            {
                // Send complete confirmation
                Debug.Log("Sending confirmation");
                outputStream.WriteByte(1);
                await outputStream.FlushAsync();
                Debug.Log("Confirmation sent");
            }
            UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log("Recv'd Spatial Anchor"); }, true);

            return tempByteArray;
        }
        catch(Exception e)
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log(e.Message); }, true);
            return null;
        }




    }

    private async Task<bool> tryAddAnchorToBatch()
    {

        //Debug.Log("Creating Export Anchor Batch in Socket Stream");
        try
        {

            Debug.Log($"Attempting to export anchor: {myAnchorTransferBatch.AnchorNames[0]}");
            MemoryStream memoryStream = (MemoryStream)await XRAnchorTransferBatch.ExportAsync(myAnchorTransferBatch);
            serializedAnchors = memoryStream.ToArray();
            Debug.Log($"Created Anchor Batch of size: {serializedAnchors.Length}");
            anchorSerialized = true;
            return true;
        }
        catch (Exception e)
        {
            return false;
        }

    }


    private async Task<bool> tryAddLocalAnchor()
    {
        
        //Debug.Log("Creating Export Anchor Batch in Socket Stream");
        try
        {
            TrackableId myTrackableId = GameObject.Find("AnchorParent").GetComponent<ARAnchor>().trackableId;
            //Debug.Log(myTrackableId);
            anchorAdded = myAnchorTransferBatch.AddAnchor(myTrackableId, GetLocalIPAddress());
            //Debug.Log($"Added anchor: {anchorAdded}");

            return true;
        }
        catch (Exception e)
        {
            return false;
        }

    }

    private async Task<bool> tryImportLocalAnchor(byte[] serializedAnchor)
    {
        if (serializedAnchor != null)
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log($"Attempting to import anchor of size: {serializedAnchor.Length}"); }, true);

            XRAnchorTransferBatch newAnchorTransferBatch = await XRAnchorTransferBatch.ImportAsync(new MemoryStream(serializedAnchor));
            foreach (var name in newAnchorTransferBatch.AnchorNames) UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log("Anchors after Import: " + name);  }, true);
            if (newAnchorTransferBatch != null)
            {
                //foreach (var name in newAnchorTransferBatch.AnchorNames)  UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log("Anchors before LandR: " + name);  }, true);
                newAnchorTransferBatch.LoadAndReplaceAnchor(newAnchorTransferBatch.AnchorNames[0], GameObject.Find("AnchorParent").GetComponent<ARAnchor>().trackableId);
                //foreach (var name in newAnchorTransferBatch.AnchorNames)  UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log("Anchors afer LandR: " + name);  }, true);
                UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log("Host Anchor Imported to Local System");  }, true);
                return true;

            }
            else
            {
            
                Debug.Log("Host anchor was null");
                return false;
            }



        }
        else
        {
            Debug.Log("Anchor receive request received, but anchor was null");
            return false;
        }


    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tcpClient"></param>
    /// <returns></returns>
    public async Task<bool> tryReceiveScene(StreamSocket tcpClient)
    {

        // Buffer to store the response bytes.
        int bytesRead = 0;
        int totalBytes = 0;
        int bufferSize = 32768;
        double progress = 0;
        int counter = 0;
        byte[] tempByteArray = null;
        int size = 0;

        try
        {
            using (var stream = tcpClient.InputStream.AsStreamForRead())
            {

                // read size
                byte[] sizeBytes = new byte[sizeof(int)];
                await stream.ReadAsync(sizeBytes, 0, sizeof(int));
                await stream.FlushAsync();
                size = BitConverter.ToInt32(sizeBytes, 0);
                //UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log($"Attempting to download Scene of size {size}"); }, true);


                byte[] myReadBuffer = new byte[size];
                tempByteArray = new byte[size];

                // Incoming message may be larger than the buffer size.
                do
                {
                    if (bufferSize > (size - totalBytes))
                    {
                        bufferSize = size - totalBytes;
                    }
                    bytesRead = await stream.ReadAsync(myReadBuffer, 0, bufferSize);
                    Array.Copy(myReadBuffer, 0, tempByteArray, totalBytes, bytesRead);
                    totalBytes += bytesRead;
                    counter += 1;
                    if (counter == 120)
                    {
                        progress = (Convert.ToDouble(totalBytes) / Convert.ToDouble(size)) * 100;
                       // UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log("Recv'd " + progress + "% of Scene"); }, true);
                        counter = 0;
                    }

                } while (totalBytes < size);

            }

            using (Stream outputStream = tcpClient.OutputStream.AsStreamForWrite())
            {
                // Send complete confirmation
                Debug.Log("Sending confirmation");
                outputStream.WriteByte(1);
                await outputStream.FlushAsync();
                Debug.Log("Confirmation sent");
            }
            //UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log("Recv'd Scene"); }, true);

            var newScene = SerializationTools.DeserializeSceneStructFromStream(new MemoryStream(tempByteArray));
            //Debug.Log("Scene Received");

            if (newScene != null)
            {
                foreach (var target in newScene.Keys)
                {

                    if (GameObject.Find(target))
                    {
                        //if target is not owned (i.e., the target is from a peer device), AND the target exists from a previous scene update, update its position
                        if (!GameObject.Find(target).GetComponent<TargetEntity>().isOwner)
                        {
                            var existingTarget = GameObject.Find(target);
                            existingTarget.transform.localPosition = newScene[target].targetLocation;
                            existingTarget.transform.localRotation = newScene[target].targetRotation;
                            existingTarget.transform.localScale = newScene[target].targetScale;
                        }

                    }
                    //if the target is brand new and from a peer device, add to scene
                    else
                    {
                        var parent = GameObject.Find("AnchorParent").transform;
                        var newTarget = UnityEngine.Object.Instantiate(prefabTarget, newScene[target].targetLocation, newScene[target].targetRotation, parent);
                        newTarget.name = newScene[target].targetGUID;
                        newTarget.transform.localPosition = newScene[target].targetLocation;
                        newTarget.transform.localRotation = newScene[target].targetRotation;
                        newTarget.transform.localScale = newScene[target].targetScale;
                        //Debug.Log("Adding new target to sceneList");
                    }


                }

            }
            else
            {
                Debug.Log("Received Scene is Null");
            }

            return true;
        
        }
        catch (Exception e)
        {
            UnityEngine.WSA.Application.InvokeOnAppThread(async () => { Debug.Log(e.Message); }, true);
            return false;
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
            UnityEngine.WSA.Application.InvokeOnAppThread(async () =>
            {
                foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
                {
                    if (target.GetComponent<TargetEntity>().isOwner == true)
                    {
                        //Update the existing target in sceneList with its most current location and roation before sending.
                        sceneList[target.name] = new TargetStruct(target.transform.localPosition, target.transform.localRotation, target.transform.localScale, target.name);
                    }

                }
            }, true);
 

            

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
