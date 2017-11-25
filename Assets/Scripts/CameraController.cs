using UnityEngine;
using System.Collections.Generic;


public class CameraController : MonoBehaviour
{

    public static CameraController instance;

    public GameObject menuCamera;

    enum carFollowObject { thirdPerson, hood, top }

    // List of cars that can be followed by the camera
    List<CarController> followCars = new List<CarController>();

    // Car that is being followed
    CarController currentFollowCar;

    // Integer that says which followcar is currently being followed
    int currentFollowCarNum = 0;

    // Object that the camera can follow
    GameObject[] followObjects;

    // Current object that the camera is following
    carFollowObject followObjectIndex;

    // Lerp speed of the camera rotation
    public float thirdLerpSpeedTransform = 10f;
    public float thirdLerpSpeedRotation = 10f;
    public float hoodLerpSpeedTransform = 10f;
    public float hoodLerpSpeedRotation = 10f;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        SetNextFollowObject();
        SetNextFollowCar();
    }

    void LateUpdate()
    {
        UpdateTransform();
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
        {
            gameObject.SetActive(false);
            menuCamera.SetActive(true);
            return;
        }

        if (Vector3.Distance(transform.position, followObjects[(int)followObjectIndex].transform.position) > 2)
        {
            transform.position = followObjects[(int)followObjectIndex].transform.position;
            transform.rotation = followObjects[(int)followObjectIndex].transform.rotation;
            return;
        }

        if (followObjectIndex == carFollowObject.thirdPerson)
        {
            // Follow exactly the position
            transform.position = Vector3.Lerp(transform.position, followObjects[(int)followObjectIndex].transform.position, (currentFollowCar.maxSpeed * 0.4f + Mathf.Sqrt(currentFollowCar.velocity.magnitude)) * 1f / GA_Parameters.fps);

            // Lerp towards rotation to avoid stuttering
            transform.rotation = Quaternion.Lerp(transform.rotation, followObjects[(int)followObjectIndex].transform.rotation, (currentFollowCar.turnSpeed * 0.1f) * 1f / GA_Parameters.fps);
        }

        else if (followObjectIndex == carFollowObject.hood)
        {
            // Follow exactly the position
            transform.position = followObjects[(int)followObjectIndex].transform.position;

            // Lerp towards rotation to avoid stuttering
            transform.rotation = Quaternion.Lerp(transform.rotation, followObjects[(int)followObjectIndex].transform.rotation, currentFollowCar.turnSpeed * 1f / GA_Parameters.fps);
        }

        else
        {
            transform.position = followObjects[(int)followObjectIndex].transform.position;
            transform.rotation = Quaternion.Euler(90, 0, 0);
        }
    }

    // Method that allows to set another car to follow
    public void SetFollowCars(List<CarController> carControllers)
    {
        gameObject.SetActive(true);
        menuCamera.SetActive(false);

        if (carControllers == null)
            return;

        followCars = carControllers;
        currentFollowCarNum = 0;
        currentFollowCar = carControllers[0];
        followObjects = currentFollowCar.carFollowObjects;
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

    // Method that check is V is pressed and if so changes the current car that is being followed
    void SetNextFollowCar()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (currentFollowCarNum + 1 < followCars.Count)
                currentFollowCarNum++;
            else
                currentFollowCarNum = 0;

            currentFollowCar = followCars[currentFollowCarNum];
            followObjects = currentFollowCar.carFollowObjects;
        }
    }
}
