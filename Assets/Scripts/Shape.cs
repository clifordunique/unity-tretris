using System;
using System.Collections.Generic;
using UnityEngine;

public class Shape {

    public List<Vector2> shape;
    public Sprite sprite;
    public Vector2 center;
    public int num;

    public Shape(List<Vector2> shape, Sprite sprite, Vector2 center, int num) {
        this.shape = shape;
        this.sprite = sprite;
        this.center = center;
        this.num = num;
    }

    public override string ToString() {
        string s = sprite.ToString()+" -> ";
        foreach (Vector2 v in shape)
            s += v + ",";
        return s;
    }

    internal void rotate() {
        //cx+cy-y, cy-cx+x
        Vector2 diff = new Vector2(center.x+center.y,center.y-center.x);
        for (int i = 0; i < shape.Count; i++)
            shape[i] = new Vector2(diff.x-shape[i].y, diff.y+shape[i].x);
    }
}