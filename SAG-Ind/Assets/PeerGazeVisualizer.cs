using UnityEngine;

public class PeerGazeVisualizer : MonoBehaviour
{

    public Transform sharedAnchor;      // Local anchor (same as sender’s anchor in shared space)
    public GameObject gazeMarkerPrefab; // Sphere or gaze object

    public GameObject peerGazeMarker;

    void Start()
    {
        peerGazeMarker = Instantiate(gazeMarkerPrefab);
        if (peerGazeMarker != null)
        {
            Debug.Log("gazePrefab instantiated:");
        }
        
       // peerGazeMarker.transform.SetParent(sharedAnchor, worldPositionStays: true);
    }

    public void UpdatePeerGaze(Vector3 relativePos, Vector3 relativeDir)
    {

        if (sharedAnchor == null || peerGazeMarker == null)
            return;

        // Convert relative to world-space
        Vector3 worldPos = sharedAnchor.TransformPoint(relativePos);
        Vector3 worldDir = sharedAnchor.TransformDirection(relativeDir);

        Debug.Log("worldPos for peer Gaze: " + worldPos);
        Debug.Log("worldPos for peer Dir: " + worldDir);

        peerGazeMarker.transform.position = worldPos;
        peerGazeMarker.transform.forward = worldDir;
    }
}
