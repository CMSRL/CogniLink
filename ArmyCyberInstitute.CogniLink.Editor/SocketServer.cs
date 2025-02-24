using System;
using UnityEngine;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

public class SocketServer
{


    async public static Task<byte[]> AnchorReceive(string deviceAddress) 
    {

        try
        {
            //Debug.Log($"Opening Anchor Client");
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(deviceAddress, 11314);
            var newAnchor = await ReceiveSerializedSpatialAnchorsAsync(client);
            //Debug.Log($"Anchor Received from Client X of size {newAnchor.Length}");
            client.Close();
            return newAnchor;

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }

    }

    async public static Task<bool> AnchorSend(byte[] serializedAnchors, string deviceAddress)
    {

        try
        {
            //Debug.Log($"Opening Anchor Client");
            using TcpClient client = new TcpClient();
            await client.ConnectAsync(deviceAddress, 11316);
            var anchorSent = await SendSerializedSpatialAnchorsAsync(serializedAnchors, client);
            //Debug.Log($"Anchor Received from Client X of size {newAnchor.Length}");
            client.Close();
            return true;

        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }

    }

  
    async static Task<bool> SendSerializedSpatialAnchorsAsync(byte[] serializedAnchors, TcpClient client)
    {

        try
        {

            using (Stream stream = client.GetStream())
            {
                // Send size of data
                UnityEngine.Debug.Log($"Sending anchor size: {serializedAnchors.Length}");
                await stream.WriteAsync(BitConverter.GetBytes(serializedAnchors.Length));
                await stream.FlushAsync();

                // Send data
                UnityEngine.Debug.Log("Sending anchors (from device)");
                await stream.WriteAsync(serializedAnchors);
                await stream.FlushAsync();
                UnityEngine.Debug.Log("Anchors sent");

                // Wait for confirmation from server
                UnityEngine.Debug.Log("Waiting for confirmation");
                byte[] buffer = new byte[1];
                await stream.ReadAsync(buffer);
                UnityEngine.Debug.Log("Sending anchors complete");



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

    async public static Task<SerializableDictionary<string,TargetStruct>> SceneReceive(string deviceAddress)
    {

        try
        {
            //Debug.Log($"Opening Scene Client");
            using (TcpClient client = new TcpClient())
            {
                await client.ConnectAsync(deviceAddress, 11315);
                //Debug.Log("Connected for Scene");
                var sceneBytes = await ReceiveSerializedSceneAsync(client);
                //Debug.Log("Received Scene of Size: " + sceneBytes.Length);
                var newScene = SerializationTools.DeserializeSceneStructFromStream(new MemoryStream(sceneBytes));
                return newScene;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }

    }

    async public static Task<bool> SceneSend(string deviceAddress, SerializableDictionary<string, TargetStruct> scene)
    {

        try
        {
            //Debug.Log($"Opening Scene Client");
            using (TcpClient client = new TcpClient())
            {
                var serializedScene = SerializationTools.SerializeSceneStructToStream(scene);
                await client.ConnectAsync(deviceAddress, 11317);
                Debug.Log("Connected to send scene to device: " + deviceAddress);
                var sceneSendComplete = await SendSerializedSceneAsync(serializedScene, client);
                
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }

    }

    async static Task<bool> SendSerializedSceneAsync(byte[] serializedScene, TcpClient client)
    {

        try
        {

            using (Stream stream = client.GetStream())
            {
                // Send size of data
                UnityEngine.Debug.Log($"Sending scene size: {serializedScene.Length}");
                await stream.WriteAsync(BitConverter.GetBytes(serializedScene.Length));
                await stream.FlushAsync();

                // Send data
                UnityEngine.Debug.Log("Sending scene (from device)");
                await stream.WriteAsync(serializedScene);
                await stream.FlushAsync();
                //UnityEngine.Debug.Log("Anchors sent");

                // Wait for confirmation from server
                UnityEngine.Debug.Log("Waiting for confirmation");
                byte[] buffer = new byte[1];
                await stream.ReadAsync(buffer);
                UnityEngine.Debug.Log("Sending scene complete");



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

    public async static Task<byte[]> ReceiveSerializedSpatialAnchorsAsync(TcpClient tcpClient)
    {

        try
        {
            // Receive the spatial anchor(s) data
            using (var stream = tcpClient.GetStream())
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

                // Send complete confirmation
                Debug.Log("Sending confirmation");
                stream.WriteByte(1);
                await stream.FlushAsync();
                Debug.Log("Confirmation sent");

                return buffer;

            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async static Task<byte[]> ReceiveSerializedSceneAsync(TcpClient tcpClient)
    {

        try
        {
            // Receive the spatial anchor(s) data
            using (var stream = tcpClient.GetStream())
            {
                //Debug.Log($"Opening Stream");
                // read size
                byte[] sizeBytes = new byte[sizeof(int)];
                await stream.ReadAsync(sizeBytes, 0, sizeof(int));
                await stream.FlushAsync();
                int size = BitConverter.ToInt32(sizeBytes, 0);
                //Debug.Log($"Attempting to download Scene of size {size}");

                // read data
                //Debug.Log("reading...");
                byte[] buffer = new byte[size];
                await stream.ReadAsync(buffer, 0, size);
                await stream.FlushAsync();
               // Debug.Log("finished reading Scene");

                // Send complete confirmation
                //Debug.Log("Sending confirmation");
                stream.WriteByte(1);
                await stream.FlushAsync();
                //Debug.Log("Scene Receive Confirmation sent");
               
                return buffer;

            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            return null;
        }
    }



}
