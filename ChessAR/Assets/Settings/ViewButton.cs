using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ViewAct()
    {
        if (Settings.settingControl == -1) Settings.settingControl = 0;
    }
}
