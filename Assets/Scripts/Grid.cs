using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    public Vector2 pos;
    public bool isPlaced = false, isShape = false;

    internal void setPos(int x, int y) {
        this.pos = new Vector2(x, y);
    }
}
