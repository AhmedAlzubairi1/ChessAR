using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaySelect : MonoBehaviour
{
    public Camera cm;
    public Color pieceSelectColor;
    public Color squareSelectColor;
    public GameObject selectedPiece;
    public GameObject selectedSquare;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI statusText;

    public Sprite emptySpr;
    public Sprite black_pawn;
    public Sprite black_rook;
    public Sprite black_bishop;
    public Sprite black_knight;
    public Sprite black_queen;
    public Sprite black_king;
    public Sprite white_pawn;
    public Sprite white_rook;
    public Sprite white_bishop;
    public Sprite white_knight;
    public Sprite white_queen;
    public Sprite white_king;

    public Transform blackCapture;
    public Transform whiteCapture;
    private int blackIndex;
    private int whiteIndex;

    public GameObject promotionPanel;
    Vector3 promotionScale;

    private Color oldColorPiece;
    private GameObject prevGmPiece;
    private Color oldColorSquare;
    public static float initialHeight = 0.6f;
    private float halfHeight = initialHeight;
    private float raiseHeight = 2f;

    private bool whiteTurn;
    private bool ready;
    private bool updateMove;

    private float touchTimer;
    //private float touchDist;
    private bool touchSelect;
    private Vector3 touchSpeed;
    private Color colorWhite;
    private Color colorBlack;
    private ColorUpdater cu;

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

    private Dictionary<string, string> statusStrings = new Dictionary<string, string>(){
        { "start", "tap a piece" },
        { "tap and hold", "tap and hold the piece" },
        { "move", "move phone to select a square" },
        { "confirm move", "you have moved the piece" },
        { "confirm capture", "you have captured a piece" }
    };

    // castling
    private bool [,] not_moved_castling = new bool[2, 3]
    {
        // {white king, white queen side rook, white king side rook}
        {true, true, true},
        // {black king, black queen side rook, black king side rook}
        {true, true, true}
    };

    private bool can_castling = false;
    private List<string> castling_square_list = new List<string>()
    {
        "C1",
        "D1",
        "F1",
        "G1",
        "C8",
        "D8",
        "F8",
        "G8"
    };

    void Start()
    {
        whiteTurn = true;
        ready = true;
        updateMove = false;
        touchTimer = -1;
        //touchDist = -1;
        touchSelect = false;
        touchSpeed = Vector3.zero;
        statusText.text = statusStrings["start"];
        promotionScale = promotionPanel.transform.localScale;
        promotionPanel.SetActive(false);

        blackIndex = 0;
        whiteIndex = 0;

        for (int i = 0; i < 16; i++)
        {
            whiteCapture.GetChild(i).gameObject.GetComponent<Image>().sprite = emptySpr;
            blackCapture.GetChild(i).gameObject.GetComponent<Image>().sprite = emptySpr;
        }
        cu = GameObject.Find("ChessBoard").GetComponent<ColorUpdater>();
        SetTheme();
    }
    void Update()
    {
        if (whiteTurn) {
            turnText.text = "WHITE turn";
        } else {
            turnText.text = "BLACK turn";
        }
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
                        statusText.text = statusStrings["start"];
                    }
                }
                touchTimer = -1;
                //touchDist = -1;
            }
            else if (Time.time - touchTimer > 0.5f && selectedPiece)
            {
                // if (touchDist == -1) touchDist = Vector3.Distance(cm.transform.position, selectedPiece.transform.position);
                // else if (!touchSelect && Vector3.Distance(cm.transform.position, selectedPiece.transform.position) / touchDist < 0.8f)
                if (!touchSelect)
                {
                    touchSelect = true;
                    statusText.text = statusStrings["move"];
                    if (selectedSquare) SetColor(selectedSquare, oldColorSquare);
                    selectedSquare = selectedPiece.GetComponent<CurrentSquare>().currentSquare;
                    oldColorSquare = GetColor(selectedSquare);
                }

                if (touchSelect)
                {
                    selectedPiece.layer = 2;
                    LayerMask mask = (1 << 0) | (0 << 2);

                    Vector3 desPos = selectedPiece.transform.localPosition;
                    desPos.y = raiseHeight;
                    //raise the piece first
                    if (selectedPiece.transform.localPosition.y < raiseHeight - 0.01f)
                        selectedPiece.transform.localPosition = Vector3.SmoothDamp(selectedPiece.transform.localPosition, desPos, ref touchSpeed, 0.05f);
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
                        // movement speed following phone
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
        // Cast ray from the screen
        RaycastHit hit;
        Ray raycast = cm.ScreenPointToRay(Input.mousePosition);

        // Detect raycast hit an object
        if (Physics.Raycast(raycast, out hit))
        {
            // store informations about current gameObject
            GameObject obj = hit.transform.gameObject;

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

                // only indicate new selection for different pieces
                if (prevGmPiece != obj)
                {
                    ready = false;
                    StartCoroutine(raisePiece(obj, prevGmPiece));

                    // overwrite previous selection gameObject and color with current
                    prevGmPiece = obj;
                    oldColorPiece = GetColor(obj);

                    //SetColor(obj, pieceSelectColor);
                    selectedPiece = obj;
                    statusText.text = statusStrings["tap and hold"];
                }
                else
                {
                    prevGmPiece = null;
                    selectedPiece = null;

                    ready = false;
                    StartCoroutine(raisePiece(null, obj));
                    statusText.text = statusStrings["start"];
                }
            }
            else if (piece == 0 && selectedPiece)
            {
                bool same = false;

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
            statusText.text = statusStrings["start"];
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
            { // move piece to original space after deselecting
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
            raiseL1 = Mathf.SmoothDamp(raiseL1, initialHeight, ref speed1, 0.05f);
            raiseL2 = Mathf.SmoothDamp(raiseL2, halfHeight, ref speed2, 0.05f);

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
        statusText.text = statusStrings["confirm move"];

        // get the row and col of the selected square
        int[] row_col = SquareToRowAndCol(selectedSquare.name);
        int row = row_col[0];
        int col = row_col[1];

        // get the destination square
        GameObject des_square = GameObject.Find(RowAndColToSquare(row, col));
        // get the piece on the destination square
        GameObject des_piece = des_square.GetComponent<CurrentPiece>().currentPiece;

        //scales needed for animation
        Vector3 sel_piece_scale = selectedPiece.transform.localScale;
        Vector3 des_piece_scale = Vector3.zero;
        if (des_piece) des_piece_scale = des_piece.transform.localScale;

        float scaleFactor = 1.0f;
        float scaleSpeed = 0;

        Vector3 speed = Vector3.zero;
        Vector3 desPos = Vector3.zero;

        //0 for raise to max height, 1 for horizontal move, 2 for drop
        int transStage = inputStage;

        //set the destination before start
        if (transStage == 0) desPos = selectedPiece.transform.localPosition;
        else desPos = selectedSquare.transform.localPosition;
        desPos.y = raiseHeight;

        //stop the animation if special move is needed (capture, switch or promotion)
        bool stop = false;
        int specialMove = 0;

        //capture
        if (des_piece && des_piece != selectedPiece) specialMove += 1;
        //promotion
        if (selectedPiece.tag == "Pawn")
        {
            if (selectedPiece.name.Contains("white") && selectedSquare.name.Contains("8")) specialMove += 2;
            else if (selectedPiece.name.Contains("black") && selectedSquare.name.Contains("1")) specialMove += 2;
        }
        // castling
        if (selectedPiece.tag == "King" || selectedPiece.tag == "Rook")
        {
            if (castling_square_list.Contains(selectedSquare.name) && can_castling) 
            {
                Debug.Log("specialMove = 1");
                specialMove = 1;
            }
        }

        while (Vector3.Distance(desPos, selectedPiece.transform.localPosition) > 0.01f || transStage != 2)
        {
            //if approaching the destination, change to the next destination
            if (Vector3.Distance(desPos, selectedPiece.transform.localPosition) < 0.01f)
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
                    stop = (specialMove > 0);
                    transStage = 2;
                }
            }

            if (transStage == 2 && stop)
            {
                //the capture, switch and promotion handler
                scaleFactor = Mathf.SmoothDamp(scaleFactor, 0, ref scaleSpeed, 0.05f);

                // castling
                if (des_piece == null && specialMove == 1)
                {
                    // move current piece
                    selectedPiece.transform.localPosition = desPos;
                    // now move another piece
                    if (selectedPiece.name.Contains("white"))
                    {
                        if (selectedPiece.tag == "King")
                        {
                            // queen side castling
                            if (selectedSquare.name == "C1")
                            {
                                GameObject queen_side_rook = GameObject.Find("rook white");
                                GameObject new_square = GameObject.Find("D1");
                                queen_side_rook.transform.localPosition = new_square.transform.localPosition;
                            }
                            // king side castling
                            if (selectedSquare.name == "G1")
                            {
                                GameObject queen_side_rook = GameObject.Find("rook white (1)");
                                GameObject new_square = GameObject.Find("F1");
                                queen_side_rook.transform.localPosition = new_square.transform.localPosition;
                            }
                        }
                        else if (selectedPiece.tag == "Rook")
                        {
                            // queen side castling
                            if (selectedSquare.name == "D1")
                            {
                                GameObject queen_side_rook = GameObject.Find("king white");
                                GameObject new_square = GameObject.Find("C1");
                                queen_side_rook.transform.localPosition = new_square.transform.localPosition;
                            }
                            // king side castling
                            if (selectedSquare.name == "F1")
                            {
                                GameObject queen_side_rook = GameObject.Find("king white");
                                GameObject new_square = GameObject.Find("G1");
                                queen_side_rook.transform.localPosition = new_square.transform.localPosition;
                            }
                        }
                    }
                    // black piece
                    else
                    {
                        if (selectedPiece.tag == "King")
                        {
                            // queen side castling
                            if (selectedSquare.name == "C8")
                            {
                                GameObject queen_side_rook = GameObject.Find("rook black");
                                GameObject new_square = GameObject.Find("D8");
                                queen_side_rook.transform.localPosition = new_square.transform.localPosition;
                            }
                            // king side castling
                            if (selectedSquare.name == "G8")
                            {
                                GameObject queen_side_rook = GameObject.Find("rook black (1)");
                                GameObject new_square = GameObject.Find("F8");
                                queen_side_rook.transform.localPosition = new_square.transform.localPosition;
                            }
                        }
                        else if (selectedPiece.tag == "Rook")
                        {
                            // queen side castling
                            if (selectedSquare.name == "D8")
                            {
                                GameObject queen_side_rook = GameObject.Find("king black");
                                GameObject new_square = GameObject.Find("C8");
                                queen_side_rook.transform.localPosition = new_square.transform.localPosition;
                            }
                            // king side castling
                            if (selectedSquare.name == "F8")
                            {
                                GameObject queen_side_rook = GameObject.Find("king black");
                                GameObject new_square = GameObject.Find("G8");
                                queen_side_rook.transform.localPosition = new_square.transform.localPosition;
                            }
                        }
                    }
                }

                //capture-----
                if (des_piece != null)
                {
                    statusText.text = statusStrings["confirm capture"];
                    des_piece.transform.localScale = des_piece_scale * scaleFactor;

                    if (scaleFactor < 0.01f)
                    {
                        Sprite target = null;
                        if (des_piece.name.Contains("white"))
                        {
                            
                            switch (des_piece.tag)
                            {
                                case ("Rook"): target = white_rook; break;
                                case ("Pawn"): target = white_pawn; break;
                                case ("Knight"): target = white_knight; break;
                                case ("Bishop"): target = white_bishop; break;
                                case ("Queen"): target = white_queen; break;
                                case ("King"): target = white_king; break;
                            }
                            whiteCapture.GetChild(whiteIndex).gameObject.GetComponent<Image>().sprite = target;
                            whiteIndex += 1;
                        }
                        else if (des_piece.name.Contains("black"))
                        {
                            switch (des_piece.tag)
                            {
                                case ("Rook"): target = black_rook; break;
                                case ("Pawn"): target = black_pawn; break;
                                case ("Knight"): target = black_knight; break;
                                case ("Bishop"): target = black_bishop; break;
                                case ("Queen"): target = black_queen; break;
                                case ("King"): target = black_king; break;
                            }
                            blackCapture.GetChild(blackIndex).gameObject.GetComponent<Image>().sprite = target;
                            blackIndex += 1;
                        }

                        Destroy(des_piece);
                        des_piece = null;

                        if (specialMove == 1)
                        {
                            stop = false;
                            specialMove = 0;
                        }
                    }
                }

                //promotion
                if (specialMove > 1 && selectedPiece.tag == "Pawn")
                {
                    promotionPanel.SetActive(true);
                    promotionPanel.transform.localPosition = selectedPiece.transform.localPosition;
                    promotionPanel.transform.localScale = promotionScale * (1 - scaleFactor);
                    selectedPiece.transform.localScale = sel_piece_scale * scaleFactor;

                    Vector3 promotionR = promotionPanel.transform.localEulerAngles;
                    promotionR.y += 1;
                    if (promotionR.y > 180) promotionR.y -= 360;
                    promotionPanel.transform.localEulerAngles = promotionR;

                    if (Input.touchCount == 1)
                    {
                        RaycastHit hit;
                        Ray raycast = cm.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(raycast, out hit))
                        {
                            GameObject obj = hit.transform.gameObject;
                            if (obj.name.Contains("Promotion"))
                            {
                                Material curMat = selectedPiece.GetComponent<Renderer>().material;
                                selectedPiece.tag = obj.tag;
                                selectedPiece.GetComponent<MeshFilter>().mesh = obj.GetComponent<MeshFilter>().mesh;
                                selectedPiece.GetComponent<Renderer>().material = curMat;
                                scaleFactor = 1;
                            }
                        }
                    }
                }
                else if (specialMove > 1)
                {
                    promotionPanel.transform.localScale = promotionScale * scaleFactor;
                    selectedPiece.transform.localScale = sel_piece_scale * (1 - scaleFactor);

                    if (scaleFactor < 0.01f)
                    {
                        promotionPanel.SetActive(false);
                        selectedPiece.transform.localScale = sel_piece_scale;
                        specialMove = 0;
                        stop = false;
                    }
                }
            }
            else selectedPiece.transform.localPosition = Vector3.SmoothDamp(selectedPiece.transform.localPosition, desPos, ref speed, 0.05f);

            yield return null;
        }

        selectedPiece.transform.localPosition = desPos;
        SetColor(selectedSquare, oldColorSquare);
        selectedPiece = null;
        selectedSquare = null;
        prevGmPiece = null;

        // reset chess board color after each move
        resetChessBoardColor();
        clearPossibleMoves();
        ready = true;
        statusText.text = statusStrings["start"];
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
                // set not_moved_castling to false if the rook moved
                if (is_white_piece)
                {
                    if (current_piece.name == "rook white" && current_square.name != "A1")
                        not_moved_castling[0, 1] = false;
                    if (current_piece.name == "rook white (1)" && current_square.name != "H1")
                        not_moved_castling[0, 2] = false;
                }
                else
                {
                    if (current_piece.name == "rook black" && current_square.name != "A8")
                        not_moved_castling[1, 1] = false;
                    if (current_piece.name == "rook black (1)" && current_square.name != "H8")
                        not_moved_castling[1, 2] = false;
                }
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
                // set not_moved_castling to false if the king moved
                if (is_white_piece)
                {
                    if (current_piece.name == "king white" && current_square.name != "E1")
                        not_moved_castling[0, 0] = false;
                }
                else
                {
                    if (current_piece.name == "king black" && current_square.name != "E8")
                        not_moved_castling[1, 0] = false;
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

                // there is an enemy piece blocking the way. break
                if (isEnemyPiece(new_row, new_col,current_piece)) break;

                if (isValidMove(new_row, new_col, current_piece)) possible_moves[new_row, new_col] = true;

                // the pawn is at second row
                if (is_white_piece && row_col[0] == 1) new_row += 1;
                else if (!is_white_piece && row_col[0] == 6) new_row -= 1;

                // there is an enemy piece blocking the way. break
                if (isEnemyPiece(new_row, new_col,current_piece)) break;

                if (isValidMove(new_row, new_col, current_piece) && temp.GetComponent<CurrentPiece>().currentPiece == null)
                    possible_moves[new_row, new_col] = true;

                break;
        }

        showPossibleMoves();
        setPossibleCaptures(current_piece);
        if (current_piece.tag == "King" || current_piece.tag == "Rook")
            setCastling(current_piece);
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

    // find possible capture
    void setPossibleCaptures(GameObject current_piece)
    {
        // get current square
        GameObject current_square = current_piece.GetComponent<CurrentSquare>().currentSquare;
        bool is_white_piece = current_piece.name.Contains("white");
        // get current row and col
        int[] row_col = SquareToRowAndCol(current_square.name);
        int new_row = row_col[0];
        int new_col = row_col[1];
        // Pawn moves
        if (current_piece.tag == "Pawn")
        {
            // white pawn moves upward
            if (is_white_piece) 
                new_row += 1;
            // black pawu moves downward
            else
                new_row -= 1;
            
            // check for capture for the left side
            if (isValidMove(new_row, new_col - 1, current_piece) && isEnemyPiece(new_row, new_col - 1, current_piece))
            {
                // set it to be a valid move
                possible_moves[new_row, new_col - 1] = true;
                setCaptureColor(new_row, new_col - 1);
            }
            // check for capture for the right side
            if (isValidMove(new_row, new_col + 1, current_piece) && isEnemyPiece(new_row, new_col + 1, current_piece))
            {
                // set it to be a valid move
                possible_moves[new_row, new_col + 1] = true;
                setCaptureColor(new_row, new_col + 1);
            }
        }
        else
        {
            for (int i = 0; i < possible_moves.GetLength(0); i++)
            {
                for (int j = 0; j < possible_moves.GetLength(1); j++)
                {
                    if (possible_moves[i, j] == true && isValidMove(i, j, current_piece) && isEnemyPiece(i, j, current_piece))
                    {
                        // set capture color
                        setCaptureColor(i, j);
                    }
                }
            }
        }
    }

    // given the current piece, the destination row and color,
    // return true if there is an enemy piece on the square
    bool isEnemyPiece(int row, int col, GameObject current_piece)
    {
        // get the destination square
        GameObject des_square = GameObject.Find(RowAndColToSquare(row, col));
        // get piece on the destination square
        GameObject des_piece = des_square.GetComponent<CurrentPiece>().currentPiece;
        // if des_piece is null, return false
        if (des_piece == null) return false;
        else
        {
            //get the color by name
            bool current_white = current_piece.name.Contains("white");
            bool des_white = des_piece.name.Contains("white");

            // return true if the color is not the same, false otherwise
            return ((current_white && !des_white) || (!current_white && des_white));
        }
    }

    // set the color of the given square to cyan 
    void setCaptureColor(int row, int col)
    {
        // get the destination square
        GameObject des_square = GameObject.Find(RowAndColToSquare(row, col));
        // set color to be cyan
        SetColor(des_square, Color.cyan);
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

                // there is an enemy piece blocking the way. break
                if (isEnemyPiece(new_row, new_col,current_piece)) break;
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

                // there is an allied piece blocking the way. break
                if (!isValidMove(new_row, new_col, current_piece)) break;

                //set possible_moves to be true
                possible_moves[new_row, new_col] = true;

                // there is an enemy piece blocking the way. break
                if (isEnemyPiece(new_row, new_col,current_piece)) break;
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
    void SetTheme() {
        if (ColorManager.colorScheme == "bw") {
            colorBlack = cu.black.color;
            colorWhite = cu.white.color;
        } else if (ColorManager.colorScheme == "wood") {
            colorBlack = cu.bwSquare.color;
            colorWhite = cu.wwSquare.color;
        } else if (ColorManager.colorScheme == "stone") {
            colorBlack = cu.bsSquare.color;
            colorWhite = cu.wsSquare.color;
        }
    }

    // reset chess board color
    void resetChessBoardColor()
    {
        SetTheme();
        for (int i = 0; i < possible_moves.GetLength(0); i++)
        {
            for (int j = 0; j < possible_moves.GetLength(1); j++)
            {
                // get possiblt squares
                GameObject possible_square = GameObject.Find(RowAndColToSquare(i, j));

                // black cells if row + col % 2 = 0
                // if ((i + j) % 2 == 0) SetColor(possible_square, Color.black);
                if ((i + j) % 2 == 0) {
                    SetColor(possible_square, colorBlack);
                }
                // white cells
                else {
                    SetColor(possible_square, colorWhite);
                }//Color.white);
            }
        }
        foreach (Transform top in GameObject.Find("TopLayer").transform) {
            string name = top.gameObject.name;
            if (name == "White" || name.Split(' ')[0] == "White") {
                SetColor(top.gameObject, colorWhite);
            } else if (name == "Black" || name.Split(' ')[0] == "Black") {
                SetColor(top.gameObject, colorBlack);
            }
        }
        can_castling = false;
    }

    // reset everything
    void resetAll()
    {
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

    // set castling square
    void setCastling(GameObject cur_piece)
    {
        // king
        if (cur_piece.tag == "King")
        {
            // white
            if (cur_piece.name.Contains("white"))
            {
                // get objects of the queen side and king side rook
                GameObject queen_side_rook = GameObject.Find("rook white");
                GameObject king_side_rook = GameObject.Find("rook white (1)");
                // king does not move
                if (not_moved_castling[0, 0])
                {
                    // queen side rook does not move 
                    if (not_moved_castling[0, 1] && queen_side_rook != null)
                    {
                        // if there is a queen side castling
                        if (checkNoPieceBetweenCastling(cur_piece, queen_side_rook))
                        {
                            // set the color of the square
                            GameObject new_king_square = GameObject.Find("C1");
                            SetColor(new_king_square, Color.yellow);
                            // set the possible_moves to be true
                            int[] new_square = SquareToRowAndCol("C1");
                            possible_moves[new_square[0], new_square[1]] = true;
                            can_castling = true;
                        }
                    }
                    // king side rook does not move
                    if (not_moved_castling[0, 2] && king_side_rook != null)
                    {
                        // if there is a king side castling
                        if (checkNoPieceBetweenCastling(cur_piece, king_side_rook))
                        {
                            // set the color of the square
                            GameObject new_king_square = GameObject.Find("G1");
                            SetColor(new_king_square, Color.yellow);
                            // set the possible_moves to be true
                            int[] new_square = SquareToRowAndCol("G1");
                            possible_moves[new_square[0], new_square[1]] = true;
                            can_castling = true;
                        }
                    }
                }
            }
            // black
            else
            {
                GameObject queen_side_rook = GameObject.Find("rook black");
                GameObject king_side_rook = GameObject.Find("rook black (1)");
                // king does not move
                if (not_moved_castling[1, 0])
                {
                    // queen side rook does not move 
                    if (not_moved_castling[1, 1] && queen_side_rook != null)
                    {
                        // if there is a queen side castling
                        if (checkNoPieceBetweenCastling(cur_piece, queen_side_rook))
                        {
                            // set the color of the square
                            GameObject new_king_square = GameObject.Find("C8");
                            SetColor(new_king_square, Color.yellow);
                            // set the possible_moves to be true
                            int[] new_square = SquareToRowAndCol("C8");
                            possible_moves[new_square[0], new_square[1]] = true;
                            can_castling = true;
                        }
                    }
                    // king side rook does not move
                    if (not_moved_castling[1, 2] && king_side_rook != null)
                    {
                        // if there is a king side castling
                        if (checkNoPieceBetweenCastling(cur_piece, king_side_rook))
                        {
                            // set the color of the square
                            GameObject new_king_square = GameObject.Find("G8");
                            SetColor(new_king_square, Color.yellow);
                            // set the possible_moves to be true
                            int[] new_square = SquareToRowAndCol("G8");
                            possible_moves[new_square[0], new_square[1]] = true;
                            can_castling = true;
                        }
                    }
                }
            }
        }
        // rook
        else if (cur_piece.tag == "Rook")
        {
            // white rook
            if (cur_piece.name.Contains("white"))
            {
                Debug.Log("white rook");
                GameObject king = GameObject.Find("king white");
                GameObject rook_square = cur_piece.GetComponent<CurrentSquare>().currentSquare;
                // queen side castling
                if (rook_square.name == "A1")
                {
                    Debug.Log("A1 rook");
                    // king and rook not move
                    if (not_moved_castling[0, 0] && not_moved_castling[0, 1])
                    {
                        Debug.Log("not moved");
                        // if there is a queen side castling
                        if (checkNoPieceBetweenCastling(king, cur_piece))
                        {
                            Debug.Log("no piece between");
                            // set the color of the square
                            GameObject new_rook_square = GameObject.Find("D1");
                            SetColor(new_rook_square, Color.yellow);
                            // set the possible_moves to be true
                            int[] new_square = SquareToRowAndCol("D1");
                            possible_moves[new_square[0], new_square[1]] = true;
                            can_castling = true;
                        }
                    }
                }
                // king side castling
                else if (rook_square.name == "H1")
                {
                    // king and rook not move
                    if (not_moved_castling[0, 0] && not_moved_castling[0, 2])
                    {
                        // if there is a queen side castling
                        if (checkNoPieceBetweenCastling(king, cur_piece))
                        {
                            // set the color of the square
                            GameObject new_rook_square = GameObject.Find("F1");
                            SetColor(new_rook_square, Color.yellow);
                            // set the possible_moves to be true
                            int[] new_square = SquareToRowAndCol("F1");
                            possible_moves[new_square[0], new_square[1]] = true;
                            can_castling = true;
                        }
                    }
                }
            }
            // black rook
            else
            {
                GameObject king = GameObject.Find("king black");
                GameObject rook_square = cur_piece.GetComponent<CurrentSquare>().currentSquare;
                // queen side castling
                if (rook_square.name == "A8")
                {
                    // king and rook not move
                    if (not_moved_castling[1, 0] && not_moved_castling[1, 1])
                    {
                        // if there is a queen side castling
                        if (checkNoPieceBetweenCastling(king, cur_piece))
                        {
                            // set the color of the square
                            GameObject new_rook_square = GameObject.Find("D8");
                            SetColor(new_rook_square, Color.yellow);
                            // set the possible_moves to be true
                            int[] new_square = SquareToRowAndCol("D8");
                            possible_moves[new_square[0], new_square[1]] = true;
                            can_castling = true;
                        }
                    }
                }
                // king side castling
                else if (rook_square.name == "H8")
                {
                    // king and rook not move
                    if (not_moved_castling[1, 0] && not_moved_castling[1, 2])
                    {
                        // if there is a queen side castling
                        if (checkNoPieceBetweenCastling(king, cur_piece))
                        {
                            // set the color of the square
                            GameObject new_rook_square = GameObject.Find("F8");
                            SetColor(new_rook_square, Color.yellow);
                            // set the possible_moves to be true
                            int[] new_square = SquareToRowAndCol("F8");
                            possible_moves[new_square[0], new_square[1]] = true;
                            can_castling = true;
                        }
                    }
                }
            }
            
        }
    }

    // Given the king and the rook
    // return true if there is no pieces between them
    bool checkNoPieceBetweenCastling(GameObject king, GameObject rook)
    {
        GameObject rook_square = rook.GetComponent<CurrentSquare>().currentSquare;
        GameObject king_square = king.GetComponent<CurrentSquare>().currentSquare;
        // white
        if (king.name.Contains("white"))
        {
            if (rook_square.name == "A1")
            {
                // get the squares and pieces between king and rook
                GameObject squareB = GameObject.Find("B1");
                GameObject squareC = GameObject.Find("C1");
                GameObject squareD = GameObject.Find("D1");
                GameObject pieceB = squareB.GetComponent<CurrentPiece>().currentPiece;
                GameObject pieceC = squareC.GetComponent<CurrentPiece>().currentPiece;
                GameObject pieceD = squareD.GetComponent<CurrentPiece>().currentPiece;
                // if they are all null, return true
                if (pieceB == null && pieceC == null && pieceD == null)
                    return true;
            }
            else if (rook_square.name == "H1")
            {
                // get the squares and pieces between king and rook
                GameObject squareF = GameObject.Find("F1");
                GameObject squareG = GameObject.Find("G1");
                GameObject pieceF = squareF.GetComponent<CurrentPiece>().currentPiece;
                GameObject pieceG = squareG.GetComponent<CurrentPiece>().currentPiece;
                // if they are all null, return true
                if (pieceF == null && pieceG == null)
                    return true;
            }
        }
        // black
        else
        {
            if (rook_square.name == "A8")
            {
                // get the squares and pieces between king and rook
                GameObject squareB = GameObject.Find("B8");
                GameObject squareC = GameObject.Find("C8");
                GameObject squareD = GameObject.Find("D8");
                GameObject pieceB = squareB.GetComponent<CurrentPiece>().currentPiece;
                GameObject pieceC = squareC.GetComponent<CurrentPiece>().currentPiece;
                GameObject pieceD = squareD.GetComponent<CurrentPiece>().currentPiece;
                // if they are all null, return true
                if (pieceB == null && pieceC == null && pieceD == null)
                    return true;
            }
            else if (rook_square.name == "H8")
            {
                // get the squares and pieces between king and rook
                GameObject squareF = GameObject.Find("F8");
                GameObject squareG = GameObject.Find("G8");
                GameObject pieceF = squareF.GetComponent<CurrentPiece>().currentPiece;
                GameObject pieceG = squareG.GetComponent<CurrentPiece>().currentPiece;
                // if they are all null, return true
                if (pieceF == null && pieceG == null)
                    return true;
            }
        }
        return false;
    }
    
}
