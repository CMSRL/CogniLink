using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.CompilerServices;

[CreateAssetMenu(fileName = "DeviceConfiguration", menuName = "CogniLink/Device Configuration", order = 1)]
public class DeviceConfigurationScriptableObject : ScriptableObject
{
    public GameObject targetPrefab;
    public List<string> DeviceIPAddresses = new List<string>();
}

