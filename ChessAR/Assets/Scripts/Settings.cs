using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    float speed;

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
        angle = chessboard.transform.localEulerAngles;
        scale = chessboard.transform.localScale;
        startPosition = chessboard.transform.localPosition;
        position = startPosition;
        scaleFactor = 1;

        speed = 0;

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
                float ratio = Vector2.Distance(Input.GetTouch(0).position, Input.GetTouch(1).position) / touchDist;
                float newScaleFactor = scaleFactor;
                if (ratio > 1.1f)
                {
                    newScaleFactor *= (ratio - 0.1f);
                    if (newScaleFactor > 3.0f) newScaleFactor = 3.0f;
                }
                else if (ratio < 0.9f)
                {
                    newScaleFactor *= (ratio + 0.1f);
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

                if (accCount > 12)
                {
                    Handheld.Vibrate();
                    resetCount = 0;
                    resetScene();
                }
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
            float deltaFactor = chessboard.transform.localScale.x / scale.x - scaleFactor;
            chessboard.transform.localScale = scale * (scaleFactor + resetNum(deltaFactor, 0.05f));

            //position animation for reset
            Vector3 deltaPosition = chessboard.transform.localPosition - position;
            chessboard.transform.localPosition = resetVector3(deltaPosition, 0.05f) + position;
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
            speed = -0.05f * deltaAngle;
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

    float resetNum(float inputDelta, float step)
    {
        float delta = inputDelta;
        if (delta > 0.05f) delta -= 0.05f;
        else if (delta < -0.05f) delta += 0.05f;
        else delta = 0;
        return delta;
    }

    Vector3 resetVector3(Vector3 inputDelta, float step)
    {
        Vector3 delta = inputDelta;

        if (delta.x > step) delta.x -= step;
        else if (delta.x < -step) delta.x += step;
        else delta.x = 0;

        if (delta.y > step) delta.y -= step;
        else if (delta.y < -step) delta.y += step;
        else delta.y = 0;

        if (delta.z > step) delta.z -= step;
        else if (delta.z < -step) delta.z += step;
        else delta.z = 0;

        return delta;
    }

    public static void resetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
