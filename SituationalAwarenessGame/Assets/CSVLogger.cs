using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CSVLogger : MonoBehaviour
{
    // public variables //

    // private variables //
    private string currentDateTime = DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
    
    private string CSVDirPath;
    private string GazeFilePath;
    private string GameFilePath;
    private int frameCounter = 0;

    private TextWriter tw;

    // Backing fields
    private string _score;
    private string _word;
    private string _grayBoxPos;
    private string _shotLocation;
    private string _role;
    private Vector3 _gazeHitPoint;
    private Vector3 _gazeHitNormal;
    private string _currentHitObject;
    private string _dwellDuration;
    private string _isFixation;
    private Vector3 _eyeGazePos;
    private Vector3 _eyeGazeDir;
    private bool _pauseStatus;


    // Properties
    public Vector3 GazeHitPoint { get => _gazeHitPoint; set => _gazeHitPoint = value; }
    public Vector3 GazeHitNormal { get => _gazeHitNormal; set => _gazeHitNormal = value; }
    public string CurrentHitObject { get => _currentHitObject; set => _currentHitObject = value; }
    public string DwellDuration { get =>_dwellDuration; set => _dwellDuration = value; }
    public string IsFixation { get => _isFixation; set => _isFixation = value; }
    public string Score { get => _score; set => _score = value; }
    public string Word { get => _word; set => _word = value; }
    public string Role { get => _role; set => _role = value; }
    public string GrayBoxPos { get => _grayBoxPos; set => _grayBoxPos = value; }
    public string ShotLocation { get => _shotLocation; set => _shotLocation = value; }
    public Vector3 EyeGazePos { get => _eyeGazePos; set => _eyeGazePos = value; }
    public Vector3 EyeGazeDir { get => _eyeGazeDir; set => _eyeGazeDir = value; }
    public bool PauseStatus { get => _pauseStatus; set => _pauseStatus = value; }

    private List<string> GameRows = new List<string>();
    private List<string> GazeRows = new List<string>();

    // functions //

    void Awake()
    {
        CSVDirPath = Application.persistentDataPath + "/Logs/";

        // checking for csv directory 
        if (!Directory.Exists(CSVDirPath))
        {
            Directory.CreateDirectory(CSVDirPath);
        }

        //  Performance metrics logger
        
        // create new csv filepath
        GazeFilePath = Application.persistentDataPath + "/Logs/" + "Gaze_" + currentDateTime + ".csv";
        GameFilePath = Application.persistentDataPath + "/Logs/" + "Game_" + currentDateTime + ".csv";
        
        // initiate columns of CSV
        tw = new StreamWriter(GameFilePath, false); // false indicates overwriting the file
        tw.WriteLine("TimeStamp, Score, Word, Role, Frame#, PauseStatus, HighlightPosRow, HighlightPosCol,  ShotLocationRow, ShotLocationCol, ShotLocationPos" ); // edit this to update coloumns of CSV
        tw.Close();

        tw = new StreamWriter(GazeFilePath, false); // false indicates overwriting the file
        tw.WriteLine("TimeStamp, Role, Frame#, EyeGazePos, EyeGazeDir, GazeHitPoint, GazeHitNormal, DwellDuration, IsFixation, CurrentHitObject, PauseStatus"); // edit this to update coloumns of CSV
        tw.Close();
    }

    void Update()
    {
        
        frameCounter++;

        if (frameCounter % 8 == 0)
        {
            LogCurrentGameRow();
        }
        if (frameCounter >= 8000) 
        {
               frameCounter = 0;
        }
    }


    public void LogCurrentGameRow()
    {
        float timeStamp = Time.time;
        int frame = Time.frameCount;
        // Debug.Log("Log current row was called");
        // Debug.Log("row val:" + $"{timeStamp:F2},{_score},{_word},{_role},{frame},{_grayBoxPos},{_shotLocation},{_pauseStatus}");
  
        string row = $"{timeStamp:F2},{_score},{_word},{_role},{frame},{_pauseStatus},{_grayBoxPos}, {_shotLocation}";
        GameRows.Add(row);
    }

    public void LogCurrentGazeRow()
    {
        float timeStamp = Time.time;
        int frame = Time.frameCount;
        // Debug.Log("Log current row was called");
        // Debug.Log("row val:" + $"{timeStamp:F2},{_role},{frame},{FormatVector(_eyeGazePos)}, {FormatVector(_eyeGazeDir)}, {FormatVector(_gazeHitPoint)}, {FormatVector(_gazeHitNormal)}, {_dwellDuration}, {_isFixation}, {_currentHitObject}");
  
        string row = $"{timeStamp:F2},{_role},{frame},{FormatVector(_eyeGazePos)},{FormatVector(_eyeGazeDir)},{FormatVector(_gazeHitPoint)},{FormatVector(_gazeHitNormal)},{_dwellDuration},{_isFixation},{_currentHitObject},{_pauseStatus}";
        GazeRows.Add(row);
    }
 
    private string FormatVector(Vector3 v) => $"\"{v.x:F2},{v.y:F2},{v.z:F2}\"";
    //private string FormatVector(Vector3 v) => $"{v.x:F2},{v.y:F2},{v.z:F2}";

    void OnApplicationPause()
    {
        batchWriteToFiles();
        GazeRows.Clear();
        GameRows.Clear();

    }

    public void batchWriteToFiles()
    {
        tw = new StreamWriter(GazeFilePath, true);
        foreach (var row in GazeRows)
        {
            tw.WriteLine(row);
        }
        tw.Close();

        tw = new StreamWriter(GameFilePath, true);
        foreach (var row in GameRows)
        {
            tw.WriteLine(row);
        }
        tw.Close();
    }



}
