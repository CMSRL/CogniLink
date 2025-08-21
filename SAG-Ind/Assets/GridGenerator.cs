using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Linq;



public class GridGenerator : MonoBehaviour
{
    
    public GameObject cellPrefab;
    public int rows = 6;
    public int columns = 6;

    public int cellRows = 4; // Number of rows in each small cell
    public int cellCols = 4; // Number of columns in each small cell
    public float interCellSpacing = 0f; // Spacing between the small cells

    private GameObject currentTargetShape;
    public float cellSpacing = 0.4f;
    public GameObject[] shapes;
    private List<GameObject> allShapes = new List<GameObject>();
    private GameObject targetIndicator;
    public GameObject currentTargetIndicatorInstance;

    
    private System.Random randoms = new System.Random();
    public Vector3 targetIndicatorPosition = new Vector3(-0.5f, 1f, 0); // Position above top-left cell
    public GameObject targetIndicatorPrefab; // Assign this in the inspector

    private GameObject scoreIndicator;
    public GameObject scoreIndicatorPrefab;
    private TextMeshPro scoreText;


    public Vector3 shapeOffset = new Vector3(0.2f, 0.2f, -1);
    Vector3 sizeOffset = new Vector3(-1.5f, -1.5f, 0);

    private Vector3 shapeLocalPosition_0;
    private Vector3 shapeLocalPosition_1;
    private Vector3 shapeLocalPosition_2;
    private Vector3 shapeLocalPosition_3;

    private Vector3[] cellOffsets = new Vector3[]
    {
        new Vector3(0f, 0f, 0f),
        new Vector3(0.65f, 0f, 0f),
        new Vector3(0f, 0.65f, 0f),
        new Vector3(0.65f, 0.65f, 0f)
    };

    private int cloneCounter = 0;
    public GameObject labelPrefab; // Assign this in the inspector
    public float labelOffset = 0.1f;

    // public GameObject randomWordTextPrefab; // Assign a prefab with TextMeshPro in the inspector
    // private TextMeshPro randomWordText;
    // private List<string> randomWords = new List<string> { "Elephant", "Galaxy", "Toaster", "Python", "Quantum", "Rainbow", 
    //     "Banana", "Octopus", "Volcano", "Tornado", "Spaceship", "Penguin",
    //     "Jellyfish", "Cactus", "Unicorn", "Meteor", "Chocolate", "Castle",
    //     "Robot", "Dinosaur", "Bicycle", "Laptop", "Compass", "Pineapple" };

    // private Coroutine randomWordCoroutine;


    // Gray Cube Implementation

       public GameObject grayCubePrefab;
    private GameObject grayCube;
    private Coroutine grayCubeCoroutine;
    public int currentPattern = 1;
    private float moveInterval = 3f;

    private Vector3[] trianglePattern = new Vector3[]
    {
        new Vector3(0, 0, 0), new Vector3(1, 1, 0), new Vector3(2, 2, 0), new Vector3(1, 3, 0), new Vector3(0, 4, 0)
    };

    private Vector3[] squarePattern = new Vector3[]
    {
        new Vector3(0, 0, 0), new Vector3(0, 4, 0), new Vector3(4, 4, 0), new Vector3(4, 0, 0)
    };

    private Vector3[] xPattern = new Vector3[]
    {
        new Vector3(0, 0, 0), new Vector3(2, 2, 0), new Vector3(4, 4, 0), new Vector3(0, 4, 0), new Vector3(2, 2, 0), new Vector3(4, 0, 0)
    };


    void Start()
    {
        GenerateGrid();
        AddGridLabels();
        CreateTargetIndicator();
        UpdateScoreDisplay();
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
        }
        GameManager.Instance.SetShapes(shapes, allShapes);

        GameManager.Instance.SetNewRandomTarget(allShapes, shapes);
        GameManager.Instance.RestartChangeTargetRoutine(allShapes, shapes);
        // string thisTarget = GameBoard.Instance.target;
        // GameManager.Instance.SetNewTarget(thisTarget);
        //GameManager.Instance.RestartChangeTargetRoutine(allShapes, shapes);
       

        // AddRandomWordText();
        // if (randomWordCoroutine == null)
        // {
        //     randomWordCoroutine = StartCoroutine(UpdateRandomWord());
        // }

        CreateGrayCube();
        StartGrayCubeMovement();

    }

    void Update()
    {
       
    }


   void GenerateGrid()
    {
       // GridCell gridCell = new GridCell(0,0,0,"none","Still");
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
            
                     int randomNumber = Random.Range(1,4);
                     bool occured = false;

                    // First:

                    
                        occured = true;
                        Vector3 localPosition = new Vector3(row * cellSpacing, col * cellSpacing, 0);
                        GameObject cell = Instantiate(cellPrefab, transform);
                        cell.transform.localPosition = localPosition;
                    if (randomNumber != 2)
                    {
                        Vector3 shapeLocalPosition = new Vector3(row * cellSpacing*0.04f, col * cellSpacing*0.04f, 0);
                        GameObject shape = SpawnShapeInCell(cell, shapeLocalPosition);
                        allShapes.Add(shape);
                   
                    }
                    
                    randomNumber = Random.Range(1,4);
                    // Second:
                   
                        occured = true;
                        localPosition = new Vector3((row * cellSpacing) + 0.65f, (col * cellSpacing), 0);
                        GameObject cell1 = Instantiate(cellPrefab, transform);
                        cell1.transform.localPosition = localPosition;
                     if (randomNumber != 2)
                    {
                        Vector3 shapeLocalPosition = new Vector3(row * cellSpacing*0.04f, col * cellSpacing*0.04f, 0);
                        GameObject shape = SpawnShapeInCell(cell1, shapeLocalPosition);
                        allShapes.Add(shape);
                 
                    }
                    
                    // Third:
                     randomNumber = Random.Range(1,4);
                    
                        occured = true;
                       localPosition = new Vector3((row * cellSpacing) , (col * cellSpacing) + 0.65f, 0);
                        GameObject cell2 = Instantiate(cellPrefab, transform);
                        cell2.transform.localPosition = localPosition;
                    if (randomNumber != 2)
                    {
                        Vector3 shapeLocalPosition = new Vector3(row * cellSpacing*0.04f, col * cellSpacing*0.04f, 0);
                        GameObject shape = SpawnShapeInCell(cell2, shapeLocalPosition);
                        allShapes.Add(shape);
                        
                    }

                    // Fourth:
                    randomNumber = Random.Range(1,4);
                    
                        occured = true;
                        localPosition = new Vector3((row * cellSpacing) + 0.65f, (col * cellSpacing) + 0.65f, 0);
                        GameObject cell3 = Instantiate(cellPrefab, transform);
                        cell3.transform.localPosition = localPosition;
                    if (randomNumber != 2 || occured == false)
                    {
                        Vector3 shapeLocalPosition = new Vector3(row * cellSpacing*0.04f, col * cellSpacing*0.04f, 0);
                        GameObject shape = SpawnShapeInCell(cell3, shapeLocalPosition);
                        allShapes.Add(shape);
                        
                    }
                    
                
            }
        }

        //GameBoard.Instance.gridObjects = gridObjects;
        AddGridLabels();
    }


    GameObject SpawnShapeInCell(GameObject cell, Vector3 position)
    {
        cloneCounter++;
        GameObject randomShape = shapes[Random.Range(0, shapes.Length)];
        Vector3 shapeLocalPosition = position + shapeOffset;

        GameObject shapeInstance = Instantiate(randomShape, cell.transform);
        shapeInstance.name = $"{shapeInstance.name}_{cloneCounter}";
        shapeInstance.transform.localPosition = shapeLocalPosition + new Vector3(-0.1f,0.4f,1f);
        shapeInstance.transform.localScale = (Vector3.one + sizeOffset)*0.5f;

        if (shapeInstance.GetComponent<Collider>() == null)
        {
            shapeInstance.AddComponent<BoxCollider>();
        }

        var interactable = shapeInstance.AddComponent<StatefulInteractable>();
        interactable.selectMode = InteractableSelectMode.Single;
        interactable.OnClicked.AddListener(() => GameManager.Instance.OnShapeClicked(shapeInstance, shapes, allShapes));


        return shapeInstance;
    }

    void AddGridLabels()
    {
        // Add column labels (numbers)
        for (int col = 0; col < columns; col++)
        {
            CreateLabel((col + 1).ToString(), new Vector3((col) * (cellSpacing) + 0.3f, -labelOffset, 0));
        }

        // Add row labels (letters)
        for (int row = rows -1 ; row >= 0; row--)
        {
            CreateLabel(((char)('F' - row)).ToString(), new Vector3(-labelOffset, (row) * (cellSpacing) + 0.3f, 0));
        }
    }

    void CreateLabel(string text, Vector3 localPosition)
    {
        GameObject label = Instantiate(labelPrefab, transform);
        label.transform.localPosition = localPosition;
        TextMeshPro tmp = label.GetComponent<TextMeshPro>();
        if (tmp != null)
        {
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 5; // Adjust as needed
        }
    }



    public void UpdateTargetIndicator()
    {
            if (targetIndicator != null)
        {
            if (currentTargetIndicatorInstance != null)
            {
                Destroy(currentTargetIndicatorInstance);
            }
            
            // GameObject targetIndicator = GameManager.Instance.GetCurrentTargetIndicator();
            TextMeshPro tmpText = targetIndicator.GetComponentInChildren<TextMeshPro>();
            currentTargetShape = GameManager.Instance.GetCurrentTargetShape();
            // get from target struct

            if (tmpText != null)
            {
                float textWidth = tmpText.preferredWidth;
                
                currentTargetIndicatorInstance = Instantiate(currentTargetShape, targetIndicator.transform);
                if (currentTargetIndicatorInstance != null)
                {
                    currentTargetIndicatorInstance.transform.localPosition = new Vector3(textWidth + 0.1f, -0.5f , 0);
                    //currentTargetIndicatorInstance.transform.localEulerAngles += new Vector3(180f, 0f, 0f);

                    
                    //currentTargetIndicatorInstance.transform.localScale = Vector3.one; //* 0.5f;
                }

                //GameManager.Instance.SetCurrentTargetIndicatorInstance()
            }
            if (currentTargetShape != null)
            {
                Debug.Log($"New target set: {currentTargetShape.name}");
            }
            else
            {
                Debug.LogError("currentTargetShape is null in UpdateTargetIndicator");
            }

            
        }

       

    }

    void CreateTargetIndicator()
    {
        targetIndicator = Instantiate(targetIndicatorPrefab, transform);
        targetIndicator.transform.localPosition = targetIndicatorPosition;
        
        TextMeshPro tmpText = targetIndicator.GetComponentInChildren<TextMeshPro>();
        tmpText.text = "Target:";
        tmpText.fontSize = 5f;

        scoreIndicator = Instantiate(scoreIndicatorPrefab, targetIndicator.transform);
        scoreIndicator.transform.localPosition = targetIndicatorPosition + new Vector3(9,-11 , 0); // Position above target indicator
        
        scoreText = scoreIndicator.GetComponentInChildren<TextMeshPro>();

        if (scoreText != null)
        {
            UpdateScoreDisplay();
        }
        else
        {
            Debug.LogError("TextMeshPro component not found in scoreIndicator");
        } 

    }

    public void UpdateScoreDisplay()
    {

        if (scoreText != null)
        {
        scoreText.text = "Score: " + GameManager.Instance.GetScore();
        scoreText.fontSize = 5f;
        }
        else
        {
            Debug.LogError("scoreText is null in UpdateScoreDisplay");
        }
    }

    //   void AddRandomWordText()
    // {
    //     GameObject randomWordTextObject = Instantiate(randomWordTextPrefab, transform);
    //     randomWordTextObject.transform.localPosition = new Vector3(6f, 9f, 0); // Adjust to position at the top-left corner of the grid
    //     randomWordText = randomWordTextObject.GetComponent<TextMeshPro>();

    //     if (randomWordText != null)
    //     {
    //         randomWordText.text = "Loading...";
    //         randomWordText.fontSize = 5f; // Adjust font size as needed
    //         randomWordText.alignment = TextAlignmentOptions.Left;
    //     }
    //     else
    //     {
    //         Debug.LogError("TextMeshPro component not found in randomWordTextPrefab");
    //     }
    // }

    // IEnumerator UpdateRandomWord()
    // {
    //     while (true)
    //     {
    //         if (randomWordText != null)
    //         {
    //             string newWord = randomWords[Random.Range(0, randomWords.Count)];
    //             randomWordText.text = $"Word: {newWord}";
    //         }
    //         yield return new WaitForSeconds(20f); // Change word every 20 seconds
    //     }
    // }

    // Gray Cube Implementation

      void CreateGrayCube()
    {
        grayCube = Instantiate(grayCubePrefab, transform);
        grayCube.transform.localScale = new Vector3(1.5f, 1.5f, 0.1f);
        grayCube.transform.localPosition = new Vector3(0, 0, 0);
    }

    void StartGrayCubeMovement()
    {
        if (grayCubeCoroutine != null)
        {
            StopCoroutine(grayCubeCoroutine);
        }
        grayCubeCoroutine = StartCoroutine(MoveGrayCube());
    }

    IEnumerator MoveGrayCube()
    {
        int index = 0;
        Vector3[] shapePattern = GetPatternByShape(currentPattern);

        while (true)
        {
            if (index >= shapePattern.Length)
            {
                index = 0;
            }

            Vector3 targetCell = shapePattern[index];
            Vector3 newPosition = new Vector3((targetCell.x * cellSpacing) + 0.35f , (targetCell.y * cellSpacing) + 0.35f, -0.2f);

            grayCube.transform.localPosition = newPosition;
            index++;
            yield return new WaitForSeconds(moveInterval);
        }
    }
  Vector3[] GetPatternByShape(int shape)
    {
        switch (shape)
        {
            case 1:
                return trianglePattern;
            case 2:
                return squarePattern;
            case 3:
                return xPattern;
            default:
                return trianglePattern;
        }
    }




}
