using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorUpdater : MonoBehaviour
{
    public Material black;
    public Material white;
    public Material bwPiece;
    public Material bwSquare;
    public Material wwPiece;
    public Material wwSquare;
    public Material bsPiece;
    public Material bsSquare;
    public Material wsPiece;
    public Material wsSquare;
    private string selectedColor;
    private GameObject[] whiteSquares;
    private GameObject[] blackSquares;
    private GameObject[] whitePieces;
    private GameObject[] blackPieces;
    List<GameObject> whiteTops = new List<GameObject>();
    List<GameObject> blackTops = new List<GameObject>();
    void Start()
    {
        selectedColor = ColorManager.colorScheme;
        whiteSquares = GameObject.FindGameObjectsWithTag("white-square");
        blackSquares = GameObject.FindGameObjectsWithTag("black-square");
        whitePieces = GameObject.FindGameObjectsWithTag("white-piece");
        blackPieces = GameObject.FindGameObjectsWithTag("black-piece");
        foreach (Transform top in GameObject.Find("TopLayer").transform) {
            string name = top.gameObject.name;
            if (name == "White" || name.Split(' ')[0] == "White") {
                whiteTops.Add(top.gameObject);
            } else if (name == "Black" || name.Split(' ')[0] == "Black") {
                blackTops.Add(top.gameObject);
            }
        }
        SetColors(selectedColor);
    }
    void Update()
    {
        if (ColorManager.colorScheme != selectedColor) {
            selectedColor = ColorManager.colorScheme;
            SetColors(selectedColor);
        }
    }
    public void SetColors(string color)
    {
        if (color == "bw") {
            foreach (GameObject square in whiteSquares)
            {
                GameObject objectMain = square.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = white;
            }
            foreach (GameObject square in blackSquares)
            {
                GameObject objectMain = square.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = black;
            }
            foreach (GameObject piece in whitePieces)
            {
                GameObject objectMain = piece.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = white;
            }
            foreach (GameObject piece in blackPieces)
            {
                GameObject objectMain = piece.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = black;
            }
            foreach (GameObject top in whiteTops)
            {
                Renderer r = top.GetComponent<Renderer>();
                r.material = white;
            }
            foreach (GameObject top in blackTops)
            {
                Renderer r = top.GetComponent<Renderer>();
                r.material = black;
            }
        } else if (color == "wood") {
            foreach (GameObject square in whiteSquares)
            {
                GameObject objectMain = square.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = wwSquare;
            }
            foreach (GameObject square in blackSquares)
            {
                GameObject objectMain = square.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = bwSquare;
            }
            foreach (GameObject piece in whitePieces)
            {
                GameObject objectMain = piece.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = wwPiece;
            }
            foreach (GameObject piece in blackPieces)
            {
                GameObject objectMain = piece.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = bwPiece;
            }
            foreach (GameObject top in whiteTops)
            {
                Renderer r = top.GetComponent<Renderer>();
                r.material = wwSquare;
            }
            foreach (GameObject top in blackTops)
            {
                Renderer r = top.GetComponent<Renderer>();
                r.material = bwSquare;
            }
        } else if (color == "stone") {
            foreach (GameObject square in whiteSquares)
            {
                GameObject objectMain = square.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = wsSquare;
            }
            foreach (GameObject square in blackSquares)
            {
                GameObject objectMain = square.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = bsSquare;
            }
            foreach (GameObject piece in whitePieces)
            {
                GameObject objectMain = piece.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = wsPiece;
            }
            foreach (GameObject piece in blackPieces)
            {
                GameObject objectMain = piece.transform.parent.gameObject;
                Renderer r = objectMain.GetComponent<Renderer>();
                r.material = bsPiece;
            }
            foreach (GameObject top in whiteTops)
            {
                Renderer r = top.GetComponent<Renderer>();
                r.material = wsSquare;
            }
            foreach (GameObject top in blackTops)
            {
                Renderer r = top.GetComponent<Renderer>();
                r.material = bsSquare;
            }
        }
    }
}