// using CogniLink;
// using UnityEngine;

// public class MultiplayerManager : MonoBehaviour
// {
//     private SocketClient socketClient;      // CogniLink client for networking
//     private AnchorParent anchorParent;     // CogniLink anchor management

//     public GameObject playerPrefab;        // The player prefab to spawn
//     private GameObject localPlayer;        // Local player GameObject

//     void Start()
//     {
//         // Find the SocketClient and AnchorParent in the scene
//         socketClient = FindObjectOfType<SocketClient>();
//         anchorParent = FindObjectOfType<AnchorParent>();

//         // Ensure we're connected
//         if (socketClient != null)
//         {
//             socketClient.OnConnected += OnConnected;
//             socketClient.OnMessageReceived += OnMessageReceived;

//             // Connect to the server (replace with your server address)
//             socketClient.Connect("ws://localhost:8080");
//         }
//         else
//         {
//             Debug.LogError("SocketClient not found in the scene!");
//         }
//     }

//     // Called once the player is successfully connected
//     private void OnConnected()
//     {
//         Debug.Log("Successfully connected to the server.");

//         // Instantiate the local player in the scene
//         localPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
//         anchorParent.AddAnchor(localPlayer.transform);

//         // Send a message to the server about the player's spawn (if needed)
//         var message = new { type = "PlayerSpawned", position = localPlayer.transform.position };
//         socketClient.SendMessage("PlayerSpawned", JsonUtility.ToJson(message));
//     }

//     // Handle messages from the server or other players
//     private void OnMessageReceived(string messageType, string data)
//     {
//         if (messageType == "PlayerSpawned")
//         {
//             // Deserialize data and spawn the remote player
//             var remotePlayerData = JsonUtility.FromJson<PlayerData>(data);
//             SpawnRemotePlayer(remotePlayerData);
//         }
//         else if (messageType == "UpdatePosition")
//         {
//             // Handle position updates from other players
//             var positionData = JsonUtility.FromJson<Vector3>(data);
//             UpdatePlayerPosition(positionData);
//         }
//     }

//     // Spawn a remote player at the received position
//     private void SpawnRemotePlayer(PlayerData data)
//     {
//         GameObject remotePlayer = Instantiate(playerPrefab, data.position, Quaternion.identity);
//         anchorParent.AddAnchor(remotePlayer.transform);
//     }

//     // Update the local player's position based on network data
//     private void UpdatePlayerPosition(Vector3 newPosition)
//     {
//         if (localPlayer != null)
//         {
//             localPlayer.transform.position = newPosition;
//         }
//     }

//     // Send the local player's position to the server
//     void Update()
//     {
//         if (localPlayer != null && socketClient != null && socketClient.IsConnected)
//         {
//             var positionMessage = JsonUtility.ToJson(localPlayer.transform.position);
//             socketClient.SendMessage("UpdatePosition", positionMessage);
//         }
//     }

//     private void OnApplicationQuit()
//     {
//         if (socketClient != null && socketClient.IsConnected)
//         {
//             socketClient.Disconnect();
//         }
//     }
// }

// [System.Serializable]
// public class PlayerData
// {
//     public string playerId;
//     public Vector3 position;
// }
