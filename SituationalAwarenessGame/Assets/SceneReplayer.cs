using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SceneReplayer : MonoBehaviour
{
    [Header("CSV Files for Replay")]
    public TextAsset csvFileA;
    public TextAsset csvFileB;

    private class ObjectState
    {
        public string name;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public bool isActive;
        public string textContent;
    }

    private Dictionary<float, List<ObjectState>> replayDataA = new();
    private Dictionary<float, List<ObjectState>> replayDataB = new();
    private Dictionary<string, GameObject> spawnedObjectsA = new();
    private Dictionary<string, GameObject> spawnedObjectsB = new();

    private List<float> sortedTimestamps = new();
    private float replayStartTime;
    private float currentTime;
    private bool isReplaying = false;
    public float resizeFactor;

    void Start()
    {
        if (csvFileA == null || csvFileB == null)
        {
            Debug.LogError("CSV files not assigned.");
            return;
        }

        ParseCSV(csvFileA.text, replayDataA);
        ParseCSV(csvFileB.text, replayDataB);

        HashSet<float> combinedTimestamps = new(replayDataA.Keys);
        combinedTimestamps.UnionWith(replayDataB.Keys);
        sortedTimestamps = new List<float>(combinedTimestamps);
        sortedTimestamps.Sort();

        replayStartTime = Time.time;
        currentTime = 0f;
        isReplaying = true;
    }

    void Update()
    {
        if (!isReplaying) return;

        currentTime = Time.time - replayStartTime;

        foreach (float timestamp in sortedTimestamps)
        {
            if (currentTime < timestamp) break;

            if (replayDataA.TryGetValue(timestamp, out var statesA))
                ReplayObjects(statesA, spawnedObjectsA, Vector3.zero); // No offset

            if (replayDataB.TryGetValue(timestamp, out var statesB))
                ReplayObjects(statesB, spawnedObjectsB, new Vector3(2, 0, 0) * resizeFactor); // Offset for comparison

            replayDataA.Remove(timestamp);
            replayDataB.Remove(timestamp);
        }

        if (replayDataA.Count == 0 && replayDataB.Count == 0)
        {
            Debug.Log("Replay finished.");
            isReplaying = false;
        }
    }

    void ReplayObjects(List<ObjectState> states, Dictionary<string, GameObject> objectMap, Vector3 positionOffset)
    {
        foreach (var state in states)
        {
            if (!objectMap.TryGetValue(state.name, out var obj))
            {
                string cleanName = state.name.Split("(")[0].Split("_")[0];
                GameObject prefab = Resources.Load<GameObject>($"Prefabs/{cleanName}");

                if (prefab == null)
                {
                    Debug.LogWarning($"Missing prefab for: {state.name}");
                    continue;
                }

                obj = Instantiate(prefab);
                obj.name = state.name;
                objectMap[state.name] = obj;
            }

            Vector3 pos = state.position + positionOffset;
            obj.transform.localPosition = obj.name.StartsWith("Label") ? pos + new Vector3(1, -0.2f, 0)*resizeFactor : pos;
            obj.transform.rotation = Quaternion.Euler(state.rotation);
            obj.transform.localScale = state.scale;
            obj.SetActive(state.isActive);

            if (IsTextObject(obj))
            {
                TMP_Text tmp = obj.GetComponentInChildren<TMP_Text>(true);
                if (tmp != null) tmp.text = state.textContent;

                else if (obj.GetComponentInChildren<TextMesh>(true) is TextMesh textMesh)
                    textMesh.text = state.textContent;
            }
        }
    }

    void ParseCSV(string content, Dictionary<float, List<ObjectState>> replayDict)
    {
        string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        for (int i = 1; i < lines.Length; i++)
        {
            string[] tokens = ParseCSVLine(lines[i]);
            if (tokens.Length < 6) continue;

            float timestamp = float.Parse(tokens[0]);
            string objName = tokens[1];
            Vector3 pos = ParseVector(tokens[2]);
            Vector3 rot = ParseVector(tokens[3]);
            Vector3 scale = ParseVector(tokens[4]);
            bool isActive = bool.Parse(tokens[5]);
            string textContent = tokens.Length > 6 ? tokens[6].Replace("\"", "") : "";

            if (!replayDict.ContainsKey(timestamp))
                replayDict[timestamp] = new List<ObjectState>();

            replayDict[timestamp].Add(new ObjectState
            {
                name = objName,
                position = pos*resizeFactor,
                rotation = rot,
                scale = scale*resizeFactor,
                isActive = isActive,
                textContent = textContent
            });
        }
    }

    string[] ParseCSVLine(string line)
    {
        List<string> result = new();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else if (c == '"')
                inQuotes = !inQuotes;
            else
                current += c;
        }

        result.Add(current);
        return result.ToArray();
    }

    Vector3 ParseVector(string v)
    {
        v = v.Replace("\"", "");
        string[] values = v.Split(',');
        return new Vector3(
            float.Parse(values[0]),
            float.Parse(values[1]),
            float.Parse(values[2])
        );
    }

    bool IsTextObject(GameObject obj)
    {
        return obj.CompareTag("text") || obj.name.ToLower().Contains("label") || obj.GetComponent<TMP_Text>() != null;
    }
}
