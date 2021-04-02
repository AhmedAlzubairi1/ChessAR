using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySelect : MonoBehaviour {
  public Camera cm;
  public Color selectColor;
  public GameObject selected;
  private Color oldColor;
  private GameObject prevGm;
  private Renderer prevGmr;

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

    // SelectedHandler();
  }
  void FindObj() {
    // Cast ray from the screen
    RaycastHit hit;
    Ray raycast = cm.ScreenPointToRay(Input.mousePosition);

    // Detect raycast hit an object
    if (Physics.Raycast(raycast, out hit)) {
      GameObject gm = hit.transform.gameObject;
      if (gm.tag == "Piece") { // make sure it's a piece
        Renderer gmr = gm.GetComponent<Renderer>(); // store current gameObject's renderer
        bool same = false;
        if (prevGm && prevGmr) { // if a previous selection exists
          prevGmr.material.color = oldColor; // set it to old color
          if (prevGm == gm) { same = true; } // same as previous selection...
        }
        prevGm = gm; // overwrite previous selection gameObject with current
        prevGmr = gmr; // overwrite previous selection renderer with current

        oldColor = gmr.material.color; // store current selection's color
        if (!same) { // only indicate new selection for different pieces
          gmr.material.color = selectColor;
          selected = gm;
        } else {
          selected = null;
        }
      }
    }
  }
  // void SelectedHandler() {
  //   selected.
  // }
}
