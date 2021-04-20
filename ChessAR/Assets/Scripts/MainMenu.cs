using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private string selectedColor;
    private GameObject bw;
    private GameObject wood;
    private GameObject stone;
    private GameObject indicator;
    void Start() {
        selectedColor = ColorManager.colorScheme;
        bw = GameObject.Find("bw-button");
        wood = GameObject.Find("wood-button");
        stone = GameObject.Find("stone-button");
        indicator = GameObject.Find("indicator");
    }
    void Update() {
        if (ColorManager.colorScheme != selectedColor) {
            selectedColor = ColorManager.colorScheme;
            UpdateIndicator();
        }
    }
    void UpdateIndicator() {
        if (selectedColor == "bw") {
            indicator.transform.localPosition = bw.transform.localPosition;
        } else if (selectedColor == "wood") {
            indicator.transform.localPosition = wood.transform.localPosition;
        } else if (selectedColor == "stone") {
            indicator.transform.localPosition = stone.transform.localPosition;
        }
    }
    public void NewGame() {
        SceneManager.LoadScene("chessScene");
    }
}
