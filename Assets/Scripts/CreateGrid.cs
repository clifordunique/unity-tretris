using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGrid : MonoBehaviour {
    public const string LAYER_NAME = "Game";
    public Vector3 scale = new Vector3(1.93f, 1.89f, 1f);
    public Vector2 offset = new Vector2(0f, 0f), tileSize = new Vector2(3.2f, 3.2f);
    public Vector2 gridSize = new Vector2(10, 20);
    public List<List<GameObject>> grid = new List<List<GameObject>>();
    private void Awake() {
        Sprite sprite = Resources.Load<Sprite>("Images/grid");
        for (int i = 0; i < gridSize.x; i++) {
            List<GameObject> gos = new List<GameObject>();
            for (int j = 0; j < gridSize.y; j++) {
                gos.Add(SpawnTile(i, j, sprite));
            }
            grid.Add(gos);
        }
        transform.localScale = scale;
    }

    GameObject SpawnTile(int x, int y, Sprite sprite) {
        GameObject g = new GameObject(x + ":" + y);
        g.transform.parent = transform;
        SpriteRenderer tile = g.AddComponent<SpriteRenderer>();
        tile.sprite = sprite;
        tile.sortingLayerName = LAYER_NAME;
        tile.size.Set(tileSize.x, tileSize.y);
        offset = tile.size;

        g.transform.position = transform.position + new Vector3(x * offset.x, y * offset.y, 0);

        g.AddComponent<Grid>().setPos(x,y);
        return g;
    }
}
