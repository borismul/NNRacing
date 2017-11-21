using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CarController : MonoBehaviour
{
    // Variables of the car that are set by user in settings menu
    public float accSpeed;
    public float breakSpeed;
    public float maxSpeed;
    public float turnSpeed;

    // Mask that is used to detect collisions with walls
    public LayerMask mask;

    // Object that the camera can follow
    public GameObject[] carFollowObjects;

    public GameObject carFollowCameraPrefab;
    public GameObject trackSphere;

    public CameraController carFollowCamera;

    public static List<CameraController> cameras = new List<CameraController>();

    // Car variables that determine the movement of the car
    float acc;
    public float turn;
    float curRotAngle;
    float prevRotAngle;

    public Vector3 velocity;

    Vector3 accVector;

    // Colider of the car
    BoxCollider col;

    // Fitness and track managers of the car
    FitnessTracker fitnessTracker;
    TrackManager trackManager;

    // The neural network that controls the car
    NeuralNetwork network;

    public bool isActive;

    // Stuff declared so garbage collector has less overhead
    List<float> input = new List<float>();
    float angle;
    RaycastHit hit;
    Vector3 direction;
    float curdist;
    int visionPoints;
    List<float> output;
    public TrailRenderer trailren;

    void Awake()
    {
        // Get used components
        col = GetComponent<BoxCollider>();
        fitnessTracker = GetComponent<FitnessTracker>();
        trackManager = GetComponent<TrackManager>();

        carFollowCamera = Instantiate(carFollowCameraPrefab).GetComponent<CameraController>();
        carFollowCamera.SetFollowCar(this);
        cameras.Add(carFollowCamera);
    }

    void Start()
    {
        // Set important car parameters
        accSpeed = GA_Parameters.accSpeed;
        breakSpeed = GA_Parameters.breakSpeed;
        turnSpeed = GA_Parameters.turnSpeed;
        maxSpeed = GA_Parameters.maxSpeed;

        if (trailren != null)
        {
            trailren.sortingLayerName = ("Tracker");
            trailren.sortingOrder = 11;
        }

        Color col = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        trailren.material.color = col;
        trailren.gameObject.GetComponent<Renderer>().material.color = col;
    }

    void ManualController()
    {
        // If this has a network, the car is not manually controller so return
        if (network != null)
            return;

        // Get the keyboard inputs
        GetInputs();

        // Update the car. If something went wrong stop simulation
        if (!UpdateCar(Time.deltaTime, GA_Parameters.stopAtCrash))
        {
            trackManager.SelectNextTrack();
        }
    }

    void GetInputs()
    {
        // accelerate
        acc = Input.GetAxisRaw("Vertical");

        if (acc > 0)
            acc = accSpeed;
        else if (acc < 0.05f && acc > -0.05f)
            acc = 0;
        else
            acc = -breakSpeed;

        // turn
        turn = Input.GetAxisRaw("Horizontal") * turnSpeed;
    }

    // Method that updates the car postition and fitness. If a something happens that should stop the simulation,
    // false is returned, else true.
    public bool UpdateCar(float deltaTime, bool stopAtCrash)
    {
        // If the current neural network is misformed stop simulation
        if (network != null && !CalculateNetworkOutput())
            return false;

        if (network == null)
            GetInputs();
        
        // Move the car
        Move(deltaTime);

        // If the car has a collision
        if (HasCollision() || ((velocity.x == 0 && velocity.y == 0) && network != null))
        {
            // If the car has to stop at a crash stop the simulation
            if (stopAtCrash)
            {
                fitnessTracker.UpdateFitness(deltaTime, true);
                //trackSphere.GetComponent<Renderer>().material.color = Color.red;
                trackSphere.transform.position = trackSphere.transform.position - new Vector3(0, 1, 0);
                return false;
            }

            // Else reset the car and add a crash to the fitnessTracker
            Reset();
            fitnessTracker.AddCrash();
        }
        
        // Update the fitness tracker, checks if the number of laps has been completed and if so, stops the simulation
        if (!fitnessTracker.UpdateFitness(deltaTime, stopAtCrash))
            return false;

        // If everything went well let simulation continue
        return true;
    }

    // Method that moves and rotates the car to its new position
    void Move(float deltaTime)
    {
        // Determine an accelation vector
        accVector = acc * transform.forward - (velocity.magnitude * velocity.magnitude) * transform.forward * (accSpeed - 3) / (maxSpeed * maxSpeed) - 3 * transform.forward;

        // Add it to the velocity
        velocity += accVector * deltaTime;

        // If the velocity reached its maximum speed, reduce it to the maximum velocity
        if (velocity.magnitude > maxSpeed)
        {
            velocity /= (velocity.magnitude + 0.001f);
            velocity *= maxSpeed;
        }

        if (Vector3.Dot(velocity, transform.forward) < 0f)
            velocity = Vector3.zero;

        // Determine the rotation angle of the car
        curRotAngle += turn * deltaTime;
        Quaternion rot = Quaternion.Euler(0, curRotAngle, 0);
        
        // Determine the difference in rotation since last update
        float rotDiff = curRotAngle - prevRotAngle;
        Quaternion rotVector = Quaternion.Euler(0, rotDiff, 0);

        // Rotate the velocity vector by this amount
        velocity = rotVector * velocity;

        // Set the new position and rotation
        transform.position += velocity * deltaTime;
        transform.rotation = rot;

        // Store the current rotation angle
        prevRotAngle = curRotAngle;
    }

    // Method that checks if the car is colliding with something 
    // ### Check for going through walls ###
    public bool HasCollision()
    {
        Vector3 size = col.size * 1.05f;

        Vector3 TR = transform.TransformPoint(new Vector3(size.x / 2, col.center.y, size.z / 2));
        RaycastHit hit;
        if (Physics.Raycast(transform.position, TR - transform.position, out hit, (TR - transform.position).magnitude, mask))
            return true;

        Vector3 TL = transform.TransformPoint(new Vector3(size.x / 2, col.center.y, -size.z / 2));

        if (Physics.Raycast(transform.position, TL - transform.position, out hit, (TL - transform.position).magnitude, mask))
            return true;

        Vector3 BL = transform.TransformPoint(new Vector3(-size.x / 2, col.center.y, -size.z / 2));

        if (Physics.Raycast(transform.position, BL - transform.position, out hit, (BL - transform.position).magnitude, mask))
            return true;

        Vector3 BR = transform.TransformPoint(new Vector3(-size.x / 2, col.center.y, size.z / 2));

        if (Physics.Raycast(transform.position, BR - transform.position, out hit, (BR - transform.position).magnitude, mask))
            return true;

        //Debug.DrawRay(transform.position, TR - transform.position, Color.red, Mathf.Infinity);
        //Debug.DrawRay(transform.position, TL - transform.position, Color.red, Mathf.Infinity);
        //Debug.DrawRay(transform.position, BL - transform.position, Color.red, Mathf.Infinity);
        //Debug.DrawRay(transform.position, BR - transform.position, Color.red, Mathf.Infinity);

        return false;
    }

    // Method that resets the car to the current point in the track manager, rotated towards the next
    public void Reset()
    {
        transform.position = trackManager.currentTrack.CurrentPosition();
        transform.rotation = trackManager.currentTrack.CurrentRotation();
        curRotAngle = transform.rotation.eulerAngles.y;
        prevRotAngle = transform.rotation.eulerAngles.y;
        velocity = Vector3.zero;
        acc = 0;
        turn = 0;
    }

    // Method that gets all inputs for the neural network and then calculates the output of the network
    public bool CalculateNetworkOutput()
    {
        int inputs = network.inputs;

        // Create a new list of inputs
        input.Clear();

        visionPoints = inputs - 1;
        // Rotate around the car and cast rays to see how far each wall is at that rotation
        for (int i = 0; i < visionPoints; i++)
        {
            // Cosine space the angles of the vectors at which a wall distance is measured
            angle = (1 - Mathf.Cos(i * Mathf.PI/ visionPoints)) * Mathf.PI;
            
            // Create the direction by rotating the forward vector by angle
            direction = new Vector3(Mathf.Cos(angle) * transform.forward.x + Mathf.Sin(angle) * transform.forward.z, 0, -Mathf.Sin(angle) * transform.forward.x + Mathf.Cos(angle) * transform.forward.z);

            //Cast the ray
            if (Physics.Raycast(transform.position, direction, out hit, Mathf.Infinity, mask))
                curdist = hit.distance;
            else
                curdist = -1;

            //Debug.DrawRay(transform.position, direction * hit.distance, Color.red);
            // Add as input
            input.Add((curdist - 10) / 25);
        }

        // add the velocity as input
        input.Add((velocity.magnitude - 15) / 5);

        return SetOutput(input);
        
    }

    // Get the output of the neural network and set it to the inputs for the cars
    public bool SetOutput(List<float> input)
    { 
        output = network.Update(input);

        if (output == null || output.Count < 4)
            return false;

        for (int i = 0; i < output.Count; i++)
        {
            if (output[i] > 0.6f)
                output[i] = 1;
            else
                output[i] = 0;
        }
        
        acc = output[0] * accSpeed - output[1] * breakSpeed;
        turn = (output[2] - output[3]) * turnSpeed;

        //if (output[0] > 0.5f)
        //    acc = (output[0] - 0.5f) * accSpeed;
        //else
        //    acc = (output[0] - 0.5f) * breakSpeed;

        //turn = (output[1] - 0.5f) * turnSpeed;


        return true;
    }

    // Set a new network to this car, assumed is that you want to start a simulation so the fitness is set to 0
    public void SetNewNetwork(NeuralNetwork network, bool replay, bool stop)
    {
        this.network = network;
        fitnessTracker.Reset();
        carFollowCamera.gameObject.SetActive(false);

        if (stop)
        {
            isActive = false;
            network = null;
            trailren.Clear();
        }
        else
        {
            trackSphere.transform.position = transform.position;
            if (network == null)
            {
                carFollowCamera.gameObject.SetActive(true);
                CameraController.currentActiveMainCamera = carFollowCamera;
            }

            isActive = true;
        }
    }

    // Void that allows the car to stop moving
    public void StandStill()
    {
        acc = 0;
        velocity = Vector3.zero;
        turn = 0;
    }

    public bool HasNetwork()
    {
        return network != null;
    }

    public int GetCurrentTrack()
    {
        return trackManager.currentTrackIndex;
    }

    public NeuralNetwork GetNetwork()
    {
        return network;
    }

    // Getters for the fitnessTracker and trackManager
    public FitnessTracker GetFitnessTracker()
    {
        return fitnessTracker;
    }

    public TrackManager GetTrackManager()
    {
        return trackManager;
    }

    public void OnDestroy()
    {
        if(carFollowCamera!= null)
            Destroy(carFollowCamera.gameObject);
    }
}
