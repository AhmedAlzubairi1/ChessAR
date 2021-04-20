using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorManager : MonoBehaviour
{
    public static string colorScheme = "bw";
    public void SetBW() {
        colorScheme = "bw";
    }
    public void SetWood() {
        colorScheme = "wood";
    }
    public void SetStone() {
        colorScheme = "stone";
    }
}
