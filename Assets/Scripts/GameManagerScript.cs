using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManagerScript : MonoBehaviour {
    public float points = 0, speed = 1f, decayDown, decayDownStart = 1f, level = 1, mouseDownTime, touchDownTime, mouseUpTime = 1000, touchUpTime, sleepTimeStart = 1000,
        sleepTime,
decayFloorDown, decayFloorDownStart = 1f, decaySpaceBar, decaySpaceBarStart = 1f, min = .1f, timeOfPressed, decayTouchHor, decayTouchHorStart = .1f;
    public Shape shape, shapeNext, held;
    public bool isFloor = false, firstTimePressed = false, gameOver = false, isMouseDown = false, isTouchDown = false, canHold = true, isPause;
    public CreatePiece createPiece;
    public Vector3 pieceScale = new Vector3(0.125f, 0.125f, 1f), gridScale;
    public Sprite gridSprite, ghostGridSprite;
    public List<List<GameObject>> grid;
    public Vector2 origin = new Vector2(4, 19), tileSize = new Vector2(.25f, .25f),
        position, oldPosition, changeVector = Vector2.zero, fingerStartPos;
    public List<Vector2> oldGhost = new List<Vector2>(), oldShape = new List<Vector2>();
    public Vector3 mouseStartPos;
    string highscorekey = "highscore";
    GameObject next, nextUI, holdUI, hold;
    public GameObject gameOverUI, pauseUI;

    public Text pointsText, levelText, highScore;

    private void Awake() {
        grid = GameObject.Find("Grid").GetComponent<CreateGrid>().grid;
        createPiece = GameObject.FindGameObjectWithTag("PlayArea").GetComponent<CreatePiece>();
        position = origin;
        decayDown = decayDownStart;
        decayFloorDown = decayFloorDownStart;
        UnityEngine.Random.InitState(System.Environment.TickCount);
        ghostGridSprite = Resources.Load<Sprite>("Images/grid-ghost");
    }

    private void Start() {
        // must be in start because some things haven't loaded yet
        shapeNext = createPiece.CreateNewPiece();
        SpriteRenderer gridSR = grid[0][0].GetComponent<SpriteRenderer>();
        gridSprite = gridSR.sprite;
        gridScale = gridSR.transform.localScale;
        highScore.text = PlayerPrefs.GetInt(highscorekey, 0) + "";
        levelText.text = level + "";
        nextUI = GameObject.Find("Next");
        holdUI = GameObject.Find("Hold");
        createNextPiece(shapeNext);
    }

    public void holdClicked() {
        if (!canHold)
            return;
        Shape temp = createPiece.CreateNewPiece(shape.num);
        if (held != null) // then swap current shape with held shape
            shape = held;
        else
            shape = null;
        Destroy(hold);
        hold = setInUI(temp, holdUI, new Vector3(-246, 555, 0));
        foreach (Vector2 v in oldShape)
            changeToGrid((int)v.x, (int)v.y);
        held = temp;
        canHold = false;
        foreach (Vector2 v in oldGhost) // old method didn't work because rotations change where shape was
            if (!grid[(int)v.x][(int)v.y].GetComponent<Grid>().isShape)
                changeToGrid((int)v.x, (int)v.y);
        oldGhost.Clear();
        oldShape.Clear();
        position = origin;
    }
    private void createNextPiece(Shape shapeNext) {
        next = setInUI(shapeNext, nextUI, new Vector3(173, 555, 0));
    }
    private GameObject setInUI(Shape shape, GameObject parent, Vector3 localposition) {
        // create the sprite and add it to the ui
        string LAYER_NAME = "UI";
        GameObject n = new GameObject(parent.name+"Piece");
        n.layer = LayerMask.NameToLayer(LAYER_NAME);
        foreach (Vector2 v in shape.shape) {
            GameObject g = new GameObject(v.x + ":" + v.y);
            SpriteRenderer tile = g.AddComponent<SpriteRenderer>();
            tile.sprite = shape.sprite;
            tile.sortingLayerName = LAYER_NAME;
            tile.size.Set(tileSize.x, tileSize.y);
            Vector2 offset = tile.size;
            g.layer = LayerMask.NameToLayer(LAYER_NAME);

            g.transform.position = transform.position + new Vector3(v.x * offset.x, v.y * offset.y, 0);
            g.transform.parent = n.transform;
        }
        n.transform.parent = parent.transform;
        n.transform.localPosition = localposition;
        n.transform.localScale = new Vector3(20f, 20f, 7.2f);
        return n;
    }

    public void retry() {
        gameOverUI.SetActive(false);
        SceneManager.LoadScene("SampleScene");
    }

    public void pause() {
        isPause = !isPause;
        pauseUI.SetActive(isPause);
    }

    private void FixedUpdate() {
        if (gameOver || isPause)
            return;
        // start mouse controls
        if (Input.GetMouseButtonDown(0)) {
            if(!isMouseDown)
                mouseStartPos = Input.mousePosition;
            isMouseDown = true;
            mouseDownTime = Time.time;
        } if (Input.GetMouseButtonUp(0)) {
            isMouseDown = false;
            mouseUpTime = Time.time;
        }
        // start touch controls
        int tc = Input.touchCount;
        if (tc > 0 && Input.GetTouch(0).phase == TouchPhase.Began) {
            fingerStartPos = Input.GetTouch(0).position;
            touchDownTime = Time.time;
            isTouchDown = true;
        }
        if (tc > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            touchUpTime = Time.time;
            isTouchDown = false;
            fingerStartPos = Input.GetTouch(0).position;
        }
        decayDown = Mathf.Max(decayDown - Time.deltaTime, -1);
        decaySpaceBar = Mathf.Max(decaySpaceBar - Time.deltaTime, -1);
        decayTouchHor = Mathf.Max(decayTouchHor - Time.deltaTime, -1);
        //decayHor = Mathf.Max(decayHor - Time.deltaTime, -1);
        if (shape == null) {
            shape = shapeNext;
            shapeNext = null;
            RenderShape(shape);
        }
        if (shapeNext == null) {
            shapeNext = createPiece.CreateNewPiece();
            Destroy(next);
            createNextPiece(shapeNext);
        }
        oldPosition = position;
        // keyboard/gamepad movement logic
        float dir = Input.GetAxis("Horizontal"), downPress = Input.GetAxis("Vertical");
        // mouse movement logic
        //if(isMouseDown)
        //    Debug.Log(mouseStartPos + " " + Input.mousePosition+" "+ Vector2.Distance(mouseStartPos, Input.mousePosition));
        if (decayTouchHor <= 0 && isMouseDown && Vector2.Distance(mouseStartPos,Input.mousePosition) > 1) {
            Vector2 v = Input.mousePosition - mouseStartPos;
            //Debug.Log(v + " " + v.normalized + " " + v.magnitude + " " + v.normalized.magnitude);
            dir = v.normalized.x;
            decayTouchHor = decayTouchHorStart;
            mouseStartPos = Input.mousePosition;
            if (v.y < -10) {
                downPress = v.y;
                dir = 0;
            }
        }
        // touch movement logic
        if (decayTouchHor <= 0 && isTouchDown && tc > 0 && Input.GetTouch(0).phase == TouchPhase.Moved) {
            Vector2 v = Input.GetTouch(0).position - fingerStartPos;
            dir = v.normalized.x;
            decayTouchHor = decayTouchHorStart;
            fingerStartPos = Input.GetTouch(0).position;
            if (v.y < -10) {
                downPress = v.y;
                dir = 0;
            }
            //float angle = Vector2.Angle(fingerStartPos, Input.GetTouch(0).position);
            //dir = Mathf.Abs(angle) > 90 && Mathf.Abs(angle) < 180 + 90 ? -1 : 1;
        }
        if (decaySpaceBar <= 0 && (Input.GetButton("Jump") ||
            (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended && Math.Abs(touchUpTime - touchDownTime) < .5) ||
            (Input.GetMouseButtonUp(0) && Math.Abs(mouseUpTime - mouseDownTime) < .5))) { // flip piece logic                                flip controls
            changeVector.x = 0;
            rotateShape(); // TODO can clip out of bounds on rotation
            decaySpaceBar = decaySpaceBarStart; // make sure flips don't happen too fast
        }

        if (Input.GetButton("Submit")) {
            Debug.Log("Submit");
            holdClicked();
        }

        if (Mathf.Abs(dir) > min)  //                                                         horizontal controls
            changeVector.x = Mathf.Sign(dir);
        if (downPress < -min) {
            decayDown = Mathf.Min(decayDown, .1f);
            if (firstTimePressed && Time.time - timeOfPressed < 0.5f) {
                // TODO double press to make speed 0
                //decayDown = -1; // doesn't work, need to teleport to bottom somehow probably need ghost image as prerequesite
                //foreach (Vector2 v in oldShape)
                //    changeToGrid((int)v.x, (int)v.y);
                //foreach (Vector2 v in oldGhost)
                //    changeToBlock((int)v.x, (int)v.y,shape.sprite,true);
                //shape = null;
                firstTimePressed = false;
            } else
                firstTimePressed = true;
        } else
            timeOfPressed = Time.time;
        changeVector.y = -1; // minus because it starts at the top ie grid position y = 19  not y = 0

        if (shape != null)
            displayGhost(); 
        // check if you can move in position direction // if not then remove the changeVector
        bool floorChanged = false;
        if(shape != null)
            foreach (Vector2 v in shape.shape) {
                int x = (int)(v.x + position.x + changeVector.x);
                // -1 = 0+ 0 + -1 // 0 1 -1 0
                if (x < 0 || x >= grid.Count ||
                    ((int)(v.y + position.y) < grid[x].Count // for overhang
                        && grid[x][(int)(v.y + position.y)].GetComponent<Grid>().isPlaced)) // then you can't move in the designated x direction
                    changeVector.x = 0; // this isn't necessarily 0 due to rotation
                x = (int)(v.x + position.x + changeVector.x);
                int y = (int)(v.y + position.y + changeVector.y);
                //Debug.Log(x + "," + y + " : "+v+" -> "+position+" -> "+changeVector);
                // TODO exception here when rotating out of bounds.  While moving an object out of bounds rotate it
                if (x < 0 || x >= grid.Count) { // 10 this fixes rotation issues
                    changeVector.x = x >= grid.Count ? grid.Count - 1 - x : -x;
                    x += (int)changeVector.x;
                }
                //Debug.Log(x + " " + v.x + " " + position.x + " " + changeVector.x);

                List<GameObject> val = grid[x]; // check above should make sure x isn't out of bounds so don't need to check here
                if (y >= val.Count) // for overhang
                    continue;
                if (y < 0 || val[y].GetComponent<Grid>().isPlaced) { // y <= 0 because the bottom is 0 and the top is 19 so don't go below the bottom
                    changeVector.y = 0;
                    isFloor = true; 
                    floorChanged = true;
                }
                if (changeVector.magnitude == 0)
                    break;
            }
        if (decayDown > 0)  //                                                                                 veritcal controls
            changeVector.y = 0; // put decay down here and floorChanged bool to fix (floor doesn't work when it touches the bottom then moves away ie: __- if it touches the right then you move over to the left it will float)
        else
            decayDown = decayDownStart;  // restart the decay so it doesn't move down every tick
        if (!floorChanged)
            isFloor = false;
        if (isFloor) // start floor decay
            decayFloorDown = Mathf.Max(decayFloorDown - Time.deltaTime, -1);
        else
            decayFloorDown = decayFloorDownStart;
        if (decayFloorDown <= 0) { // this means the piece is now stuck in place
            // TODO if below is out of range then that means game over
            foreach (Vector2 v in shape.shape) // turn this piece into something another piece can't move through
                if ((int)(v.y + position.y) >= grid[(int)(v.x + position.x)].Count)
                    endGame();
                else
                    grid[(int)(v.x + position.x)][(int)(v.y + position.y)].GetComponent<Grid>().isPlaced = true;
            shape = shapeNext;
            decayFloorDown = decayFloorDownStart;
            shapeNext = null;
            isFloor = false;
            position = origin;
            List<int> breaks = checkForBreaks();
            if (breaks.Count > 0)
                breakRows(breaks);
            addPoints(4); // add 4 points because you laid a shape with 4 pieces down
            oldGhost.Clear();
            oldShape.Clear();
            canHold = true;
        }
        if (changeVector.magnitude > 0) {
            //Debug.Log((position + changeVector) + " " + position + " " + changeVector);
            position += changeVector;
            moveShape(shape);
        }
        changeVector = Vector2.zero;
    }

    private void displayGhost() {
        // move y down until it hits floor or an isActive Grid then display a ghost sprite on each piece there
        int i; // i is the position to place the ghost
        bool breakout = false;
        for (i = (int)position.y; i > -4; i--) {
            foreach (Vector2 v in shape.shape) { 
                if ((int)(v.x + position.x) < 0 || (int)(v.x + position.x) >= grid.Count || (int)(v.y + i) >= grid[(int)(v.x + position.x)].Count)
                    break;
                if ((int)(v.y + i) <= 0 || // ghost hits ground or ghost above grid // || (int)(v.y + i) >= grid[(int)(v.x + position.x)].Count 
                    grid[(int)(v.x + position.x)][(int)(v.y + i)].GetComponent<Grid>().isPlaced) { // ghost hits placed grid
                    if (grid[(int)(v.x + position.x)][(int)(v.y + i)].GetComponent<Grid>().isPlaced)
                        i += 1;
                    breakout = true;
                    //break; // break removed because if one part hits the ground and another hits a piece it should count the piece not the ground
                }
            }
            if (breakout)
                break;
        }
        //Debug.Log(i);
        foreach (Vector2 v in oldGhost) // old method didn't work because rotations change where shape was
            if (!grid[(int)v.x][(int)v.y].GetComponent<Grid>().isShape) 
                changeToGrid((int)v.x, (int)v.y);
        oldGhost.Clear();
        foreach (Vector2 v in shape.shape) {
            if ((int)(v.x + position.x) < 0 || (int)(v.x + position.x) >= grid.Count ||
                    (int)(v.y + i) < 0 || (int)(v.y + i) >= grid[(int)(v.x + position.x)].Count - 1)
                continue;
            if (!grid[(int)(v.x + position.x)][(int)(i + v.y)].GetComponent<Grid>().isShape) {
                changeTo((int)(v.x + position.x), (int)(i + v.y), ghostGridSprite, gridScale, false);
                oldGhost.Add(new Vector2(v.x + position.x, i + v.y));
            }
        }
    }

    private void endGame() {
        gameOver = true;
        gameOverUI.SetActive(true);
    }

    private void addPoints(int v) {
        points += v;
        this.pointsText.text = points + "";
        if (points >= 100 * level) {
            level++;
            levelText.text = level + "";
            // up difficulty by decreasing decayDownStart and decayFloorDownStart
            decayDownStart *= .9f;
            decayFloorDownStart *= .9f;
        }

        if (points > PlayerPrefs.GetInt(highscorekey, 0)) {
            PlayerPrefs.SetInt(highscorekey, (int)points);
            highScore.text = points + "";
        }
    }

    private void breakRows(List<int> breaks) {
        // list should be in descending order
        // then move the stuff above the breaks down number of breaks y
        // can't take the biggest break because they might break a 10 and a 6 for instance, in which case
        // move everything above the 10 down the 10, then move everything above the 6 down to the 6
        breaks.Sort();
        breaks.Reverse();
        foreach (int b in breaks)
            for (int i = 0; i < grid.Count; i++) // 10
                for (int j = b; j < grid[i].Count; j++) // 20
                    if (b + 1 >= grid[i].Count || !grid[i][j + 1].GetComponent<Grid>().isPlaced) {
                        changeToGrid(i, j);
                        break;
                    } else
                        changeToBlock(i, j, grid[i][j + 1].GetComponent<SpriteRenderer>().sprite);

        // lastly add points //maybe add points to seperate method, but whatever
        addPoints(breaks.Count * 15 + (int)level + (breaks.Count == 4 ? 20+(int)level : 0)); // add addition 10 points for getting a tetris

    }

    private List<int> checkForBreaks() {
        List<int> breakRows = new List<int>();
        int[] cols = new int[grid[0].Count];
        for (int i = 0; i < grid.Count; i++) // max 10
            for (int j = 0; j < grid[i].Count; j++) // max 20
                if (grid[i][j].GetComponent<Grid>().isPlaced)
                    cols[j]++;
        for (int i = 0; i < cols.Length; i++)
            if (cols[i] == 10)
                breakRows.Add(i);
        return breakRows;
    }



    private GameObject changeToGrid(int x, int y) {
        GameObject g = changeTo(x, y, gridSprite, gridScale,false);
        if (g != null)
            g.GetComponent<Grid>().isShape = false;
        return g;
    }
    private GameObject changeToBlock(int x, int y, Sprite sprite) {
        return changeTo(x, y, sprite, pieceScale, true);
    }
    private GameObject changeToBlock(int x, int y, Sprite sprite, bool isPlaced) {
        GameObject g = changeTo(x, y, sprite, pieceScale, isPlaced);
        if(g!=null)
            g.GetComponent<Grid>().isShape = true;
        return g;
    }

    private GameObject changeTo(int x, int y,Sprite sprite,Vector3 localscale,bool isPlaced) {
        if (x >= grid.Count || y >= grid[x].Count)
            return null;
        GameObject g = grid[x][y];
        SpriteRenderer spriteRenderer = g.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.transform.localScale = localscale;
        g.GetComponent<Grid>().isPlaced = isPlaced;
        return g;
    }

    private void RenderShape(Shape shape) {
        foreach (Vector2 v in shape.shape)
            changeToBlock((int)(v.x + origin.x), (int)(v.y + origin.y), shape.sprite, false); // false because it hasn't been placed yet
    }

    private void rotateShape() {
        // put old position back to grid
        foreach (Vector2 v in oldShape)
            changeToGrid((int)v.x, (int)v.y);
        //foreach (Vector2 v in shape.shape)
        //    changeToGrid((int)(v.x + oldPosition.x), (int)(v.y + oldPosition.y));
        shape.rotate();
        oldShape.Clear();
        // put new position as shape
        foreach (Vector2 v in shape.shape) {
            if ((int)(v.x + position.x + changeVector.x) < 0)
                changeVector.x = -(v.x + position.x);
            //Debug.Log((int)(v.x + position.x + changeVector.x) + " " + v.x + " " + position.x + " " + changeVector.x);
            oldShape.Add(v + position+ changeVector);
            changeToBlock((int)(v.x + position.x + changeVector.x), (int)(v.y + position.y), shape.sprite, false); // false because it hasn't been placed yet
        }
    }

    private void moveShape(Shape shape) {
        // move it down one y in the grid change piece back to grid
        // change old grid position back
        foreach(Vector2 v in oldShape)
            changeToGrid((int)v.x, (int)v.y);
        //foreach (Vector2 v in shape.shape)
        //    changeToGrid((int)(v.x + oldPosition.x), (int)(v.y + oldPosition.y));
        // change shape position
        oldShape.Clear();
        foreach (Vector2 v in shape.shape) {
            oldShape.Add(v + position);
            changeToBlock((int)(v.x + position.x), (int)(v.y + position.y), shape.sprite, false);
        }
    }
}
