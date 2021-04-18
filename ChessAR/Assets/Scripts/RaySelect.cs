using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RaySelect : MonoBehaviour
{
    public Camera cm;
    public Color pieceSelectColor;
    public Color squareSelectColor;
    public GameObject selectedPiece;
    public GameObject selectedSquare;
    public TextMeshProUGUI turnText;

    private Color oldColorPiece;
    private GameObject prevGmPiece;
    private Color oldColorSquare;
    public static float initialHeight = 0.6f;
    private float halfHeight = 1.0f;
    private float raiseHeight = 1.8f;

    private bool whiteTurn;
    private bool ready;
    private bool updateMove;

    private float touchTimer;
    private float touchDist;
    private bool touchSelect;
    private Vector3 touchSpeed;

    // 2D array of possible moves of a piece
    private bool[,] possible_moves = new bool[8, 8]
    {
        {false, false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false, false},
        {false, false, false, false, false, false, false, false}
    };

    // list of tag of pieces
    private List<string> pieces_list = new List<string>()
    {
        "Rook",
        "Bishop",
        "Knight",
        "Queen",
        "King",
        "Pawn"
    };

    void Start()
    {
        whiteTurn = true;
        ready = true;
        updateMove = false;
        touchTimer = -1;
        touchDist = -1;
        touchSelect = false;
        touchSpeed = Vector3.zero;
    }

    void Update()
    {
        //#if UNITY_IPHONE || UNITY_ANDROID
        if (Input.touchCount == 1 && ready)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began) touchTimer = Time.time;
            else if (Input.GetTouch(0).phase == TouchPhase.Ended)
            {
                if (Time.time - touchTimer < 0.3f) FindObj();
                else if (touchSelect)
                {
                    ready = false;
                    touchSelect = false;
                    selectedPiece.layer = 0;

                    int[] row_col = SquareToRowAndCol(selectedSquare.name);

                    if (possible_moves[row_col[0], row_col[1]])
                    {
                        whiteTurn = !whiteTurn;
                        StartCoroutine(TranslatePiece(1));
                    }
                    else
                    {
                        StartCoroutine(raisePiece(null, selectedPiece));
                        selectedPiece = null;
                    }
                }
                touchTimer = -1;
                touchDist = -1;
            }
            else if (Time.time - touchTimer > 0.3f && selectedPiece)
            {
                if (touchDist == -1) touchDist = Vector3.Distance(cm.transform.position, selectedPiece.transform.position);
                else if (!touchSelect && Vector3.Distance(cm.transform.position, selectedPiece.transform.position) / touchDist < 0.8f)
                {
                    touchSelect = true;
                    if (selectedSquare) SetColor(selectedSquare, oldColorSquare);
                    selectedSquare = selectedPiece.GetComponent<CurrentSquare>().currentSquare;
                    oldColorSquare = GetColor(selectedSquare);
                }

                if (touchSelect)
                {
                    selectedPiece.layer = 2;
                    LayerMask mask = (1 << 0) | (0 << 1);

                    Vector3 desPos = selectedPiece.transform.localPosition;
                    desPos.y = raiseHeight;
                    //raise the piece first
                    if (selectedPiece.transform.localPosition.y < raiseHeight - 0.01f)
                        selectedPiece.transform.localPosition = Vector3.SmoothDamp(selectedPiece.transform.localPosition, desPos, ref touchSpeed, 0.1f);
                    else if (selectedPiece.transform.localPosition.y < raiseHeight)
                        selectedPiece.transform.localPosition = desPos;
                    //find possible slots and move the piece
                    else
                    {
                        RaycastHit hit;
                        Ray raycast = cm.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

                        if (Physics.Raycast(raycast, out hit, mask))
                        {
                            GameObject obj = hit.transform.gameObject;
                            if (pieces_list.Contains(obj.tag)) obj = obj.GetComponent<CurrentSquare>().currentSquare;

                            if (selectedSquare != obj)
                            {
                                SetColor(selectedSquare, oldColorSquare);
                                selectedSquare = obj;
                                oldColorSquare = GetColor(obj);
                            }
                        }

                        desPos = selectedSquare.transform.localPosition;
                        desPos.y = raiseHeight;
                        selectedPiece.transform.localPosition = Vector3.SmoothDamp(selectedPiece.transform.localPosition, desPos, ref touchSpeed, 0.1f);
                    }
                }
            }
        }
        /*
        #else
        if (Input.GetMouseButtonDown(0))
        {
            FindObj();
        }
        #endif
        */
        SelectedHandler();
    }

    void FindObj()
    {
        Debug.Log(Time.time - touchTimer);
        // Cast ray from the screen
        RaycastHit hit;
        Ray raycast = cm.ScreenPointToRay(Input.mousePosition);

        // Detect raycast hit an object
        if (Physics.Raycast(raycast, out hit))
        {
            // store informations about current gameObject
            GameObject obj = hit.transform.gameObject;
            bool same = false;

            // 0 for square, 1 for piece
            int piece = -1;

            if (pieces_list.Contains(obj.tag))
            {
                if (obj.name.Contains("white") && whiteTurn) piece = 1;
                else if (obj.name.Contains("black") && !whiteTurn) piece = 1;
                else
                {
                    obj = obj.GetComponent<CurrentSquare>().currentSquare;
                    piece = 0;
                }
            }
            else if (obj.tag == "Square") piece = 0;

            // a piece
            if (piece == 1)
            {
                //reset chessboard and show possible moves
                selectedSquare = null;
                resetChessBoardColor();
                updateMove = true;

                // if a previous selection exists, reset to old color
                if (prevGmPiece)
                {
                    //SetColor(prevGmPiece, oldColorPiece);
                    if (prevGmPiece == obj) same = true;
                }

                // only indicate new selection for different pieces
                if (!same)
                {
                    ready = false;
                    StartCoroutine(raisePiece(obj, prevGmPiece));

                    // overwrite previous selection gameObject and color with current
                    prevGmPiece = obj;
                    oldColorPiece = GetColor(obj);

                    //SetColor(obj, pieceSelectColor);
                    selectedPiece = obj;
                }
                else
                {
                    prevGmPiece = null;
                    selectedPiece = null;

                    ready = false;
                    StartCoroutine(raisePiece(null, obj));
                }
            }
            else if (piece == 0 && selectedPiece)
            {
                // if a previous selection exists, reset to old color
                if (selectedSquare)
                {
                    SetColor(selectedSquare, oldColorSquare);
                    if (selectedSquare == obj) same = true;
                }

                // overwrite previous selection gameObject and color with current
                selectedSquare = obj;
                oldColorSquare = GetColor(obj);

                // if the same selection, move the piece
                if (same) MovePiece();
            }
        }
        else
        {
            ready = false;
            StartCoroutine(raisePiece(null, selectedPiece));
            selectedPiece = null;
        }
    }

    void SelectedHandler()
    {
        // need to select piece before select square 
        if (selectedPiece)
        {
            if (ready && updateMove)
            {
                findPossibleMoves(selectedPiece);
                updateMove = false;
            }

            if (selectedSquare)
            {
                // get the row and col of the selected square
                int[] row_col = SquareToRowAndCol(selectedSquare.name);

                // if the selected square is not in possible_moves movies
                if (possible_moves[row_col[0], row_col[1]] == false)
                    SetColor(selectedSquare, Color.red);
                else
                    SetColor(selectedSquare, Color.blue);
            }
        }
        else
        {
            // reset chess borad and the selected piece
            resetAll();
        }
    }

    public void MovePiece()
    {
        // get the row and col of the selected square
        int[] row_col = SquareToRowAndCol(selectedSquare.name);

        // check if this is a valid moves and move it only it is valid
        if (possible_moves[row_col[0], row_col[1]] == true)
        {
            whiteTurn = !whiteTurn;
            ready = false;
            StartCoroutine(TranslatePiece(0));
            if (whiteTurn) {
                turnText.text = "WHITE turn";
            } else {
                turnText.text = "BLACK turn";
            }
        }
        else
        {
            ready = false;
            StartCoroutine(raisePiece(null, selectedPiece));
            resetAll();
        }
    }

    private IEnumerator raisePiece(GameObject selected, GameObject prev)
    {
        Vector3 currentPos1 = Vector3.zero;
        Vector3 currentPos2 = Vector3.zero;
        float raiseL1 = 0;
        float raiseL2 = 0;
        float speed1 = 0;
        float speed2 = 0;
        Vector3 speed = Vector3.zero;

        if (prev)
        {
            currentPos1 = prev.transform.localPosition;
            raiseL1 = currentPos1.y;

            Vector3 originSquarePos = prev.GetComponent<CurrentSquare>().currentSquare.transform.localPosition;
            originSquarePos.y = currentPos1.y;

            while (Vector3.Distance(currentPos1, originSquarePos) > 0.01f)
            {
                currentPos1 = Vector3.SmoothDamp(currentPos1, originSquarePos, ref speed, 0.1f);
                prev.transform.localPosition = currentPos1;
                yield return null;
            }

            currentPos1 = originSquarePos;
            prev.transform.localPosition = currentPos1;
        }
        if (selected)
        {
            currentPos2 = selected.transform.localPosition;
            raiseL2 = currentPos2.y;
        }

        //animation
        while (raiseL1 - initialHeight > 0.01f || halfHeight - raiseL2 > 0.01f)
        {
            raiseL1 = Mathf.SmoothDamp(raiseL1, initialHeight, ref speed1, 0.1f);
            raiseL2 = Mathf.SmoothDamp(raiseL2, halfHeight, ref speed2, 0.1f);

            if (prev)
            {
                currentPos1.y = raiseL1;
                prev.transform.localPosition = currentPos1;
            }
            if (selected)
            {
                currentPos2.y = raiseL2;
                selected.transform.localPosition = currentPos2;
            }

            yield return null;
        }

        //finally setting the height to initial
        if (prev)
        {
            currentPos1.y = initialHeight;
            prev.transform.localPosition = currentPos1;
        }
        if (selected)
        {
            currentPos2.y = halfHeight;
            selected.transform.localPosition = currentPos2;
        }

        ready = true;
    }

    private IEnumerator TranslatePiece(int inputStage)
    {
        Vector3 speed = Vector3.zero;
        Vector3 desPos = Vector3.zero;

        //0 for raise to max height, 1 for horizontal move, 2 for drop
        int transStage = inputStage;

        if (transStage == 0) desPos = selectedPiece.transform.localPosition;
        else desPos = selectedSquare.transform.localPosition;
        desPos.y = raiseHeight;

        //stop the animation if special move is needed (capture, switch or promotion)
        bool stop = false;
        bool specialMove = false;

        while (Vector3.Distance(desPos, selectedPiece.transform.localPosition) > 0.01f || transStage != 2)
        {
            //if approaching the destination, change to the next destination
            if (Vector3.Distance(desPos, selectedPiece.transform.localPosition) < 0.1f)
            {
                if (transStage == 0)
                {
                    desPos = selectedSquare.transform.localPosition;
                    desPos.y = raiseHeight;
                    transStage = 1;
                }
                else if (transStage == 1)
                {
                    desPos.y = initialHeight;
                    stop = specialMove;
                    transStage = 2;
                }
            }

            if (stop)
            {
                //the capture, switch and promotion handler
            }
            else selectedPiece.transform.localPosition = Vector3.SmoothDamp(selectedPiece.transform.localPosition, desPos, ref speed, 0.1f);

            yield return null;
        }

        selectedPiece.transform.localPosition = desPos;
        //SetColor(prevGmPiece, oldColorPiece);
        SetColor(selectedSquare, oldColorSquare);
        selectedPiece = null;
        selectedSquare = null;
        prevGmPiece = null;

        // reset chess board color after each move
        resetChessBoardColor();
        clearPossibleMoves();
        ready = true;
    }

    // -------------------------------------------------------------------------------
    // find possible_moves
    void findPossibleMoves(GameObject current_piece)
    {
        // get current square
        GameObject current_square = current_piece.GetComponent<CurrentSquare>().currentSquare;
        bool is_white_piece = current_piece.name.Contains("white");

        int[] row_col = SquareToRowAndCol(current_square.name);
        int new_row = row_col[0];
        int new_col = row_col[1];

        // reset possible_moves array
        clearPossibleMoves();

        // modify possible_moves array
        switch (current_piece.tag)
        {
            // Rook moves
            case "Rook":
                SetCrossMoves(row_col[0], row_col[1], current_piece);
                break;
            // Bishop moves
            case "Bishop":
                setDiagonalMoves(row_col[0], row_col[1], current_piece);
                break;
            // Knight moves
            case "Knight":
                // possible directions of knight
                int[,] knight_directions = new int[8, 2] { { 2, -1 }, { 2, 1 }, { -2, -1 }, { -2, 1 }, { 1, -2 }, { -1, -2 }, { 1, 2 }, { -1, 2 } };
                // for each direction
                for (int i = 0; i < possible_moves.GetLength(0); i++)
                {
                    new_row = row_col[0] + knight_directions[i, 0];
                    new_col = row_col[1] + knight_directions[i, 1];
                    if (isValidMove(new_row, new_col, current_piece)) possible_moves[new_row, new_col] = true;
                }
                break;
            case "Queen":
                SetCrossMoves(row_col[0], row_col[1], current_piece);
                setDiagonalMoves(row_col[0], row_col[1], current_piece);
                break;
            // King moves
            case "King":
                // possible directions of king
                int[,] king_directions = new int[8, 2] { { 1, 0 }, { -1, 0 }, { 0, -1 }, { 0, 1 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };
                // for each direction
                for (int i = 0; i < possible_moves.GetLength(0); i++)
                {
                    new_row = row_col[0] + king_directions[i, 0];
                    new_col = row_col[1] + king_directions[i, 1];
                    if (isValidMove(new_row, new_col, current_piece)) possible_moves[new_row, new_col] = true;
                }
                break;
            // Pawn moves
            case "Pawn":
                new_row = row_col[0];
                new_col = row_col[1];

                // the row before the pawn
                if (is_white_piece) new_row += 1;
                else new_row -= 1;

                GameObject temp = GameObject.Find(RowAndColToSquare(new_row, new_col));

                if (isValidMove(new_row, new_col, current_piece)) possible_moves[new_row, new_col] = true;

                // the pawn is at second row
                if (is_white_piece && row_col[0] == 1) new_row += 1;
                else if (!is_white_piece && row_col[0] == 6) new_row -= 1;

                if (isValidMove(new_row, new_col, current_piece) && temp.GetComponent<CurrentPiece>().currentPiece == null)
                    possible_moves[new_row, new_col] = true;

                break;
        }

        showPossibleMoves();
    }

    // reflect possible_moves arrry to the chess board
    void showPossibleMoves()
    {
        for (int i = 0; i < possible_moves.GetLength(0); i++)
        {
            for (int j = 0; j < possible_moves.GetLength(1); j++)
            {
                if (possible_moves[i, j] == true)
                {
                    // get possiblt squares
                    GameObject possible_square = GameObject.Find(RowAndColToSquare(i, j));

                    // set color of the material to squareSelectColor
                    SetColor(possible_square, squareSelectColor);
                }
            }
        }
    }

    // set the corss moves for Rook or Queen
    void SetCrossMoves(int current_row, int current_col, GameObject current_piece)
    {
        // possible directions of corss move 
        int[,] rook_directions = new int[2, 7]
        {
            {1, 2, 3, 4, 5, 6, 7},
            {-1, -2, -3, -4, -5, -6, -7}
        };

        // for 4 directions (up, down, left, right)
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                int new_row = current_row;
                int new_col = current_col;

                //the four directions
                if (i == 0) new_row += rook_directions[0, j];
                else if (i == 1) new_row += rook_directions[1, j];
                else if (i == 2) new_col += rook_directions[0, j];
                else if (i == 3) new_col += rook_directions[1, j];

                // there is a piece blocking the way. break
                if (!isValidMove(new_row, new_col, current_piece)) break;

                //set possible_moves to be true
                possible_moves[new_row, new_col] = true;
            }
        }
    }

    // set the diagonal moves for Bishop or Queen
    void setDiagonalMoves(int current_row, int current_col, GameObject current_piece)
    {
        // possible directions of diagonal move 
        int[,] rook_directions = new int[2, 7]
        {
            {1, 2, 3, 4, 5, 6, 7},
            {-1, -2, -3, -4, -5, -6, -7}
        };

        // for 4 directions (top left, top right, bottom left, bottom right)
        for (int i = 0; i < 4; i++)
        {
            //the four directions controlled by (m, n)
            int m = 0;
            if (i > 1) m = 1;

            int n = 0;
            if (i == 1 || i == 3) n = 1;

            for (int j = 0; j < 7; j++)
            {
                // row changes, col does not change
                int new_row = current_row + rook_directions[m, j];
                int new_col = current_col + rook_directions[n, j];

                // there is a piece blocking the way. break
                if (!isValidMove(new_row, new_col, current_piece)) break;

                //set possible_moves to be true
                possible_moves[new_row, new_col] = true;
            }
        }
    }

    // given the new row, new col and current piece 
    // return true if there is no pieces with the same color on it or it is not out of bound
    bool isValidMove(int row, int col, GameObject current_piece)
    {
        GameObject current_square = current_piece.GetComponent<CurrentSquare>().currentSquare;
        if (RowAndColToSquare(row, col) == current_square.name) return false;

        // new row and col are not out of bound
        if (row >= 0 && row <= 7 && col >= 0 && col <= 7)
        {
            // get the destination square
            GameObject des_square = GameObject.Find(RowAndColToSquare(row, col));
            // get piece on the destination square
            GameObject des_piece = des_square.GetComponent<CurrentPiece>().currentPiece;

            // if des_piece is null, return true
            if (des_piece == null) return true;
            else
            {
                //get the color by name
                bool current_white = current_piece.name.Contains("white");
                bool des_white = des_piece.name.Contains("white");

                // return true if the color is not the same, false otherwise
                return ((current_white && !des_white) || (!current_white && des_white));
            }
        }
        else return false;
    }

    // reset chess board color
    void resetChessBoardColor()
    {
        for (int i = 0; i < possible_moves.GetLength(0); i++)
        {
            for (int j = 0; j < possible_moves.GetLength(1); j++)
            {
                // get possiblt squares
                GameObject possible_square = GameObject.Find(RowAndColToSquare(i, j));

                // black cells if row + col % 2 = 0
                if ((i + j) % 2 == 0) SetColor(possible_square, Color.black);
                // white cells
                else SetColor(possible_square, Color.white);
            }
        }
    }

    // reset everything
    void resetAll()
    {
        //reset the color
        //if (prevGmPiece) SetColor(prevGmPiece, oldColorPiece);

        //reset gameobjects
        selectedPiece = null;
        selectedSquare = null;
        prevGmPiece = null;

        resetChessBoardColor();
        clearPossibleMoves();
    }

    // given current square, return row and col
    int[] SquareToRowAndCol(string square)
    {
        int row = (int)(square[1] - '0' - 1);
        int col = (int)(square[0] - 'A');
        return new int[] { row, col };
    }

    // given row and col, return square
    string RowAndColToSquare(int row, int col)
    {
        string res = "";
        res += (char)('A' + col);
        return res + (row + 1);
    }

    // set all elements in possible_moves to false
    void clearPossibleMoves()
    {
        for (int i = 0; i < possible_moves.GetLength(0); i++)
        {
            for (int j = 0; j < possible_moves.GetLength(1); j++)
            {
                possible_moves[i, j] = false;
            }
        }
    }

    void SetColor(GameObject obj, Color input)
    {
        obj.GetComponent<Renderer>().material.color = input;
    }

    Color GetColor(GameObject obj)
    {
        return obj.GetComponent<Renderer>().material.color;
    }
}
