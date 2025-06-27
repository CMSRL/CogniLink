using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[Serializable]
    public struct GazeInfo
    {
        public string address;
        public Vector3 gazePos;
        public Vector3 gazeDir;


        public GazeInfo( string address = "", Vector3 gazePos = default , Vector3 gazeDir = default )
        {
            this.address = address;
            this.gazePos = gazePos;
            this.gazeDir = gazeDir;
        }
    }


