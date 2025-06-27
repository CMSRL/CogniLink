using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GazeReplayController : MonoBehaviour
{
    public GameObject headAvatar;
    public TextAsset gazeCSV;
    public float replaySpeed = 1f;

    private class GazeFrame
    {
        public float timeStamp;
        public Vector3 origin;
        public Vector3 direction;
    }

    private List<GazeFrame> gazeData = new List<GazeFrame>();
    private float startTime;
    private int currentIndex = 0;

    void Start()
    {
        LoadCSVData();
        startTime = Time.time;
    }

    void Update()
    {
        if (currentIndex >= gazeData.Count) return;

        float elapsed = (Time.time - startTime) * replaySpeed;
        while (currentIndex < gazeData.Count && gazeData[currentIndex].timeStamp <= elapsed)
        {
            var frame = gazeData[currentIndex];
            headAvatar.transform.position = frame.origin;
            Debug.Log(frame.origin);
            //headAvatar.transform.rotation = Quaternion.LookRotation(frame.direction);
            currentIndex++;
        }
    }

   void LoadCSVData()
{
    using StringReader reader = new StringReader(gazeCSV.text);
    string line;
    bool headerSkipped = false;

    while ((line = reader.ReadLine()) != null)
    {
        if (!headerSkipped)
        {
            headerSkipped = true;
            continue;
        }

        // Split line, but preserve vector strings like "-0.05,1.58,-0.21"
        string[] parts = SplitCSVLine(line);
        if (parts.Length < 6) continue;

        float time;
        if (!float.TryParse(parts[0].Trim(), out time)) continue;

        Vector3 origin = ParseVector3(parts[3]);
        Vector3 direction = ParseVector3(parts[4]);

        gazeData.Add(new GazeFrame
        {
            timeStamp = time,
            origin = origin,
            direction = direction.normalized
        });
    }
}

string[] SplitCSVLine(string line)
{
    var inQuotes = false;
    var value = "";
    var values = new List<string>();

    foreach (char c in line)
    {
        if (c == '"')
        {
            inQuotes = !inQuotes;
            continue;
        }

        if (c == ',' && !inQuotes)
        {
            values.Add(value);
            value = "";
        }
        else
        {
            value += c;
        }
    }
    values.Add(value);
    return values.ToArray();
}


   Vector3 ParseVector3(string s)
{
    string[] vals = s.Trim().Split(',');
    if (vals.Length != 3)
    {
        Debug.LogWarning("Invalid vector string: " + s);
        return Vector3.zero;
    }

    try
    {
        return new Vector3(
            float.Parse(vals[0].Trim()),
            float.Parse(vals[1].Trim()),
            float.Parse(vals[2].Trim())
        );
    }
    catch (System.Exception e)
    {
        Debug.LogError("Failed to parse vector: " + s + " Error: " + e.Message);
        return Vector3.zero;
    }
}
}
