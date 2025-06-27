using UnityEngine;
using UnityEngine.XR;

public class AvatarController : MonoBehaviour
{
    public Transform avatarHead;
    // public Transform avatarLeftHand;
    // public Transform avatarRightHand;

    void Update()
    {
        UpdateHeadPosition();
       // UpdateHandPosition(XRNode.LeftHand, avatarLeftHand);
        //UpdateHandPosition(XRNode.RightHand, avatarRightHand);
    }

    void UpdateHeadPosition()
    {
        avatarHead.position = Camera.main.transform.position + new Vector3(0, 0, -0.5f);
       avatarHead.rotation = Camera.main.transform.rotation * Quaternion.Euler(-90f, 180f, 0f);

    }

    // void UpdateHandPosition(XRNode handNode, Transform avatarHand)
    // {
    //     InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 position);
    //     InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion rotation);

    //     avatarHand.position = position;
    //     avatarHand.rotation = rotation;
    // }
}
