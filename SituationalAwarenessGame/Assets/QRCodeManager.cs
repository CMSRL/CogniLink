using System;
using Microsoft.MixedReality.OpenXR;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class QRCodeManager : MonoBehaviour
{
    
    private ARMarkerManager m_arMarkerManager;
    private void Awake()
    {
        m_arMarkerManager = GetComponent<ARMarkerManager>();
        Debug.Log("Called m_arMarkerManager");
        m_arMarkerManager.markersChanged += OnQRCodesChanged;
        
    }

    void OnQRCodesChanged(ARMarkersChangedEventArgs args)
    {
        Debug.Log("Called OnQRCodeChanges");

        foreach (ARMarker qrCode in args.added){
            Debug.Log($"QR code with the ID {qrCode.trackableId} added.");
            PlaceObjectAtQRCode(qrCode);
            if (qrCode.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            Debug.Log($"Fully tracked QR code with the ID {qrCode.trackableId} was added.");
        }


        foreach (ARMarker qrCode in args.removed)
            Debug.Log($"QR code with the ID {qrCode.trackableId} removed.");

        foreach (ARMarker qrCode in args.updated)
        {
            Debug.Log($"QR code with the ID {qrCode.trackableId} updated.");
            Debug.Log($"Pos:{qrCode.transform.position} Rot:{qrCode.transform.rotation} Size:{qrCode.size}");
        }
        
    }
    private void PlaceObjectAtQRCode(ARMarker qrCode)
    {
            
        Debug.Log("PlaceObjectAtQRCode called");
        List<GameObject> foundObjectsList = new List<GameObject>();

        // To look at all objects in scene
        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Debug.Log("Object in Scene:" + obj.name);
        }
        
        foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
        {
            Debug.Log("Adding target:" + target.name);

            foundObjectsList.Add(target);

        }
      
        Debug.Log("Target loop crossed");
            if (foundObjectsList.Count > 0)
            {
                Debug.Log("Found: " + foundObjectsList[0].name);
                //PrintAllChildren(foundObjectsList[0]);

                foundObjectsList[0].transform.position = qrCode.transform.position;
                foundObjectsList[0].transform.rotation = qrCode.transform.rotation;

                foundObjectsList[0].transform.Rotate(180f, 0f, 0f, Space.Self);

                // Move slightly down and to the left
                foundObjectsList[0].transform.position += foundObjectsList[0].transform.right * -0.5f; // left
                foundObjectsList[0].transform.position += foundObjectsList[0].transform.up * -0.5f;   // down
                foundObjectsList[0].SetActive(true);
            }
    }



  




}
