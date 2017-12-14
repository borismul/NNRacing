using UnityEngine;
using System.Collections.Generic;


public class CameraController : MonoBehaviour
{

    public static CameraController instance;

    public GameObject menuCamera;

    public RacingCanvasController raceCanvas;

    enum carFollowObject { thirdPerson, hood, top }

    // List of cars that can be followed by the camera
    List<CarController> followCars;

    // Car that is being followed
    public static CarController currentFollowCar;

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
        GetComponent<Camera>().farClipPlane = SceneryActivator.cutoffRangesFarnew[QualitySettings.GetQualityLevel()];
    }

    void Update()
    {
        SetNextFollowObject();
        SetNextFollowCar(false);
    }

    void LateUpdate()
    {

    }

    void OnEnable()
    {
        if (followObjects == null)
            return;

        transform.position = followObjects[(int)followObjectIndex].transform.position;
        transform.rotation = followObjects[(int)followObjectIndex].transform.rotation;
    }

    public Vector3 Damp(Vector3 source, Vector3 target, float smoothing, float dt)
    {
        return Vector3.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    }

    public Quaternion Damp(Quaternion source, Quaternion target, float smoothing, float dt)
    {
        return Quaternion.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
    }

    public Vector3 DampExp(Vector3 a, Vector3 b, float lambda, float dt)
    {
        return Vector3.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }

    public Quaternion DampExp(Quaternion a, Quaternion b, float lambda, float dt)
    {
        return Quaternion.Lerp(a, b, 1 - Mathf.Exp(-lambda * dt));
    }

    // Method that lets the camera follow the current carFollowObject
    public void UpdateTransform(float dTime = 1f/60)
    {
        if (currentFollowCar != null &&currentFollowCar.finished)
            SetNextFollowCar(true);

        if (RaceManager.raceManager.curViewType == RaceManager.ViewType.MenuView)
            followCars = null;
        if (followCars == null)
        {
            gameObject.SetActive(false);
            menuCamera.SetActive(true);
            raceCanvas.gameObject.SetActive(false);

            return;
        }

        if (currentFollowCar.finished)
        {
            return;
        }

        raceCanvas.gameObject.SetActive(true);
        raceCanvas.UpdateCanvas(currentFollowCar, RaceManager.raceManager.GetTotalTime());

        if (Vector3.Distance(transform.position, followObjects[(int)followObjectIndex].transform.position) > 5)
        {
            transform.position = followObjects[(int)followObjectIndex].transform.position;
            transform.rotation = followObjects[(int)followObjectIndex].transform.rotation;
            return;
        }

        if (followObjectIndex == carFollowObject.thirdPerson)
        {
            // Follow exactly the position
            transform.position = DampExp(transform.position, followObjects[(int)followObjectIndex].transform.position, 20f, dTime);

            //transform.position = followObjects[(int)followObjectIndex].transform.position;
            // Lerp towards rotation to avoid stuttering
            transform.rotation = Damp(transform.rotation, followObjects[(int)followObjectIndex].transform.rotation, 0.001f, dTime);
            //transform.rotation = followObjects[(int)followObjectIndex].transform.rotation;
        }

        else if (followObjectIndex == carFollowObject.hood)
        {
            // Follow exactly the position
            transform.position = followObjects[(int)followObjectIndex].transform.position;

            // Lerp towards rotation to avoid stuttering
            transform.rotation = Quaternion.Lerp(transform.rotation, followObjects[(int)followObjectIndex].transform.rotation, currentFollowCar.turnSpeed * dTime);
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
        if (carControllers == null)
        {
            currentFollowCar = null;
            return;
        }

        gameObject.SetActive(true);
        menuCamera.SetActive(false);
        followCars = carControllers;
        currentFollowCarNum = 0;
        currentFollowCar = carControllers[0];
        currentFollowCar.followCar = true;
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
    void SetNextFollowCar(bool forceNext)
    {
        if ((Input.GetKeyDown(KeyCode.V) && currentFollowCar.humanPlayer == null) || forceNext)
        {
            if (currentFollowCarNum + 1 < followCars.Count)
                currentFollowCarNum++;
            else
                currentFollowCarNum = 0;

            currentFollowCar.followCar = false;

            currentFollowCar = followCars[currentFollowCarNum];
            currentFollowCar.followCar = true;
            followObjects = currentFollowCar.carFollowObjects;
        }
    }
}
