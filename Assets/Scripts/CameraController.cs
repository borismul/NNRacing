using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

    enum carFollowObject { thirdPerson, hood, top }

    // Object that the camera can follow
    GameObject[] followObjects;

    // Current object that the camera is following
    carFollowObject followObjectIndex;

    // Lerp speed of the camera rotation
    public float thirdLerpSpeedTransform = 10f;
    public float thirdLerpSpeedRotation = 10f;
    public float hoodLerpSpeedTransform = 10f;
    public float hoodLerpSpeedRotation = 10f;

    public static CameraController currentActiveMainCamera;

    CarController car;

    void Update()
    {
        SetNextFollowObject();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //UpdateTransform();
    }

    void OnEnable()
    {
        if (followObjects == null)
            return;

        transform.position = followObjects[(int)followObjectIndex].transform.position;
        transform.rotation = followObjects[(int)followObjectIndex].transform.rotation;
    }

    // Method that lets the camera follow the current carFollowObject
    public void UpdateTransform()
    {
        if (followObjects == null)
            return;

        if (Vector3.Distance(transform.position, followObjects[(int)followObjectIndex].transform.position) > 2)
        {
            transform.position = followObjects[(int)followObjectIndex].transform.position;
            transform.rotation = followObjects[(int)followObjectIndex].transform.rotation;
            return;
        }

        if (followObjectIndex == carFollowObject.thirdPerson)
        {
            // Follow exactly the position
            transform.position = Vector3.Lerp(transform.position, followObjects[(int)followObjectIndex].transform.position, (car.maxSpeed * 0.4f + Mathf.Sqrt(car.velocity.magnitude)) * 1f/GA_Parameters.fps);

            // Lerp towards rotation to avoid stuttering
            transform.rotation = Quaternion.Lerp(transform.rotation, followObjects[(int)followObjectIndex].transform.rotation, (car.turnSpeed * 0.1f) * 1f / GA_Parameters.fps);
        }

        else if (followObjectIndex == carFollowObject.hood)
        {
            // Follow exactly the position
            transform.position = followObjects[(int)followObjectIndex].transform.position;

            // Lerp towards rotation to avoid stuttering
            transform.rotation = Quaternion.Lerp(transform.rotation, followObjects[(int)followObjectIndex].transform.rotation, car.turnSpeed * 1f / GA_Parameters.fps);
        }

        else
        {
            transform.position = followObjects[(int)followObjectIndex].transform.position;
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    // Method that allows to set another car to follow
    public void SetFollowCar(CarController carController)
    {
        car = carController;
        followObjects = carController.carFollowObjects;
    }

    // Method that checks if C is pressed and if so changes the current follow object to the next in the list
    void SetNextFollowObject()
    {
        if (!Input.GetKeyDown(KeyCode.C))
            return;

        if ((int)followObjectIndex + 1 < followObjects.Length)
            followObjectIndex++;
        else
            followObjectIndex = 0;

        transform.position = followObjects[(int)followObjectIndex].transform.position;
        transform.rotation = followObjects[(int)followObjectIndex].transform.rotation;
    }


}
