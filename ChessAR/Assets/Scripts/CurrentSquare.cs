using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrentSquare : MonoBehaviour {
    public GameObject currentSquare;
    void OnTriggerStay(Collider other) {
        // Detect the square a piece is on
        if (other.gameObject.tag == "Square") {
            if (currentSquare != other.gameObject) {
                // only update the public variable when it's a new square
                currentSquare = other.gameObject;
            }
        }
    }
}
