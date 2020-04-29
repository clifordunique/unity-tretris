using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePiece : MonoBehaviour {

    Sprite[] colors;
    public bool isAwake = false;
    private void Awake() {
        colors = Resources.LoadAll<Sprite>("Images/blocks");
        isAwake = true;
    }


    public Shape CreateNewPiece() {
        return CreateNewPiece(UnityEngine.Random.Range(0, 7));
    }
    public Shape CreateNewPiece(int r) {
        return SpawnShape(r);
    }

    private Shape SpawnShape(int v) {
        List<Vector2> shape = new List<Vector2>();
        Sprite sprite = colors[v];
        //Debug.Log("Picked: " + v);
        Vector2 center = new Vector2(1,1);
        switch (v) {
            // 0 line
            case 0:
                for (int i = 0; i < 4; i++)
                    shape.Add(new Vector2(i, 0)); // TODO change this so they are in boxes like: https://vignette.wikia.nocookie.net/tetrisconcept/images/3/3d/SRS-pieces.png/revision/latest?cb=20060626173148
                center = new Vector2(1.5f,0.5f);
                break;
            // 1 square
            case 1:
                for (int i = 1; i <= 2; i++)
                    for (int j = 0; j < 2; j++)
                        shape.Add(new Vector2(i, j));
                center = new Vector2(1.5f,0.5f);
                break;
            // 2 L
            case 2:
                shape.Add(new Vector2(0, 0));
                for (int i = 0; i < 3; i++)
                    shape.Add(new Vector2(i, 1));
                break;
            // 3 reverse L
            case 3:
                shape.Add(new Vector2(2, 0));
                for (int i = 0; i < 3; i++)
                    shape.Add(new Vector2(i, 1));
                break;
            // 4 Z
            case 4:
                for (int i = 1; i <= 2; i++)
                    shape.Add(new Vector2(i, 0));
                for (int i = 0; i < 2; i++)
                    shape.Add(new Vector2(i, 1));
                break;
            // 5 reverse Z
            case 5:
                for (int i = 0; i < 2; i++)
                    shape.Add(new Vector2(i, 0));
                for (int i = 1; i <= 2; i++)
                    shape.Add(new Vector2(i, 1));
                break;
            // 6 half H
            case 6:
                shape.Add(new Vector2(1, 0));
                for (int i = 0; i <= 2; i++)
                    shape.Add(new Vector2(i, 1));
                break;
        }
        return new Shape(shape, sprite, center,v);
    }


}
