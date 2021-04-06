using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    //UI components
    public Slider viewSlider;
    public Slider sizeSlider;
    public Slider angleSlider;
    public GameObject viewButton;
    public GameObject menu;

    //The chessboard object
    public GameObject chessboard;
    Vector3 angle;
    Vector3 scale;

    //-1 for default, 0 for press on view, 1 for long press on view, 2 for long press setting mode
    public static int settingControl;

    float speed;
    float touchTime;
    float touchNegTime;

    // Start is called before the first frame update
    void Start()
    {
        settingControl = -1;
        speed = 0;
        touchTime = -1;
        touchNegTime = -1;
        menu.SetActive(false);
        viewSlider.gameObject.SetActive(false);
        angle = chessboard.transform.localEulerAngles;
        scale = chessboard.transform.localScale;
        angleSlider.value = angle.y;
    }

    // Update is called once per frame
    void Update()
    {
        float bias = chessboard.transform.localEulerAngles.y - angle.y;

        if (settingControl == 1)
        {
            bias = 0;
            chessboard.transform.localEulerAngles = rotate(angle, -viewSlider.value);
        }
        else if (settingControl == 2)
        {
            menu.SetActive(true);
            chessboard.transform.localScale = scale * sizeSlider.value;
            angle.y = angleSlider.value;
        }

        if (Input.GetMouseButton(0))
        {
            touchNegTime = -1;

            if (touchTime < 0) touchTime = Time.time;

            float count = Time.time - touchTime;

            //Press >1s to trigger free angle adjustion
            if (settingControl == 0 && touchTime > 0 && count >= 1.0f)
            {
                viewButton.SetActive(false);
                viewSlider.gameObject.SetActive(true);
                viewSlider.value = 0;
                settingControl = 1;
                touchTime = -1;
            }
            //Press >2s to trigger setting menu
            else if (settingControl == -1 && touchTime > 0 && count >= 2.0f)
            {
                settingControl = 2;
                viewButton.SetActive(false);
                touchTime = -1;
            }
        }
        else
        {
            touchTime = -1;
            if (touchNegTime < 0) touchNegTime = Time.time;

            if (settingControl == 0)
            {
                angle = rotate(angle, 180.0f);
                settingControl = -1;
            }
            else if (settingControl == 1)
            {
                if (touchNegTime > 0 && Time.time - touchNegTime >= 1.0f)
                {
                    viewButton.SetActive(true);
                    viewSlider.value = 0;
                    viewSlider.gameObject.SetActive(false);
                    settingControl = -1;
                }
            }
            else if (settingControl == 2)
            {
                if (touchNegTime > 0 && Time.time - touchNegTime >= 3.0f)
                {
                    menu.SetActive(false);
                    viewButton.SetActive(true);
                    settingControl = -1;
                }
            }
        }

        //Rotate animation of the chessboard
        if (bias > 180.0f) bias -= 360.0f;
        else if (bias < -180.0f) bias += 360.0f;

        if (bias >= 1.0f || bias <= -1.0f)
        {
            speed = -0.05f * bias;
            if (speed > 0 && speed < 1) speed = 1.0f;
            else if (speed < 0 && speed > -1) speed = -1.0f;
            
            chessboard.transform.localEulerAngles = rotate(chessboard.transform.localEulerAngles, speed);
        }
    }

    Vector3 rotate(Vector3 angle, float degree)
    {
        Vector3 newAngle = angle + new Vector3(0, degree, 0);
        if (newAngle.y > 180) newAngle.y -= 360;
        else if (newAngle.y < -180) newAngle.y += 180;

        return newAngle;
    }
}
