using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentPiece : MonoBehaviour
{
    public GameObject currentPiece;
    // list of tag of pieces
    private List<string> pieces_list = new List<string>() {
    "Rook",
    "Bishop",
    "Knight",
    "Queen",
    "King",
    "Pawn"
    };
    void OnTriggerStay(Collider other) 
    {
        // Detect the square a piece is on
        if (pieces_list.Contains(other.gameObject.tag)) 
        {
            // only update the public variable when it's a new square
            currentPiece = other.gameObject;
        }
    }
    // set the currentPiece to be null
    void OnTriggerExit(Collider other) 
    {
        currentPiece = null;
    }
}