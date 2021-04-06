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

  void Start() {
    moveButton.SetActive(false);
  }
  void Update() {
  #if UNITY_IPHONE || UNITY_ANDROID
    if ((Input.touchCount > 0) && (Input.GetTouch(0).phase == TouchPhase.Began)) {
      FindObj();
    }
  #else
    if (Input.GetMouseButtonDown(0)) {
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
    if (!moveButton.activeSelf) {
      if (Physics.Raycast(raycast, out hit)) {
        GameObject gm = hit.transform.gameObject;
        if (gm.tag == "Piece") { // make sure it's a piece
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
          } else {
            selectedPiece = null;
          }
        } else if (gm.tag == "Square") { // make sure it's a square, all same as above
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
    if (selectedPiece && selectedSquare) {
      moveButton.SetActive(true);
    } else {
      moveButton.SetActive(false);
    }
  }
  public void MovePiece() {
    StartCoroutine(TranslatePiece());
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
  }
}
