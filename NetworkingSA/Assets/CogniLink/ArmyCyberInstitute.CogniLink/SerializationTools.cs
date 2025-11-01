using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.InteropServices.ComTypes;

public class SerializationTools
{


    public static byte[] SerializeSceneStructToStream(SerializableDictionary<string, TargetStruct> targetList)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, TargetStruct>));
        MemoryStream stream = new MemoryStream();
        try
        {

            serializer.Serialize(stream, targetList);
        }
        catch (SerializationException ex)
        {
            throw new ApplicationException("The object graph could not be serialized" + ex.Message);
        }
        // Return the streamed object graph.
        return stream.ToArray();
    }

    public static SerializableDictionary<string, TargetStruct> DeserializeSceneStructFromStream(MemoryStream stream)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(SerializableDictionary<string, TargetStruct>));
        SerializableDictionary<string, TargetStruct> scene;
        try
        {
            stream.Seek(0, SeekOrigin.Begin);
            scene = serializer.Deserialize(stream) as SerializableDictionary<string, TargetStruct>;
        }
        catch (SerializationException ex)
        {
            throw new ApplicationException("The object graph could not be deserialized" + ex.Message);
        }
        // Return the streamed object graph.
        return scene;
    }

}
