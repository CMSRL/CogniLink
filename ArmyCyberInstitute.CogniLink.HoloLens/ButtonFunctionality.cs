using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Net;

public class ButtonFunctionality : MonoBehaviour
{
    public GameObject prefabTarget;
    private int cubeCounter = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createTargetCube()
    {
        var parent = GameObject.Find("AnchorParent").transform;
        var newTarget = UnityEngine.Object.Instantiate(prefabTarget, parent);
        newTarget.name = GetLocalIPAddress() + " - Target" + cubeCounter.ToString();
        cubeCounter++;
        newTarget.GetComponent<TargetEntity>().isOwner = true;
        newTarget.GetComponent<Renderer>().material.color = new Color(0, 255, 0);
        GameObject.Find("SocketClient").GetComponent<HoloLensSocketClient>().sceneList[newTarget.name] = new TargetStruct(newTarget.transform.localPosition, newTarget.transform.localRotation, newTarget.transform.localScale, newTarget.name);
    }

    public string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
}
