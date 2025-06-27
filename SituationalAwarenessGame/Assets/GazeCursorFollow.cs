using UnityEngine;
using System.Collections;

public class GazeCursorFollowRelative : MonoBehaviour
{
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor gazeRayInteractor;
   
    [SerializeField] private Transform sharedAnchor;

    private Vector3 lastHitPoint;
    private Vector3 lastHitNormal;
    private float checkInterval = 0.05f; // 20 FPS check rate, tweak as needed

    private Vector3 gazeOrigin;
    private Vector3 gazeDirection;

    private GameObject lastHitObject;
    private float dwellStartTime;
    private float dwellDuration;
    private float fixationThreshold = 100f; // deg/sec, for example
    private Vector3 lastGazeDirection;
    private CSVLogger logger;

    void Start()
    {
        logger = FindObjectOfType<CSVLogger>();
        gazeRayInteractor = FindGazeRayInteractor();
        StartCoroutine(CheckGazeRaycast());
    }

    IEnumerator CheckGazeRaycast()
    {
        while (true)
        {
            if (gazeRayInteractor != null && gazeRayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
            {
                gazeOrigin = gazeRayInteractor.rayOriginTransform.position;
                gazeDirection = gazeRayInteractor.rayOriginTransform.forward;


                GameObject currentHitObject = hit.collider?.gameObject;

                if (currentHitObject != lastHitObject)
                {
                    dwellDuration = Time.time - dwellStartTime;
                    if (lastHitObject != null)
                    {
                        Debug.Log($"Dwell on {lastHitObject.name}: {dwellDuration:F2} seconds");
                        // You can log this dwell info to CSVLogger here if desired
                    }
                    dwellStartTime = Time.time;
                    lastHitObject = currentHitObject;
                }
                else
                {
                    dwellDuration = Time.time - dwellStartTime;
                }


                float angularVelocity = Vector3.Angle(lastGazeDirection, gazeDirection) / checkInterval;
                bool isFixation = angularVelocity < fixationThreshold;  

                if (logger != null)
                {
                    logger.EyeGazePos = gazeOrigin;
                    logger.EyeGazeDir = gazeDirection;
                    logger.GazeHitPoint = hit.point;
                    logger.GazeHitNormal = hit.normal;
                    logger.CurrentHitObject = currentHitObject?.name ?? "None";
                    logger.DwellDuration = dwellDuration.ToString();
                    logger.IsFixation = isFixation.ToString();
                    logger.LogCurrentGazeRow();
                }
                
                lastGazeDirection = gazeDirection;


                if (hit.point != lastHitPoint || hit.normal != lastHitNormal)
                {
                    lastHitPoint = hit.point;
                    lastHitNormal = hit.normal;

                    transform.position = hit.point;
                    transform.forward = hit.normal;

                    
                   

                    if (sharedAnchor != null )
                    {
                        Vector3 relativePosition = sharedAnchor.InverseTransformPoint(hit.point);
                        Vector3 relativeDirection = sharedAnchor.InverseTransformDirection(hit.normal);

                        Debug.Log("Relative Pos: " + relativePosition + "Relative Dir" + relativeDirection);
                        string ip = NetworkUtils.GetLocalIPAddress();
                        for(int i =0 ; i < GameBoard.Instance.gazeInfo.Count; i++)
                        {
                            
                            // if(GameBoard.Instance.gazeInfo.Count< i )
                            // {
                            //     Debug.Log("Adding" + i + "element to send as gazeinfo");
                            //     GazeInfo gazetoSend = new GazeInfo(ip,relativePosition, relativeDirection );
                            //     GameBoard.Instance.gazeInfo.Add(gazetoSend);
                            // }
                            
                            // else
                            // {
                                 
                                GazeInfo gazetoSend = GameBoard.Instance.gazeInfo[i];

                               
                                if(gazetoSend.address == ip)
                                {
                                   
                                    gazetoSend.gazePos = relativePosition;
                                    gazetoSend.gazeDir = relativeDirection;
                                    GameBoard.Instance.gazeInfo[i] = gazetoSend;
                                    
                                }
                               
                            // }
                         
                        }
                        

                        // Send updated values over the network if needed
                    }

             
                }
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor FindGazeRayInteractor()
    {
        GameObject gazeGO = GameObject.Find("GazeInteractor");
        if (gazeGO != null)
            return gazeGO.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();

        foreach (var interactor in FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>())
        {
            if (interactor.name.ToLower().Contains("gaze"))
                return interactor;
        }

        Debug.LogWarning("Gaze XRRayInteractor not found!");
        return null;
    }
}
