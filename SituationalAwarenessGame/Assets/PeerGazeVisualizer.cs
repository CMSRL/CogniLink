using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(LineRenderer))]
public class PeerGazeVisualizer : MonoBehaviour
{
    public Transform sharedAnchor;
    public GameObject gazeMarkerPrefab; // Current gaze marker
    public float trailDuration = 2.0f;  // Seconds to keep trail
    public float recordInterval = 0.05f; // Sampling rate

    private GameObject peerGazeMarker;
 
    private float lastRecordTime;

    private class GazePoint
    {
        public Vector3 worldPos;
        public float timestamp;
    }

    private List<GazePoint> gazeTrail = new List<GazePoint>();

    void Start()
    {
        peerGazeMarker = Instantiate(gazeMarkerPrefab);
     
    }

    public void UpdatePeerGaze(Vector3 relativePos, Vector3 relativeDir)
    {
        if (sharedAnchor == null || peerGazeMarker == null) return;

        Vector3 worldPos = sharedAnchor.TransformPoint(relativePos);
        Vector3 worldDir = sharedAnchor.TransformDirection(relativeDir);

        peerGazeMarker.transform.position = worldPos;
        peerGazeMarker.transform.forward = worldDir;

        float now = Time.time;
        if (now - lastRecordTime > recordInterval)
        {
            gazeTrail.Add(new GazePoint { worldPos = worldPos, timestamp = now });
            lastRecordTime = now;
        }

       // UpdateLineRenderer();
    }

   
}
