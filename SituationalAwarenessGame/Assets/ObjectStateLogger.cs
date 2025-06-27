using System;
using System.IO;
using UnityEngine;
using TMPro;


public class ObjectStateLogger : MonoBehaviour
{
    public string tagToTrack = "Recordable";
    public float logInterval = 0.5f;

    private string logPath;
    private float lastLogTime;

    void Start()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");
        logPath = Path.Combine(Application.persistentDataPath, $"ReplayLog_{timestamp}.csv");

        File.WriteAllText(logPath, "Time,ObjectName,Position,Rotation,Scale,IsActive,TextContent\n");
    }

    void Update()
    {
        if (Time.time - lastLogTime >= logInterval)
        {
            LogAllRecordableObjects();
            lastLogTime = Time.time;
        }
    }

    void LogAllRecordableObjects()
    {
        float timeStamp = Time.time;
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);

        using (StreamWriter sw = new StreamWriter(logPath, true))
        {
            int counter = 0;

            foreach (GameObject obj in allObjects)
            {
                if (obj.CompareTag(tagToTrack) || obj.CompareTag("Cell") || obj.CompareTag("text"))
                {
                    string name = obj.name;
                    string pos = FormatVector(obj.transform.position);
                    string rot = FormatVector(obj.transform.rotation.eulerAngles);
                    string scale = FormatVector(obj.transform.lossyScale);

                    // MeshRenderer active check
                    bool anyMeshRendererActive = false;
                    foreach (var mr in obj.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (mr.enabled && obj.activeSelf && obj != null && mr != null)
                        {
                            anyMeshRendererActive = true;
                            break;
                        }
                    }

                    if(obj.CompareTag("Cell"))
                    {
                        name = name + counter++;
                    }

                    // Only extract text if this object *is* a text object
                    string textContent = IsTextObject(obj) ? GetTextFromObject(obj) : "\"\"";

                    string row = $"{timeStamp:F2},{name},{pos},{rot},{scale},{anyMeshRendererActive},{textContent}";
                    sw.WriteLine(row);
                }
            }
        }
    }

    string FormatVector(Vector3 v) => $"\"{v.x:F2},{v.y:F2},{v.z:F2}\"";

    string GetTextFromObject(GameObject obj)
    {
        TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
            return $"\"{tmp.text.Replace("\"", "'").Replace(",", ";")}\"";

        TextMesh tm = obj.GetComponentInChildren<TextMesh>(true);
        if (tm != null)
            return $"\"{tm.text.Replace("\"", "'").Replace(",", ";")}\"";

        return "\"\"";
    }

    bool IsTextObject(GameObject obj)
    {
        // You can modify this condition to suit your project:
        return obj.CompareTag("text");
    }
}
