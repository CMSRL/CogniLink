using UnityEngine;

using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private List<GridGenerator> gridGenerators = new List<GridGenerator>();
    public int Score { get; private set; }

    private GameObject currentTargetIndicatorInstance;
    public GameObject currentTargetShape { get; private set; }
    public float targetChangeInterval = 10f;
    public GameObject targetIndicator { get; private set; }
    private Coroutine changeTargetCoroutine;
    private int cloneCounter = 0;
    public int player = 1;

    GameObject[] shapesGM;
    List<GameObject> allShapesGM;

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
       Debug.Log("grid gen len:" + gridGenerators.Count);
        // if (Instance == null)
        // {
        //     Instance = this;
        //     DontDestroyOnLoad(gameObject);
        // }
        // else
        // {
        //     Destroy(gameObject);
        // }
    }

    public void SetShapes( GameObject[] shapes, List<GameObject> allShapes)
    {
        shapesGM = shapes;
        allShapesGM = allShapesGM;
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
        Debug.Log("Setcurrenttarget: " + shape.name);
        GameBoard.Instance.target = shape.name;
    }
    
    public GameObject GetCurrentTargetShape()
    {
        return currentTargetShape;
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
        // }

    }

    

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

            if (player == 1)
            {
                Debug.Log("player is 2 now");
                player = 2;
            }
            else{
                Debug.Log("player is 1 now");
                player = 1;
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
       
     

        SetCurrentTargetShape(currentTargetShape);
    
        foreach (var gridGenerator in gridGenerators)
        {
            if (gridGenerator != null)
            {
                gridGenerator.UpdateTargetIndicator();
            }
        }

       
        
    }






}
