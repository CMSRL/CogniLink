using UnityEngine;

using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private List<GridGenerator> gridGenerators = new List<GridGenerator>();
    public int Score { get; private set; }

    //private GameObject currentTargetIndicatorInstance;
    public GameObject currentTargetShape { get; private set; }
    public float targetChangeInterval = 10f;
    public GameObject targetIndicator { get; private set; }
    private Coroutine changeTargetCoroutine;
    private int cloneCounter = 0;

    GameObject[] shapesGM;
    List<GameObject> allShapesGM;

    public bool player = false;
    public GameObject playerIndicator;

    public static GameManager instance
    {
        get
        {
            if (Instance == null)
            {
                // If no instance exists, find it in the scene
                Instance = FindObjectOfType<GameManager>();

                if (Instance == null)
                {
                    // If still null, create a new GameObject with this component
                    GameObject singletonObject = new GameObject("GameManager");
                    Instance = singletonObject.AddComponent<GameManager>();
                }
            }

            return Instance;
        }
        private set
        {
                // Assign a new instance
                Instance = value;
            
        }


    }

    private void Awake()
    {
         if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;

       gridGenerators.AddRange(FindObjectsOfType<GridGenerator>());


    }

    public void SetScore(int newScore)
    {
        Score = newScore;
    }
   
    public int GetScore()
    {
        return Score;
    }

    public void IncrementScore()
    {
        Score++;
    }

    public void SetCurrentTargetShape(GameObject shape)
    {
        currentTargetShape = shape;
        //GameBoard.Instance.target = shape.name;
    }
    
    public GameObject GetCurrentTargetShape()
    {
        return currentTargetShape;
    }

     public void SetShapes( GameObject[] shapes, List<GameObject> allShapes)
    {
        shapesGM = shapes;
        allShapesGM = allShapes;
    }

     public string OnShapeClicked(GameObject shape ,  GameObject[] shapes, List<GameObject> allShapes)
    {
        // if (shape.name.StartsWith(currentTargetShape.name))
        // {
            
            Debug.Log("Target hit!");
            IncrementScore();
             foreach (var gridGenerator in gridGenerators)
            {
                if (gridGenerator != null)
                {
                    gridGenerator.UpdateScoreDisplay();
                
                }
            }
           
            string newShapeName = RespawnRandomShapeAfterDelay(shape, shapes, allShapes);
            if (AllTargetsDestroyed( allShapes))
            {
                if (changeTargetCoroutine != null)
                {
                    StopCoroutine(changeTargetCoroutine);
                }
                StartCoroutine(SetNewTargetAfterDelay(allShapes, shapes));
            }

            return newShapeName;
        //}
    }

    // public void SetNewTarget( string thisTarget)
    // {
    //     while (allShapesGM == null || allShapesGM.Count == 0 || shapesGM == null || shapesGM.Length == 0)
    //     {
    //         Debug.LogError("allShapesGM list or shapesGM array is empty or null");
    //         return;
    //     }

    //     foreach(var shape in shapesGM)
    //     {
    //          if (shape.name.StartsWith(thisTarget))
    //         {
    //             Debug.Log("target shape name: " + shape.name);
    //             currentTargetShape = shape;
    //             SetCurrentTargetShape(currentTargetShape);

    //         }
    //     }

    //     // Disable interaction for all shapesGM
        
    
    //     foreach (var gridGenerator in gridGenerators)
    //     {
    //         if (gridGenerator != null)
    //         {
    //             gridGenerator.UpdateTargetIndicator();
    //         }
    //     }

       
        
    // }



    public string RespawnRandomShapeAfterDelay(GameObject shape, GameObject[] shapes, List<GameObject> allShapes )
    {
        cloneCounter++;
        Vector3 position = shape.transform.position;
        Vector3 scale = shape.transform.localScale;
        Transform parent = shape.transform.parent;

        shape.SetActive(false);
        //yield return new WaitForSeconds(3f);

        // Choose a random shape prefab
        GameObject randomShapePrefab = shapes[Random.Range(0, shapes.Length)];;
        bool spawn = true;
        while(spawn)
        {
            randomShapePrefab = shapes[Random.Range(0, shapes.Length)];
            if(!shape.name.StartsWith(randomShapePrefab.name))
            {
                spawn = false;
            }
        }
      
        // Instantiate the new random shape
        GameObject newShape = Instantiate(randomShapePrefab, position, Quaternion.identity, parent);
        newShape.transform.localScale = scale;
        newShape.name = $"{newShape.name}_re_{cloneCounter}";

        Debug.Log("Shape Name" + shape.name);

        Debug.Log("All Shapes: " + allShapes.Count);
        int index = allShapes.IndexOf(shape);
        if (index >= 0)
        {
            allShapes[index] = newShape;
        }
        else
        {
            Debug.LogWarning($"[RespawnRandomShapeAfterDelay] Shape {shape.name} not found in allShapes!");
        }
        
       
        return newShape.name;
    }


     public bool AllTargetsDestroyed(List<GameObject> allShapes)
    {
        return allShapes.Find(s => s.activeSelf && s.name.StartsWith(GetCurrentTargetShape().name)) == null;
    }

    public IEnumerator SetNewTargetAfterDelay(List<GameObject> allShapes,  GameObject[] shapes)
    {
        yield return new WaitForSeconds(3f);
        SetNewRandomTarget(allShapes, shapes);
        RestartChangeTargetRoutine(allShapes, shapes);
    }

    public void RestartChangeTargetRoutine(List<GameObject> allShapes, GameObject[] shapes)
    {
        if (changeTargetCoroutine != null)
        {
            StopCoroutine(changeTargetCoroutine);
        }
        changeTargetCoroutine = StartCoroutine(ChangeTargetRoutine(allShapes, shapes));
    }

    public IEnumerator ChangeTargetRoutine(List<GameObject> allShapes, GameObject[] shapes)
    {
        while (true)
        {
            yield return new WaitForSeconds(targetChangeInterval);
            SetNewRandomTarget(allShapes, shapes);

            
        }
    }


    public void UpdateVisibility(string thisTarget, bool thisPlayer)
    {
       // bool thisPlayer = player;
        string targetShapeName = thisTarget;
        foreach (var shape in allShapesGM)
        {
                    
            if (shape != null)
            {
                var interactable = shape.GetComponent<StatefulInteractable>();
                if (interactable != null)
                {
                    interactable.enabled = false;
                    foreach (var renderer in shape.GetComponentsInChildren<Renderer>())
                    {
                        renderer.enabled = true;
                    }
                }

                 if (shape != null && shape.name.StartsWith(targetShapeName) && thisPlayer == true)
                {
                    // var interactable = shape.GetComponent<StatefulInteractable>();
                    if (interactable != null)
                    {
                        interactable.enabled = true;
                    }

                    foreach (var renderer in shape.GetComponentsInChildren<Renderer>())
                    {
                        renderer.enabled = false;
                    }
                }
       
                    
                
            }
        }
    }


    public void SetNewRandomTarget(List<GameObject> allShapes, GameObject[] shapes)
    {
        if (allShapes == null || allShapes.Count == 0 || shapes == null || shapes.Length == 0)
        {
            Debug.LogError("allShapes list or shapes array is empty or null");
            return;
        }

 

       var prevTargetShape = currentTargetShape;

        while(prevTargetShape == currentTargetShape)
        {
            currentTargetShape = shapes[Random.Range(0, shapes.Length)];
        }
       
    
       // currentTargetShape = shapes[Random.Range(0, shapes.Length)];
     

        SetCurrentTargetShape(currentTargetShape);


        foreach(var shape in allShapes)
         {
                    
            if (shape != null)
            {
                var interactable = shape.GetComponent<StatefulInteractable>();
                if (interactable != null)
                {
                    interactable.enabled = false;
                   
                }

                 if (shape != null && shape.name.StartsWith(currentTargetShape.name))
                {
                    // var interactable = shape.GetComponent<StatefulInteractable>();
                    if (interactable != null)
                    {
                        interactable.enabled = true;
                    }

                }
       
                    
                
            }
        }
    
        foreach (var gridGenerator in gridGenerators)
        {
            if (gridGenerator != null)
            {
                gridGenerator.UpdateTargetIndicator();
            }
        }

       
        
    }

    //  private void OnDestroy()
    // {
    //     // Clean up the instance when the object is destroyed
    //     if (instance == this)
    //     {
    //         instance = null;
    //     }
    // }





}
