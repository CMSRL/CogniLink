using System;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using System.Runtime.Serialization;

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

    public TargetStruct(Vector3 position, Quaternion rotation, Vector3 scale, String name)
    {
        targetLocation = position;
        targetRotation = rotation;
        targetScale = scale;
        targetGUID = name;
    }
}
