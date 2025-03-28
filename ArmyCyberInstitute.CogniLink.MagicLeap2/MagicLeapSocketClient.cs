using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using static UnityEngine.XR.MagicLeap.MLDepthCamera;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;




#if MAGICLEAP
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
[RequireComponent(typeof(ARAnchorManager))]
#endif

public class MagicLeapSocketClient : MonoBehaviour
{

    SerializableDictionary<string, TargetStruct> sceneList = new SerializableDictionary<string, TargetStruct>();
    public GameObject prefabTarget;
    byte[] serializedAnchors = null;
    bool anchorAdded = false;
    bool anchorSerialized = false;

    // Start is called before the first frame update
    async void Start()
    {
        await Scene_Send_Listener_Start();
        await Scene_Receive_Listener_Start();

    }

    // Update is called once per frame
    void Update()
    {

    }
    private async Task<bool> Scene_Send_Listener_Start()
    {

        string localIP = GetLocalIPAddress();
        Debug.Log($"Attempting to bind listener to IP: {localIP}");
        System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse(localIP);
        IPEndPoint localEndpoint = new IPEndPoint(ipaddress, 11315);
        TcpListener listener = new (localEndpoint);
        listener.Start();

        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        var t = Task.Run(async () => {
            while (true)
            {
                using TcpClient handler = await listener.AcceptTcpClientAsync();
                await trySendScene(handler);
            }

        } , token);



        Debug.Log("Listening for Scene Send Requests");

        return true;
    }

    private async Task<bool> Scene_Receive_Listener_Start()
    {

        string localIP = GetLocalIPAddress();
        Debug.Log($"Attempting to bind listener to IP: {localIP}");
        System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse(localIP);
        IPEndPoint localEndpoint = new(ipaddress, 11317);
        TcpListener listener = new(localEndpoint);
        listener.Start();

        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        var t = Task.Run(async () => {
            while (true)
            {
                using TcpClient handler = await listener.AcceptTcpClientAsync();
                await tryReceiveScene(handler);
            }
        }, token);



        Debug.Log("Listening for Scene Receive Requests");

        return true;
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList[1].ToString();
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }


    public async Task<bool> tryReceiveScene(TcpClient client)
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
            using (NetworkStream stream =  client.GetStream())
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

                // Send complete confirmation
                //Debug.Log("Sending confirmation");
                stream.WriteByte(1);
                await stream.FlushAsync();
               // Debug.Log("Confirmation sent");

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
            Debug.Log(e.Message);
            return false;
        }
    }

    async Task<bool> trySendScene(TcpClient client)
    {
        try
        {

            foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
            {
                if (target.GetComponent<TargetEntity>().isOwner == true)
                {
                    //Update the existing target in sceneList with its most current location and roation before sending.
                    sceneList[target.name] = new TargetStruct(target.transform.localPosition, target.transform.localRotation, target.transform.localScale, target.name);
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

        var streamArray = SerializationTools.SerializeSceneStructToStream(sceneList);

        try
        {

            using (NetworkStream stream = client.GetStream())
            {
                // Send size of data
                //UnityEngine.Debug.Log($"Sending SceneStruct size: {streamArray.Length}");
                await stream.WriteAsync(BitConverter.GetBytes(streamArray.Length));
                await stream.FlushAsync();

                // Send data
                //UnityEngine.Debug.Log("Sending SceneStruct (from device)");
                await stream.WriteAsync(streamArray);
                await stream.FlushAsync();
                //UnityEngine.Debug.Log("SceneStruct sent");

                // Wait for confirmation from server
                //UnityEngine.Debug.Log("Waiting for confirmation");
                byte[] buffer = new byte[1];
                await stream.ReadAsync(buffer);
                //UnityEngine.Debug.Log("Sending SceneStruct complete");


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

}
