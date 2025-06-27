using MixedReality.Toolkit.Input; // For MRTK3 input handling
using UnityEngine;
using UnityEngine.EventSystems; // For Unity's EventSystem

public class CubeInteraction : MonoBehaviour, IPointerClickHandler
{
    // This method will be triggered when the cube is clicked/tapped
    public void OnPointerClick(PointerEventData eventData)
    {
        // Disable the cube's GameObject to make it disappear
        
        Debug.Log("here");
        gameObject.SetActive(false);
    }
}
