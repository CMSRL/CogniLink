using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameBoard : MonoBehaviour
{
    public static GameBoard Instance { get; private set; }
    //public string[,,] gridObjects; 
    public List<GridCell> gridObjects;
    public List<GazeInfo> gazeInfo;
    public GazeInfo gazeInfoObject = new GazeInfo();

    // Start is called before the first frame update

    public string target = "";
    //public bool gameBoardChanged = false;
    public int gameBoardChanged = 0;

    // public Vector3 gazePos;
    // public Vector3 gazeDir;
    void Awake()
    {
        gazeInfo.Add(gazeInfoObject);
        gazeInfo.Add(gazeInfoObject);
        
         if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject); // Prevent duplicates
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
