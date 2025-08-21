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
    // public float targetChangeInterval = 10f;
    public GameObject targetIndicator { get; private set; }
    private Coroutine changeTargetCoroutine;
    private int cloneCounter = 0;

    GameObject[] shapesGM;
    List<GameObject> allShapesGM;

    public bool player = false;
    public GameObject playerIndicator;
    private CSVLogger logger;
    private int wrongClick = 0;
   
      

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
        logger = FindObjectOfType<CSVLogger>();

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
        GameBoard.Instance.target = shape.name;
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

    public async void OnShapeClickedWrapper(GameObject shape, GameObject[] shapes, List<GameObject> allShapes)
    {
        await OnShapeClicked(shape, shapes, allShapes);
    }

    public async Task OnShapeClicked(GameObject shape ,  GameObject[] shapes, List<GameObject> allShapes)
    {
        if (shape.name.StartsWith(currentTargetShape.name))
        {
            
            Debug.Log("Target hit!");
            //IncrementScore();
            AudioManager.Instance.PlayCorrectHitSound();
            

            string[] nameParts = shape.name.Split('_');

           
            int row = int.Parse(nameParts[nameParts.Length - 3]);
            int column = int.Parse(nameParts[nameParts.Length - 2]);
            int pos = int.Parse(nameParts[nameParts.Length - 1]);


           for (int i = 0; i < GameBoard.Instance.gridObjects.Count; i++)
            {
                var gridCell = GameBoard.Instance.gridObjects[i];
                if (gridCell.row == row && gridCell.column == column && gridCell.pos == pos)
                {
                    // Update the struct by replacing it in the list (since struct is value type)
                    gridCell.status = "Shot";
                    GameBoard.Instance.gridObjects[i] = gridCell;
                    break;
                }
            }

            GameBoard.Instance.gameBoardChanged++;
            //Destroy(shape);
        }
        else 
        {
            
            Debug.Log("Wrong target!");
            wrongClick++;
            logger.WrongClick = wrongClick.ToString();

            AudioManager.Instance.PlayWrongHitSound();
        }
    }

    public void SetNewTarget( string thisTarget)
    {
        while (allShapesGM == null || allShapesGM.Count == 0 || shapesGM == null || shapesGM.Length == 0)
        {
            Debug.LogError("allShapesGM list or shapesGM array is empty or null");
            return;
        }

        foreach(var shape in shapesGM)
        {
             if (shape.name.StartsWith(thisTarget))
            {
                // Debug.Log("target shape name: " + shape.name);
                currentTargetShape = shape;
                SetCurrentTargetShape(currentTargetShape);

            }
        }

        // Disable interaction for all shapesGM
        
    
        foreach (var gridGenerator in gridGenerators)
        {
            if (gridGenerator != null)
            {
                gridGenerator.UpdateTargetIndicator();
            }
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

                 if (shape != null && thisPlayer == true)
                 {
                        if (interactable != null)
                        {
                            interactable.enabled = true;
                        }
                 }

                
                var textPlayerRenderer =  playerIndicator.GetComponent<Renderer>();
                textPlayerRenderer.enabled = thisPlayer;
                logger.Role = thisPlayer.ToString();

                    
                var currentTargetIndicatorInstance = gridGenerators[0].currentTargetIndicatorInstance;
    
                foreach (MeshRenderer mr in currentTargetIndicatorInstance.GetComponentsInChildren<MeshRenderer>())
                {
                 
                    mr.enabled = !(textPlayerRenderer.enabled);
                }


       
                    
                
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
