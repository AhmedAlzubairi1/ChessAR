using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModalManager : MonoBehaviour
{
private string selectedColor;
    private GameObject bw;
    private GameObject wood;
    private GameObject stone;
    private GameObject indicator;
    public GameObject Modal;
    public GameObject Home;
    public GameObject Help;
    public GameObject Settings;
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
    public void ShowHome() {
        Modal.SetActive(true);
        Help.SetActive(false);
        Settings.SetActive(false);
        Home.SetActive(true);
    }
    public void ShowHelp() {
        Modal.SetActive(true);
        Home.SetActive(false);
        Settings.SetActive(false);
        Help.SetActive(true);
    }
    public void ShowSettings() {
        Modal.SetActive(true);
        Home.SetActive(false);
        Help.SetActive(false);
        Settings.SetActive(true);
    }
    public void Close() {
        Home.SetActive(false);
        Help.SetActive(false);
        Settings.SetActive(false);
        Modal.SetActive(false);
    }
    public void Exit() {
        SceneManager.LoadScene("StartMenu");
    }
}
