using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Input;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Threading.Tasks;
using static GameBoard;


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
    private GameObject currentTargetIndicatorInstance;
    private List<GridCell> gridObjects = new List<GridCell>();


    
    private System.Random randoms = new System.Random();
    public Vector3 targetIndicatorPosition = new Vector3(-0.5f, 1f, 0); // Position above top-left cell
    public GameObject targetIndicatorPrefab; // Assign this in the inspector

    private GameObject scoreIndicator;
    public GameObject scoreIndicatorPrefab;
    private TextMeshPro scoreText;

    public Vector3 shapeOffset = new Vector3(0.2f, 0.2f, -1);
    Vector3 sizeOffset = new Vector3(-1.5f, -1.5f, 0);


    private int cloneCounter = 0;
    public GameObject labelPrefab; // Assign this in the inspector
    public float labelOffset = 0.1f;

    public GameObject randomWordTextPrefab; // Assign a prefab with TextMeshPro in the inspector
    private TextMeshPro randomWordText;
    private List<string> randomWords = new List<string> { "Elephant", "Galaxy", "Toaster", "Python", "Quantum", "Rainbow", 
        "Banana", "Octopus", "Volcano", "Tornado", "Spaceship", "Penguin",
        "Jellyfish", "Cactus", "Unicorn", "Meteor", "Chocolate", "Castle",
        "Robot", "Dinosaur", "Bicycle", "Laptop", "Compass", "Pineapple", "Bear" };

    private Coroutine randomWordCoroutine;
    

    void Start()
    {
        GenerateGrid();
        CreateTargetIndicator();
        UpdateScoreDisplay();
        //GameManager.Instance.SetShapes(shapes, allShapes);
        GameManager.Instance.SetNewRandomTarget(allShapes, shapes);
        GameManager.Instance.RestartChangeTargetRoutine(allShapes, shapes);
         if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
        }

        AddRandomWordText();
        if (randomWordCoroutine == null)
        {
            randomWordCoroutine = StartCoroutine(UpdateRandomWord());
        }

    }

    void Update()
    {
      
    }

    public async Task UpdateGridChanges()
    {
          for(int i =0 ; i< GameBoard.Instance.gridObjects.Count; i++)
            {
                Debug.Log("GridCell Status" +  GameBoard.Instance.gridObjects[i].status);
                if(GameBoard.Instance.gridObjects[i].status == "Shot")
                {
                        Debug.Log("GridCell Status: Shot");
                        string baseName = GameBoard.Instance.gridObjects[i].name.Split('_')[0];
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

                    string newShapeName = GameManager.Instance.OnShapeClicked( shapePrefab,  shapes,  allShapes);
                    GridCell gridCell = GameBoard.Instance.gridObjects[i];  
                    gridCell.status = "Respawn";
                    gridCell.name = newShapeName.Split("_")[0];  
                    GameBoard.Instance.gridObjects[i] = gridCell;
                    GameBoard.Instance.gameBoardChanged++;

                }
        }

    }

    void GenerateGrid()
    {
        GridCell gridCell = new GridCell(0,0,0,"none","Still");
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
                        gridCell.row = row;
                        gridCell.column = col;
                        gridCell.pos= 0;
                        gridCell.name= shape.name;
                        gridObjects.Add(gridCell); 
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
                        gridCell.row = row;
                        gridCell.column = col;
                        gridCell.pos= 1;
                        gridCell.name= shape.name;
                        gridObjects.Add(gridCell);
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
                        gridCell.row = row;
                        gridCell.column = col;
                        gridCell.pos= 2;
                        gridCell.name= shape.name;
                        gridObjects.Add(gridCell);
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
                        gridCell.row = row;
                        gridCell.column = col;
                        gridCell.pos= 3;
                        gridCell.name= shape.name;
                        gridObjects.Add(gridCell);
                    }
                    
                
            }
        }

        GameBoard.Instance.gridObjects = gridObjects;
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
            
            if (tmpText != null)
            {
                float textWidth = tmpText.preferredWidth;
                
                currentTargetIndicatorInstance = Instantiate(currentTargetShape, targetIndicator.transform);
                if (currentTargetIndicatorInstance != null)
                {
                    currentTargetIndicatorInstance.transform.localPosition = new Vector3(textWidth + 0.1f, -0.5f , 0);
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
        scoreIndicator.transform.localPosition = targetIndicatorPosition + new Vector3(9,-9 , 0); // Position above target indicator
        
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

      void AddRandomWordText()
    {
        GameObject randomWordTextObject = Instantiate(randomWordTextPrefab, transform);
        randomWordTextObject.transform.localPosition = new Vector3(5f, 8f, 0); // Adjust to position at the top-left corner of the grid
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
            }
            yield return new WaitForSeconds(20f); // Change word every 20 seconds
        }
    }



}
