using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vuforia;

public class Settings : MonoBehaviour
{
    //The chessboard object
    public Camera ARcamera;
    public GameObject chessboard;
    Vector3 angle;
    Vector3 scale;
    Vector3 startPosition;
    Vector3 position;
    float scaleFactor;

    //animation variable
    float angleSpeed;
    Vector3 scaleSpeed;
    Vector3 posSpeed;

    //decided by the number of fingers on the phone
    int settingStage;

    //touch record (time, position, etc.)
    float touchRecord;
    float touchDist;
    Vector2 touchPos1;
    Vector2 touchPos2;
    Vector3 relaPos;

    //gyroscope information
    bool gyinfo;
    float goAngle;
    Gyroscope go;

    //acceleration variables
    float prevAcc;
    float accRecord;
    int accDirection;
    int accCount;
    int resetCount;

    // Start is called before the first frame update
    void Start()
    {
        //set auto focusing
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(start);
        VuforiaARController.Instance.RegisterOnPauseCallback(pause);

        angle = chessboard.transform.localEulerAngles;
        scale = chessboard.transform.localScale;
        startPosition = chessboard.transform.localPosition;
        position = startPosition;
        scaleFactor = 1;

        angleSpeed = 0;

        settingStage = 0;

        touchRecord = -1;
        touchDist = 0;

        gyinfo = SystemInfo.supportsGyroscope;
        go = Input.gyro;
        go.enabled = true;
        goAngle = 0;

        prevAcc = 0;
        accRecord = -1;
        accDirection = 0;
        accCount = 0;
        resetCount = 0;
    }

    void start()
    {
        //when program start
        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    void pause(bool paused)
    {
        //when program return from pause
        if (!paused) CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    // Update is called once per frame
    void Update()
    {
        //quick slash to rotate 180
        if (Input.touchCount == 1 && settingStage != -1)
        {
            if (settingStage == 0)
            {
                settingStage = 1;
                touchRecord = Time.time;
                touchPos1 = Input.GetTouch(0).position;
            }
            else if (settingStage > 1) settingStage = -1;

            touchPos2 = Input.GetTouch(0).position;
        }
        //resize using two fingers
        else if (Input.touchCount == 2 && settingStage != -1)
        {
            if (settingStage < 2)
            {
                settingStage = 2;
                touchDist = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position);
            }
            else if (settingStage > 2) settingStage = -1;
            else
            {
                //reserves 0.9-1.1 for no-change
                float ratio = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) / touchDist;
                float newScaleFactor = scaleFactor;
                if (ratio > 1.1f)
                {
                    newScaleFactor *= Mathf.Pow(ratio - 0.1f, 1.5f);
                    if (newScaleFactor > 90.0f) newScaleFactor = 90.0f;
                }
                else if (ratio < 0.9f)
                {
                    newScaleFactor *= Mathf.Pow(ratio + 0.1f, 1.5f);
                    if (newScaleFactor < 0.3f) newScaleFactor = 0.3f;
                }

                chessboard.transform.localScale = scale * newScaleFactor;
            }
        }
        //move or rotate the chessboard
        else if (Input.touchCount == 3 && settingStage != -1)
        {
            if (settingStage < 3)
            {
                settingStage = 3;
                relaPos = ARcamera.transform.position - chessboard.transform.position;
                if (gyinfo) goAngle = go.attitude.eulerAngles.z + angle.y;
            }
            else
            {
                chessboard.transform.position = ARcamera.transform.position - relaPos;
                position = chessboard.transform.localPosition;
                if (gyinfo) angle = rotate(Vector3.zero, goAngle - go.attitude.eulerAngles.z);
            }
        }
        else if (Input.touchCount > 3 && settingStage != -1) settingStage = -1;
        //for reset functions
        else if (Input.touchCount == 0 && settingStage == 0)
        {
            //count the acceleration
            float deltaAcc = Input.acceleration.y - prevAcc;
            if (Mathf.Abs(deltaAcc) > 1.0f)
            {
                accRecord = Time.time;

                if (deltaAcc > 0 && accDirection < 1)
                {
                    accDirection = 1;
                    accCount += 1;
                }
                else if (deltaAcc < 0 && accDirection > -1)
                {
                    accDirection = -1;
                    accCount += 1;
                }
            }
            prevAcc = Input.acceleration.y;

            //reset if overtime
            if (accRecord > 0 && Time.time - accRecord > 0.3f)
            {
                accRecord = -1;
                accDirection = 0;
                accCount = 0;
            }

            //reset the chessboard parameters
            if (accCount > 6)
            {
                if (resetCount == 0)
                {
                    Handheld.Vibrate();
                    resetCount = 1;
                }

                //game reset
                if (accCount > 12)
                {
                    Handheld.Vibrate();
                    resetCount = 0;
                    resetScene();
                }
                //settings reset
                else if (Time.time - accRecord > 0.25f)
                {
                    accRecord = -1;
                    accDirection = 0;
                    accCount = 0;
                    resetCount = 0;

                    angle = Vector3.zero;
                    scaleFactor = 1;
                    position = startPosition;
                }
            }

            //scale animation for reset
            chessboard.transform.localScale = Vector3.SmoothDamp(chessboard.transform.localScale, scale * scaleFactor, ref scaleSpeed, 0.2f);

            //position animation for reset
            chessboard.transform.localPosition = Vector3.SmoothDamp(chessboard.transform.localPosition, position, ref posSpeed, 0.2f);
        }
        else
        {
            //distance > 50, direction = x, time < 0.3s
            if (settingStage == 1)
            {
                touchDist = Vector2.Distance(touchPos1, touchPos2);
                float direction = Mathf.Abs(touchPos1.x - touchPos2.x) - Mathf.Abs(touchPos1.y - touchPos2.y);
                if (touchDist > 50 && direction > 0 && Time.time - touchRecord < 0.3f)
                    angle = rotate(angle, 180.0f);

                touchRecord = -1;
            }
            else if (settingStage == 2 || settingStage == -1) scaleFactor = chessboard.transform.localScale.x / scale.x;

            if (Input.touchCount == 0 && settingStage != 0) settingStage = 0;
        }

        //Rotate animation of the chessboard
        float deltaAngle = chessboard.transform.localEulerAngles.y - angle.y;
        if (deltaAngle > 180.0f) deltaAngle -= 360.0f;
        else if (deltaAngle < -180.0f) deltaAngle += 360.0f;
        
        if (deltaAngle >= 1.0f || deltaAngle <= -1.0f)
        {
            angleSpeed = -0.05f * deltaAngle;
            if (angleSpeed > 0 && angleSpeed < 1) angleSpeed = 1.0f;
            else if (angleSpeed < 0 && angleSpeed > -1) angleSpeed = -1.0f;
            
            chessboard.transform.localEulerAngles = rotate(chessboard.transform.localEulerAngles, angleSpeed);
        }
        
    }

    //rotate the angle
    Vector3 rotate(Vector3 angle, float degree)
    {
        Vector3 newAngle = angle + new Vector3(0, degree, 0);
        if (newAngle.y > 180) newAngle.y -= 360;
        else if (newAngle.y < -180) newAngle.y += 180;

        return newAngle;
    }

    //reset the game to start
    public static void resetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
