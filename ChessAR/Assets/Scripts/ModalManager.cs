using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ModalManager : MonoBehaviour
{
    private string selectedColor;
    public GameObject bw;
    public GameObject wood;
    public GameObject stone;
    public GameObject indicator;
    public GameObject Modal;
    public GameObject Home;
    public GameObject Help;
    public GameObject Settings;
    void Start() {
        selectedColor = ColorManager.colorScheme;
        UpdateIndicator();
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
