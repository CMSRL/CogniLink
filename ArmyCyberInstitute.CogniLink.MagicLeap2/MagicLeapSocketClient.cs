using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net.NetworkInformation;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;

#if MAGICLEAP
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
[RequireComponent(typeof(ARAnchorManager))]
#endif

public class MagicLeapSocketClient : MonoBehaviour
{

    SerializableDictionary<string, TargetStruct> sceneList = new SerializableDictionary<string, TargetStruct>();
    // Start is called before the first frame update
    async void Start()
    {
        await Scene_Send_Listener_Start();


    }

    // Update is called once per frame
    void Update()
    {

    }
    private async Task<bool> Scene_Send_Listener_Start()
    {

        string localIP = GetLocalIPAddress();
        string hardcodedIP = "10.213.115.56";
        System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse(hardcodedIP);
        IPEndPoint ipEndPoint = new(ipaddress, 11315);
        Socket listener = new Socket(ipEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(ipEndPoint);
        listener.Listen(100);
        //System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse(localIP);
        //IPEndPoint localEndpoint = new IPEndPoint(ipaddress, 11315);
        //TcpListener listener = new TcpListener(localEndpoint);
        //listener.Start();
        while (true)
        {
            //using TcpClient client = listener.AcceptTcpClient();
            Socket handler = await listener.AcceptAsync();
            await trySendScene(handler);
        }


        Debug.Log("Listening for Scene Send Requests");

        return true;
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        return host.AddressList[0].ToString();
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    async Task<bool> trySendScene(Socket client)
    {
        try
        {
            foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
            {
                if (sceneList.ContainsKey(target.name))
                {
                    //Debug.Log("Update target position");
                    sceneList[target.name] = new TargetStruct(target.transform.localPosition, target.transform.localRotation, target.transform.localScale, target.name);
                }
                else
                {
                    sceneList.Add(target.name, new TargetStruct(target.transform.localPosition, target.transform.localRotation, target.transform.localScale, target.name));
                    Debug.Log("Adding new target to sceneList");
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

            using (NetworkStream stream = new NetworkStream(client))
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
