
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;


#if ENABLE_WINMD_SUPPORT
using Microsoft.MixedReality.OpenXR;
using Windows.Perception.Spatial;
using Windows.Perception.Spatial.Preview;
using Windows.Storage.Streams;
using Windows.Foundation;
using System.Runtime.InteropServices.WindowsRuntime;
#endif

#if !UNITY_EDITOR
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
#endif

public class SpatialAnchors
{


#if ENABLE_WINMD_SUPPORT

    public static async Task<byte[]> tryAddLocalAnchor()
    {
        MemoryStream memoryStream;
        Debug.Log("Creating Export Anchor Batch in Socket Stream");
        try
        {
            TrackableId myTrackableId;
            XRAnchorTransferBatch myAnchorTransferBatch = new XRAnchorTransferBatch();
            myTrackableId = GameObject.Find("AnchorParent").GetComponent<ARAnchor>().trackableId;
            myAnchorTransferBatch.AddAnchor(myTrackableId, "ParentAnchor");
            memoryStream = (MemoryStream)await XRAnchorTransferBatch.ExportAsync(myAnchorTransferBatch);
            byte[] bytes = memoryStream.ToArray();

            return bytes;
        }
        catch (Exception e)
        {
            return null;
        }

    }
    public static async Task<bool> tryImportLocalAnchor(byte[] serializedAnchor)
    {
        Debug.Log("Attempting to import anchor");
        var tempMemStream = new MemoryStream(serializedAnchor);
        XRAnchorTransferBatch myAnchorTransferBatch = await XRAnchorTransferBatch.ImportAsync(tempMemStream);
        if (myAnchorTransferBatch != null)
        {
            myAnchorTransferBatch.LoadAndReplaceAnchor(myAnchorTransferBatch.AnchorNames[0], GameObject.Find("AnchorParent").GetComponent<ARAnchor>().trackableId);
            Debug.Log("Host Anchor Imported to Local System");
            return true;
        }
        else 
        {
            Debug.Log("Host anchor was null");
            return false;
        }

    }

    public static async Task<IReadOnlyDictionary<string, SpatialAnchor>> TryImportAnchors(byte[] data)
    {
        await SpatialAnchorTransferManager.RequestAccessAsync();

        // Parse data into a format that SpatialAnchorTransferManager can use
        InMemoryRandomAccessStream randomStream = new InMemoryRandomAccessStream();
        IOutputStream outputStream = randomStream.GetOutputStreamAt(0);
        DataWriter dataWriter = new DataWriter(outputStream);
        IInputStream inputStream = randomStream.GetInputStreamAt(0);

        // Write data to buffer            
        dataWriter.WriteBytes(data);
        await dataWriter.StoreAsync();
        await dataWriter.FlushAsync();

        return await SpatialAnchorTransferManager.TryImportAnchorsAsync(inputStream);
    }

    public static async Task<byte[]> SerializeSpatialAnchors(IDictionary<string, SpatialAnchor> spatialAnchors)
    {
        // Request access for spatial perception
        // Null is returned if access is not allowed
        var result = await SpatialAnchorExporter.RequestAccessAsync();
        if (result != SpatialPerceptionAccessStatus.Allowed)
            return null;

        // Export spatial anchors in dictionary
        // Null is returned if export fails
        InMemoryRandomAccessStream randomStream = new InMemoryRandomAccessStream();
        if (await SpatialAnchorTransferManager.TryExportAnchorsAsync(spatialAnchors, randomStream.GetOutputStreamAt(0)))
        {
            // Put data into a buffer
            var input = randomStream.GetInputStreamAt(0);
            Windows.Storage.Streams.Buffer buffer = new Windows.Storage.Streams.Buffer((uint)randomStream.Size);
            input.ReadAsync(buffer, (uint)randomStream.Size, InputStreamOptions.None).GetResults();

            // Read data into byte array
            DataReader dataReader = DataReader.FromBuffer(buffer);
            byte[] data = new byte[randomStream.Size];
            dataReader.ReadBytes(data);

            // Log
            Debug.Log($"Spatial anchors serialized:\n\t" +
                $"count: {spatialAnchors.Count}\n\t" +
                $"bytes: {data.Length}");
            string spatialAnchorList = "";
            foreach (var anchor in spatialAnchors)
            {
                spatialAnchorList += $"\n\t\t{anchor.Key}";
            }
            Debug.Log($"\tanchor(s):" + spatialAnchorList);

            return data;
        }

        return null;
    }

    public static SpatialAnchor CreateWorldSpatialAnchor()
    {
        SpatialLocator @default = SpatialLocator.GetDefault();
        if (@default != null)
        {
            SpatialCoordinateSystem WorldSpatialCoordinateSystem = @default.CreateStationaryFrameOfReferenceAtCurrentLocation().CoordinateSystem;
            SpatialAnchor worldSpatialAnchor = SpatialAnchor.TryCreateRelativeTo(WorldSpatialCoordinateSystem);
            if (worldSpatialAnchor == null)
            {
                Debug.Log("WARNING: Could not create the persistent world spatial anchor.");
            }
            return worldSpatialAnchor;
        }
        else
        {
            Debug.Log("WARNING: Could not get spatial locator (expected in StereoKit on desktop).");
            return null;
        }
    }




#endif


}
