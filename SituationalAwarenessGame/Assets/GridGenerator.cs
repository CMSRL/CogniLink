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
    public int rows = 5;
    public int columns = 5;
    private int c = 0;
    public int cellRows = 4; // Number of rows in each small cell
    public int cellCols = 4; // Number of columns in each small cell
    public float interCellSpacing = 0f; // Spacing between the small cells

    private GameObject currentTargetShape;
    public float cellSpacing = 0.4f;
    public GameObject[] shapes;
    private List<GameObject> allShapes = new List<GameObject>();
    private GameObject targetIndicator;
    public GameObject currentTargetIndicatorInstance;
    private GameObject prevTargetInstance;
    public string fileLine;
    
    private System.Random randoms = new System.Random();
    public Vector3 targetIndicatorPosition = new Vector3(-0.5f, 1f, 0); // Position above top-left cell
    public GameObject targetIndicatorPrefab; // Assign this in the inspector

    private GameObject scoreIndicator;
    public GameObject scoreIndicatorPrefab;
    public GameObject scoreTextPrefab;
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

    public GameObject randomWordTextPrefab; // Assign a prefab with TextMeshPro in the inspector
    private TextMeshPro randomWordText;
    private List<string> randomWords = new List<string> { "Elephant", "Galaxy", "Toaster", "Python", "Quantum", "Rainbow", 
        "Banana", "Octopus", "Volcano", "Tornado", "Spaceship", "Penguin",
        "Jellyfish", "Cactus", "Unicorn", "Meteor", "Chocolate", "Castle",
        "Robot", "Dinosaur", "Bicycle", "Laptop", "Compass", "Pineapple", "Dolphin", "Friend",
        "Rock", "Star", "North", "South", "East", "West", "Tweleve", "Three" };

    private Coroutine randomWordCoroutine;


    // Gray Cube Implementation

       public GameObject grayCubePrefab;
    private GameObject grayCube;
    private Coroutine grayCubeCoroutine;
    public int currentPattern = 1;
    private float moveInterval = 3f;

    private Vector3[] clockWisePattern = new Vector3[]
    {
        new Vector3(0, 0, 1), new Vector3(0, 5, 1), new Vector3(5, 5, 1), new Vector3(5, 0, 1)
    };

    private Vector3[] antiClockWisePattern = new Vector3[]
    {
        new Vector3(0, 0, 1), new Vector3(5, 0, 1), new Vector3(5, 5, 1), new Vector3(0, 5, 1)
    };

    private Vector3[] xPattern = new Vector3[]
    {
        new Vector3(0, 0, 1), new Vector3(0, 5, 1),  new Vector3(5, 0, 1), new Vector3(5, 5, 1)
    };

    private CSVLogger logger;
    void Start()
    {
        logger = FindObjectOfType<CSVLogger>();
        GenerateGrid();
        AddGridLabels();
        CreateTargetIndicator();
        UpdateScoreDisplay();
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
        }
        GameManager.Instance.SetShapes(shapes, allShapes);
        string thisTarget = GameBoard.Instance.target;
        GameManager.Instance.SetNewTarget(thisTarget);
        //GameManager.Instance.RestartChangeTargetRoutine(allShapes, shapes);
       

        AddRandomWordText();
        if (randomWordCoroutine == null)
        {
            randomWordCoroutine = StartCoroutine(UpdateRandomWord());
        }

        CreateGrayCube();
        StartGrayCubeMovement();

    }

    void Update()
    {
       
    }


    void GenerateGrid()
    {
        
        
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                    for (int pos = 0; pos < 4; pos++)
                        {
                            // Instantiate cell at offset position
                            Vector3 localPosition = new Vector3(row * cellSpacing, col * cellSpacing, 0f) + cellOffsets[pos];
                            GameObject cell = Instantiate(cellPrefab, transform);
                            cell.transform.localPosition = localPosition;

                            // Try to find a matching grid cell
                            foreach (GridCell gridCell in GameBoard.Instance.gridObjects)
                            {
                                if (gridCell.row == row && gridCell.column == col && gridCell.pos == pos)
                                {
                                    // Shape local position scaled down for placing on cell
                                    Vector3 shapeLocalPosition = new Vector3(row * cellSpacing * 0.04f, col * cellSpacing * 0.04f, 0f);

                                    GameObject shape = SpawnShapeAtCell(cell, shapeLocalPosition, gridCell);
                                    //Debug.Log($"Shape Spawned at pos {pos}: {shape}");
                                    allShapes.Add(shape);
                                    break;
                                }
                            }
                        }
                    
                
            }
        }

        AddGridLabels();
    }


    public void UpdateGridChanges()
    {
         GridCell gridCell;

            
           for (int i = 0; i < GameBoard.Instance.gridObjects.Count; i++)
            {
                gridCell = GameBoard.Instance.gridObjects[i];
                if (gridCell.status == "Respawn")
                {                    
                    // First:
                    // When the row and column match, print the name of the GridCell
                    Vector3 localPosition = new Vector3(gridCell.row * cellSpacing, gridCell.column * cellSpacing, 0f) + cellOffsets[gridCell.pos];
                    GameObject cell = Instantiate(cellPrefab, transform);
                    cell.transform.localPosition = localPosition;
                    Vector3 shapeLocalPosition =  new Vector3(gridCell.row * cellSpacing*0.04f, gridCell.column * cellSpacing*0.04f, 0);
                    GameObject shape = SpawnShapeAtCell(cell, shapeLocalPosition, gridCell);
                    allShapes.Add(shape);
                   
                    GameManager.Instance.IncrementScore();
                    UpdateScoreDisplay();
                    gridCell.status = "Still";
                    GameBoard.Instance.gridObjects[i] = gridCell; 
                    GameBoard.Instance.gameBoardChanged++;

                    string ShotLocation = gridCell.row +","+ gridCell.column + "," +  gridCell.pos ;
                    logger.ShotLocation = ShotLocation;
                

                }

                
            }   

            
    }

    public GameObject SpawnShapeAtCell(GameObject cell, Vector3 position, GridCell gridCell)
    {
        cloneCounter++;

        // Extract base prefab name before the last '_'
        string baseName = gridCell.name.Split('_')[0];
        baseName = baseName.Split('(')[0];
        

        // Find the prefab with matching name
        GameObject shapePrefab = null;
        foreach (var shape in shapes)
        {
            if (shape.name == baseName)
            {
                shapePrefab = shape;
                break;
            }
        }

        if (shapePrefab == null)
        {
            Debug.LogWarning($"Shape prefab with name '{baseName}' not found in shapes[]!");
            return null;
        }

        Vector3 shapeLocalPosition = position + shapeOffset;
        RemoveShapeByGridPosition(gridCell.row, gridCell.column, gridCell.pos);
        GameObject shapeInstance = Instantiate(shapePrefab, cell.transform);
        //Debug.Log("shapeInstance: " + shapeInstance.name);
        shapeInstance.name = $"{baseName}_{gridCell.row}_{gridCell.column}_{gridCell.pos}";
        shapeInstance.transform.localPosition = shapeLocalPosition + new Vector3(-0.1f, 0.4f, 1f);
        shapeInstance.transform.localScale = (Vector3.one + sizeOffset) * 0.5f;

        if (shapeInstance.GetComponent<Collider>() == null)
        {
            shapeInstance.AddComponent<BoxCollider>();
        }

        var interactable = shapeInstance.AddComponent<StatefulInteractable>();
        interactable.selectMode = InteractableSelectMode.Single;
        interactable.OnClicked.AddListener(() => GameManager.Instance.OnShapeClickedWrapper(shapeInstance, shapes, allShapes));

        return shapeInstance;
    }

   
    public void RemoveShapeByGridPosition(int targetRow, int targetCol, int targetPos)
    {
        GameObject targetShape = null;

        // Find the shape matching the row, column, pos
        targetShape = allShapes.FirstOrDefault(shape =>
        {
            string[] parts = shape.name.Split('_');
            if (parts.Length < 4) return false;

            // Try to parse the last three parts of the name as integers
            if (int.TryParse(parts[parts.Length - 3], out int row) &&
                int.TryParse(parts[parts.Length - 2], out int col) &&
                int.TryParse(parts[parts.Length - 1], out int pos))
            {
                return row == targetRow && col == targetCol && pos == targetPos;
            }
            return false;
        });

        if (targetShape != null)
        {
            // Debug.Log($"Removing shape: {targetShape.name}");

            // Remove from the list first
            allShapes.Remove(targetShape);

            // Create a local variable for the Destroy call
            GameObject shapeToDestroy = targetShape;

            //Then destroy on the main thread, but check if the object has become null between
            //removal and destruction
            if(shapeToDestroy != null)
            {
                // UnityEngine.WSA.Application.InvokeOnAppThread(() =>
                // {
                        try
                        {
                            Destroy(shapeToDestroy);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("Exception during Destroy: " + e.ToString());
                        }
                // }, true);
            }
        }
        else
        {
            Debug.LogWarning($"No shape found at row={targetRow}, column={targetCol}, pos={targetPos}");
        }
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
            label.name = label.name + tmp.text;
        }
    }

    public void UpdateTargetIndicator()
    {
        c++;
            if (targetIndicator != null)
        {
            if(prevTargetInstance!= null)
            {
                Destroy(prevTargetInstance);
            }

            if (currentTargetIndicatorInstance != null)
            {
                prevTargetInstance = currentTargetIndicatorInstance;
                currentTargetIndicatorInstance.SetActive(false);
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

               // currentTargetShape.name = currentTargetShape.name + "_" + c;

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

    void UpdateScoreDisplay()
    {

        if (scoreText != null)
        {
        
        int currentScore =  GameManager.Instance.GetScore();
        scoreText.text = "Score: " + currentScore;
        scoreText.fontSize = 5f;
        logger.Score = ""+currentScore;
        }
        else
        {
            Debug.LogError("scoreText is null in UpdateScoreDisplay");
        }
    }

      void AddRandomWordText()
    {
        GameObject randomWordTextObject = Instantiate(randomWordTextPrefab, transform);
        randomWordTextObject.transform.localPosition = new Vector3(6f, 9f, 0); // Adjust to position at the top-left corner of the grid
        randomWordText = randomWordTextObject.GetComponent<TextMeshPro>();

        if (randomWordText != null)
        {
            randomWordText.text = "Loading...";
            randomWordText.fontSize = 5f; // Adjust font size as needed
            randomWordText.alignment = TextAlignmentOptions.Left;
        }
        else
        {
            Debug.LogError("TextMeshPro component not found in randomWordTextPrefab");
        }
    }

    IEnumerator UpdateRandomWord()
    {
        while (true)
        {
            if (randomWordText != null)
            {
                string newWord = randomWords[Random.Range(0, randomWords.Count)];
                randomWordText.text = $"Word: {newWord}";
                logger.Word = newWord;

            }
            yield return new WaitForSeconds(20f); // Change word every 20 seconds
        }
    }

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
            Vector3 newPosition = new Vector3((targetCell.x * cellSpacing) + 0.35f , (targetCell.y * cellSpacing) + 0.35f, 0.1f);
            logger.GrayBoxPos =  targetCell.x + "," + targetCell.y ;
            
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
                return clockWisePattern;
            case 2:
                return antiClockWisePattern;
            case 3:
                return xPattern;
            default:
                return clockWisePattern;
        }
    }




}
