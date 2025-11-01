using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using System.Runtime.Serialization;
using System.Collections.Generic;


[Serializable]
public struct TargetStruct
{
    //[XmlElement(ElementName = "TargetLocation")]
    public Vector3 targetLocation { get; set; }

   // [XmlElement(ElementName = "TargetRotation")]
    public Quaternion targetRotation { get; set; }

    // [XmlElement(ElementName = "TargetScale")]
    public Vector3 targetScale { get; set; }

    // [XmlElement(ElementName = "TargetGUID")]
    public string targetGUID { get; set; }
    
    public List<GridCell> gridObjects { get; set; } 

    public int changesVersion { get; set; }

    public string targetName {get; set; }

    public bool currentPlayer;

    // public Vector3 gazePos { get; set; }

    // public Vector3 gazeDir { get; set; }

    // public string IP { get; set; }

    public List<GazeInfo> gazeInfo { get; set; } 

    public TargetStruct(Vector3 position, Quaternion rotation, Vector3 scale, String name, int changes = 0, List<GridCell> gridObjectList = null,  String targetShapeName = "", bool isPlayer = false,List<GazeInfo> gazeInformation = null)
    {
        targetLocation = position;
        targetRotation = rotation;
        targetScale = scale;
        targetGUID = name;
        gridObjects = gridObjectList;
        changesVersion = changes;
        targetName = targetShapeName;
        currentPlayer = isPlayer;
        gazeInfo = gazeInformation;
        // gazePos = gazePosition;
        // gazeDir = gazeDirection;
        // IP = IP_add;
    }


    
}
