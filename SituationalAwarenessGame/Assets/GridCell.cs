using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[Serializable]
    public struct GridCell
{
    public int row, column, pos;
    public string name;
    public string status;


    public GridCell(int row, int col, int pos, string name, string status )
    {
        this.row = row;
        this.column = col;
        this.pos = pos;
        this.name = name;
        this.status = status;
    }
}


