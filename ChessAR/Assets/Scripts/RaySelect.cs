using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySelect : MonoBehaviour {
  public Camera cm;
  public Color pieceSelectColor;
  public Color squareSelectColor;
  public GameObject selectedPiece;
  public GameObject selectedSquare;
  public GameObject moveButton;

  [Range(0f, 4f)]
  public float movementSpeed = 3f;

  private Color oldColorPiece;
  private GameObject prevGmPiece;
  private Renderer prevGmrPiece;
  private GameObject selectedPieceSquare;
  private Color oldColorSquare;
  private GameObject prevGmSquare;
  private Renderer prevGmrSquare;
  private int journeyLength = 2;

  // 2D array of possible moves of a piece
  private bool [,] possible_moves = new bool [8,8] {
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
  private List<string> pieces_list = new List<string>() {
   "Rook",
   "Bishop",
   "Knight",
   "Queen",
   "King",
   "Pawn"
  };


  void Start() {
    moveButton.SetActive(false);
  }
  void Update() {
  #if UNITY_IPHONE || UNITY_ANDROID
    if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) {
      //resetChessBoardColor();
      FindObj();
    }
  #else
    if (Input.GetMouseButtonDown(0)) {
      //resetChessBoardColor();
      FindObj();
    }
  #endif
    SelectedHandler();
  }
  void FindObj() {
    // Cast ray from the screen
    RaycastHit hit;
    Ray raycast = cm.ScreenPointToRay(Input.mousePosition);

    // Detect raycast hit an object
    if (!moveButton.activeSelf) 
    {
      if (Physics.Raycast(raycast, out hit)) 
      {
        GameObject gm = hit.transform.gameObject;
        //if (gm.tag == "Piece") { // make sure it's a piece
        // a piece
        if (pieces_list.Contains(gm.tag)) {
          Debug.Log(gm.GetComponent<CurrentSquare>().currentSquare.name);
          Debug.Log(gm.name);
          // show possible moves
          findPossibleMoves(gm);

          Renderer gmr = gm.GetComponent<Renderer>(); // store current gameObject's renderer
          bool same = false;
          if (prevGmPiece && prevGmrPiece) { // if a previous selection exists
            prevGmrPiece.material.color = oldColorPiece; // set it to old color
            if (prevGmPiece == gm) { same = true; } // same as previous selection...
          }
          prevGmPiece = gm; // overwrite previous selection gameObject with current
          prevGmrPiece = gmr; // overwrite previous selection renderer with current

          oldColorPiece = gmr.material.color; // store current selection's color
          if (!same) { // only indicate new selection for different pieces
            gmr.material.color = pieceSelectColor;
            selectedPiece = gm;
            selectedPieceSquare = gm.GetComponent<CurrentSquare>().currentSquare;
            
          } 
          else {
            selectedPiece = null;
          }
        } 
        else if (gm.tag == "Square") 
        { // make sure it's a square, all same as above
          Renderer gmr = gm.GetComponent<Renderer>(); // store current gameObject's renderer
          bool same = false;
          if (prevGmSquare && prevGmrSquare) { // if a previous selection exists
            prevGmrSquare.material.color = oldColorSquare; // set it to old color
            if (prevGmSquare == gm) { same = true; } // same as previous selection...
          }
          prevGmSquare = gm; // overwrite previous selection gameObject with current
          prevGmrSquare = gmr; // overwrite previous selection renderer with current

          oldColorSquare = gmr.material.color; // store current selection's color
          if (!same) { // only indicate new selection for different pieces
            gmr.material.color = squareSelectColor;
            selectedSquare = gm;
          } else {
            selectedSquare = null;
          }
        }
      }
    }
  }

  void SelectedHandler() {
    // need to select piece before select square 
     if (selectedPiece)
    {
      if (selectedSquare) 
      {
        // get the row and col of the selected square
        string new_square_name = selectedSquare.name;
        int[] row_col = SquareToRowAndCol(new_square_name);
        int new_row = row_col[0];
        int new_col = row_col[1];
        // if the selected square is not in possible_moves movies
        if (possible_moves[new_row, new_col] == false)
        {
          // get the render and set the color to red
          Renderer new_square_renderer = selectedSquare.GetComponent<Renderer>();
          new_square_renderer.material.color = Color.red;
        }
        moveButton.SetActive(true);
      }
    } 
    else 
    {
      // reset chess borad and the selected piece
      resetAll();
    }
  }
  public void MovePiece() {
    // get the row and col of the selected square
    string new_square_name = selectedSquare.name;
    int[] row_col = SquareToRowAndCol(new_square_name);
    int new_row = row_col[0];
    int new_col = row_col[1];
    // check if this is a valid moves and move it only it is valid
    if (possible_moves[new_row, new_col] == true)
    {
      StartCoroutine(TranslatePiece());
    }
    else 
    {
      resetAll();
    }
  }

  private IEnumerator TranslatePiece() {
    for (float t = 0; t < journeyLength; t += Time.deltaTime) {
      float alpha = t / journeyLength;
      selectedPiece.transform.position = Vector3.Lerp(
        selectedPiece.transform.position,
        selectedSquare.transform.position,
        alpha
      );
      yield return null;
    }
    selectedPiece.transform.position = selectedSquare.transform.position;
    moveButton.SetActive(false);
    prevGmrPiece.material.color = oldColorPiece;
    prevGmrSquare.material.color = oldColorSquare;
    selectedPiece = null;
    selectedSquare = null;
    prevGmPiece = null;
    prevGmrPiece = null;
    selectedPieceSquare = null;
    prevGmSquare = null;
    prevGmrSquare = null;

    // reset chess board color after each move
    resetChessBoardColor();
    clearPossibleMoves();
  }

  // -------------------------------------------------------------------------------
  // 
  // find possible_moves
  void findPossibleMoves(GameObject current_piece) 
  {
    // get current square
    GameObject current_square = current_piece.GetComponent<CurrentSquare>().currentSquare;
    bool is_white_piece = current_piece.name.Contains("white");
    int[] row_col = SquareToRowAndCol(current_square.name);
    int current_row = row_col[0];
    int current_col = row_col[1];
    Debug.Log("current_row: " + current_row);
    Debug.Log("current_col: " + current_col);
    // reset possible_moves array
    clearPossibleMoves();
    // modify possible_moves array
    switch(current_piece.tag) 
    {
      // Rook moves
      case "Rook":
        setCorssMoves(current_row, current_col, current_piece);
        break;
      // Bishop moves
      case "Bishop":
        setDiagonalMoves(current_row, current_col, current_piece);
        break;
      // Knight moves
      case "Knight":
        // possible directions of knight
        int[,] knight_directions = new int[8, 2] { {2, -1}, {2, 1}, {-2, -1}, {-2, 1}, {1, -2}, {-1, -2}, {1, 2}, {-1, 2} };
        // for each direction
        for(int i = 0; i < possible_moves.GetLength(0); i++)
        {
          int new_row = current_row + knight_directions[i, 0];
          int new_col = current_col + knight_directions[i, 1];
          if (isValidMove(new_row, new_col, current_piece))
          {
            //set possible_moves to be true
            possible_moves[new_row, new_col] = true;
          }
        }
        break;
      case "Queen":
        setCorssMoves(current_row, current_col, current_piece);
        setDiagonalMoves(current_row, current_col, current_piece);
        break;
      // King moves
      case "King":
        // possible directions of king
        int[,] king_directions = new int[8, 2] { {1, 0}, {-1, 0}, {0, -1}, {0, 1}, {1, 1}, {1, -1}, {-1, 1}, {-1, -1} };
        // for each direction
        for(int i = 0; i < possible_moves.GetLength(0); i++)
        {
          int new_row = current_row + king_directions[i, 0];
          int new_col = current_col + king_directions[i, 1];
          // new row and col are not out of bound
          //if (0 <= new_row && new_row <= 7 && 0 <= new_col && new_col <= 7)
          if (isValidMove(new_row, new_col, current_piece))
          {
            // set possible_moves to be true
            possible_moves[new_row, new_col] = true;
          }
        }
        break;
      // Pawn moves
      case "Pawn":
        // White Pawn
        if (is_white_piece) 
        {
          int new_row = current_row + 1;
          int new_col = current_col;
          if (isValidMove(new_row, new_col, current_piece))
          {
            // set possible_moves to be true
            possible_moves[new_row, new_col] = true;
          }
          // at second row
          if (current_row == 1)
          {
            new_row += 1;
            if (isValidMove(new_row, new_col, current_piece))
            {
              // set possible_moves to be true
              possible_moves[new_row, new_col] = true;
            }
          }
        }
        // black Pawn
        else
        {
          // black Pawn is moving down the board
          int new_row = current_row - 1;
          int new_col = current_col;
          if (isValidMove(new_row, new_col, current_piece))
          {
            // set possible_moves to be true
            possible_moves[new_row, new_col] = true;
          }
          // at second row
          if (current_row == 6)
          {
            new_row -= 1;
            if (isValidMove(new_row, new_col, current_piece))
            {
              // set possible_moves to be true
              possible_moves[new_row, new_col] = true;
            }
          }
        }
        break;
    }
    // debug
    DebugPrintPossibleMoves();
    showPossibleMoves();
  }

  // reflect possible_moves arrry to the chess board
  void showPossibleMoves()
  {
    for(int i = 0; i < possible_moves.GetLength(0); i++)
    {
      for (int j = 0; j < possible_moves.GetLength(1); j++)
      {
        if (possible_moves[i, j] == true)
        {
          // get possiblt square tag
          string possible_square_name = RowAndColToSquare(i, j);
          Debug.Log("possible_square_tag: " + possible_square_name);
          // get game object
          GameObject possible_square = GameObject.Find(possible_square_name);
          // get renderer
          Renderer possible_square_renderer = possible_square.GetComponent<Renderer>();
          // set color of the material to squareSelectColor
          possible_square_renderer.material.color = squareSelectColor;
        }
      }
    }
  }

  // set the corss moves for Rook or Queen
  void setCorssMoves(int current_row, int current_col, GameObject current_piece)
  {
    // possible directions of corss move 
    int[,] rook_directions = new int[2, 7] { 
      {1, 2, 3, 4, 5, 6, 7},
      {-1, -2, -3, -4, -5, -6, -7}
    };
    // for 4 directions (up, down, left, right)
    for(int i = 0; i < 4; i++)
    {
      // up
      for (int j = 0; j < 7; j++)
      {
        // row changes, col does not change
        int new_row = current_row + rook_directions[0, j];
        int new_col = current_col;
        // there is a piece blocking the way. break
        if (!isValidMove(new_row, new_col, current_piece))
        {
          break;
        }
        //set possible_moves to be true
        possible_moves[new_row, new_col] = true;
      }
      // down
      for (int j = 0; j < 7; j++)
      {
        // row changes, col does not change
        int new_row = current_row + rook_directions[1, j];
        int new_col = current_col;
        // there is a piece blocking the way. break
        if (!isValidMove(new_row, new_col, current_piece))
        {
          break;
        }
        //set possible_moves to be true
        possible_moves[new_row, new_col] = true;
      }
      // left
      for (int j = 0; j < 7; j++)
      {
        // col changes, row does not change
        int new_row = current_row;
        int new_col = current_col + rook_directions[1, j];
        // there is a piece blocking the way. break
        if (!isValidMove(new_row, new_col, current_piece))
        {
          break;
        }
        //set possible_moves to be true
        possible_moves[new_row, new_col] = true;
      }
      // right
      for (int j = 0; j < 7; j++)
      {
        // col changes, row does not change
        int new_row = current_row;
        int new_col = current_col + rook_directions[0, j];
        // there is a piece blocking the way. break
        if (!isValidMove(new_row, new_col, current_piece))
        {
          break;
        }
        //set possible_moves to be true
        possible_moves[new_row, new_col] = true;
      }
    }
  }

  // set the diagonal moves for Bishop or Queen
  void setDiagonalMoves(int current_row, int current_col, GameObject current_piece)
  {
    // possible directions of diagonal move 
    int[,] rook_directions = new int[2, 7] { 
      {1, 2, 3, 4, 5, 6, 7},
      {-1, -2, -3, -4, -5, -6, -7}
    };
    // for 4 directions (top left, top right, bottom left, bottom right)
    for(int i = 0; i < 4; i++)
    {
      // top left
      for (int j = 0; j < 7; j++)
      {
        // row changes, col does not change
        int new_row = current_row + rook_directions[0, j];
        int new_col = current_col + rook_directions[1, j];
        // there is a piece blocking the way. break
        if (!isValidMove(new_row, new_col, current_piece))
        {
          break;
        }
        //set possible_moves to be true
        possible_moves[new_row, new_col] = true;
      }
      // top right
      for (int j = 0; j < 7; j++)
      {
        // row changes, col does not change
        int new_row = current_row + rook_directions[0, j];
        int new_col = current_col + rook_directions[0, j];
        // there is a piece blocking the way. break
        if (!isValidMove(new_row, new_col, current_piece))
        {
          break;
        }
        //set possible_moves to be true
        possible_moves[new_row, new_col] = true;
      }
      // bottom left
      for (int j = 0; j < 7; j++)
      {
        // col changes, row does not change
        int new_row = current_row + rook_directions[1, j];
        int new_col = current_col + rook_directions[1, j];
        // there is a piece blocking the way. break
        if (!isValidMove(new_row, new_col, current_piece))
        {
          break;
        }
        //set possible_moves to be true
        possible_moves[new_row, new_col] = true;
      }
      // bottom right
      for (int j = 0; j < 7; j++)
      {
        // col changes, row does not change
        int new_row = current_row + rook_directions[1, j];
        int new_col = current_col + rook_directions[0, j];
        // there is a piece blocking the way. break
        if (!isValidMove(new_row, new_col, current_piece))
        {
          break;
        }
        //set possible_moves to be true
        possible_moves[new_row, new_col] = true;
      }
    }
  }

  // given the new row, new col and current piece 
  // return true if there is no pieces with the same color on it or it is not out of bound
  bool isValidMove(int new_row, int new_col, GameObject current_piece)
  {
    // new row and col are not out of bound
    if (0 <= new_row && new_row <= 7 && 0 <= new_col && new_col <= 7)
    {
      // name of the destination square
      string des_square_name = RowAndColToSquare(new_row, new_col);
      // get des square
      GameObject des_square = GameObject.Find(des_square_name);
      // get piece on the destination square
      GameObject des_piece = des_square.GetComponent<CurrentPiece>().currentPiece;
      // if des_piece is null, return true
      if (des_piece == null)
      {
        return true;
      }
      else
      {
        // get current piece's renderer and color
        Renderer current_piece_renderer = current_piece.GetComponent<Renderer>();
        Color current_piece_color = current_piece_renderer.material.color;
        // get des piece's renderer and color
        Renderer des_piece_renderer = des_piece.GetComponent<Renderer>();
        Color des_piece_color = des_piece_renderer.material.color;
        // if they have the same color
        if (current_piece_color.Equals(des_piece_color))
        {
          return false;
        }
        else
        {
          return true;
        }
      }
    }
    else
    {
      return false;
    }
  }

  // reset chess board color
  void resetChessBoardColor()
  {
    for(int i = 0; i < possible_moves.GetLength(0); i++)
    {
      for (int j = 0; j < possible_moves.GetLength(1); j++)
      {
        // get possiblt square tag
        string possible_square_name = RowAndColToSquare(i, j);
        // get game object
        GameObject possible_square = GameObject.Find(possible_square_name);
        // get renderer
        Renderer possible_square_renderer = possible_square.GetComponent<Renderer>();
        // black cell if row + col % 2 = 0
        if ( (i + j) % 2 == 0)
        {
          // set color of the material to black
          possible_square_renderer.material.color = Color.black;
        }
        // white cell
        else
        {
          // set color of the material to white
          possible_square_renderer.material.color = Color.white;
        }
      }
    }
  }

  // reset everything
  void resetAll()
  {
     moveButton.SetActive(false);
    if (prevGmrPiece != null)
    {
      prevGmrPiece.material.color = oldColorPiece;
    }
    if (prevGmrSquare != null)
    {
      prevGmrSquare.material.color = oldColorSquare;
    }
    selectedPiece = null;
    selectedSquare = null;
    prevGmPiece = null;
    prevGmrPiece = null;
    selectedPieceSquare = null;
    prevGmSquare = null;
    prevGmrSquare = null;
    resetChessBoardColor();
    clearPossibleMoves();
  }

  // given current square, return row and col
  int[] SquareToRowAndCol(string square) 
  {
    int row = (int)(square[1] - '0' - 1);
    int col = 0;
    switch(square[0]) 
    {
      case 'A':
        col = 0;
        break;
      case 'B':
        col = 1;
        break;
      case 'C':
        col = 2;
        break;
      case 'D':
        col = 3;
        break;
      case 'E':
        col = 4;
        break;
      case 'F':
        col = 5;
        break;
      case 'G':
        col = 6;
        break;
      case 'H':
        col = 7;
        break;
    }
    return new int[] {row, col};
  }
  // given row and col, return square
  string RowAndColToSquare(int row, int col)
  {
    string res = "";
    switch(col) 
    {
      case 0:
        res = "A";
        break;
      case 1:
        res = "B";
        break;
      case 2:
        res = "C";
        break;
      case 3:
        res = "D";
        break;
      case 4:
        res = "E";
        break;
      case 5:
        res = "F";
        break;
      case 6:
        res = "G";
        break;
      case 7:
        res = "H";
        break;
    }
    return res + (row + 1);
  }

  // set all elements in possible_moves to false
  void clearPossibleMoves() 
  {
    for(int i = 0; i < possible_moves.GetLength(0); i++)
    {
      for (int j = 0; j < possible_moves.GetLength(1); j++)
      {
        possible_moves[i, j] = false;
      }
    }
  }

  // used for debug
  void DebugPrintPossibleMoves()
  {
    string log = "";
    for(int i = 0; i < possible_moves.GetLength(0); i++)
    {
      log += "[ ";
      for (int j = 0; j < possible_moves.GetLength(1); j++)
      {
        if(possible_moves[i, j] == true)
        {
          log += "1 ";
        }
        else
        {
          log += "0 ";
        }
      }
      log += "]\n";
    }
    Debug.Log(log);
  }

}
