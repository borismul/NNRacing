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
    public LayerMask groundMask;
    // Object that the camera can follow
    public GameObject[] carFollowObjects;
    public GameObject[] wheels;
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
    public CarTrackController trackManager;

    // Player that controls the car
    public HumanPlayer humanPlayer;
    public AIPlayer aIPlayer;

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
        trackManager = GetComponent<CarTrackController>();
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

    // The inputs from the player
    void GetInputs()
    {
        // accelerate
        if (Input.GetKey(humanPlayer.controls[0]))
            acc = accSpeed;

        if (Input.GetKey(humanPlayer.controls[1]))
            acc = breakSpeed;

        if (Input.GetKey(humanPlayer.controls[2]))
            turn = -turnSpeed;

        if (Input.GetKey(humanPlayer.controls[3]))
            turn = turnSpeed;

    }

    // Method that updates the car postition and fitness. If a something happens that should stop the simulation,
    // false is returned, else true.
    public bool UpdateCar(float deltaTime, bool stopAtCrash)
    {
        // If the current neural network is misformed stop simulation
        if (aIPlayer != null && !CalculateNetworkOutput())
            return false;

        if (aIPlayer == null)
            GetInputs();

        // Move the car
        Move(deltaTime);

        // If the car has a collision
        if (OnGrass() == 4 || ((velocity.x == 0 && velocity.y == 0) && aIPlayer != null))
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
        accVector = acc * transform.forward - (velocity.magnitude * velocity.magnitude) * transform.forward * (accSpeed - 3) / (maxSpeed * maxSpeed) - 3 * transform.forward - OnGrass() * 10 * transform.forward;

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

    public int OnGrass()
    {
        int wheelsOnGrass = 0;
        RaycastHit hit;
        for (int i = 0; i < wheels.Length; i++)
        {
            Physics.Raycast(wheels[i].transform.position, Vector3.down, out hit, 2f, groundMask);
            Vector2 textureCoord = hit.textureCoord;


            Texture2D tex = (Texture2D)hit.collider.gameObject.GetComponent<Renderer>().material.GetTexture("_MainTex");
            Color pixCol = tex.GetPixel((int)(textureCoord.x * tex.width), (int)(textureCoord.y * tex.height));

            if (pixCol.r - Color.green.r < 0.2f && pixCol.g - Color.green.g < 0.2f && pixCol.b - Color.green.b < 0.2f)
                wheelsOnGrass++;
        }
        return wheelsOnGrass;
    }

    // Method that resets the car to the start of the track
    public void Reset()
    {
        fitnessTracker.Reset();
        transform.position = trackManager.CurrentPosition();
        transform.rotation = trackManager.CurrentRotation();
        curRotAngle = transform.rotation.eulerAngles.y;
        prevRotAngle = transform.rotation.eulerAngles.y;
        velocity = Vector3.zero;
        acc = 0;
        turn = 0;
        trackSphere.transform.position = transform.position;
        trackSphere.GetComponent<TrailRenderer>().Clear();
    }

    // Method that gets all inputs for the neural network and then calculates the output of the network
    public bool CalculateNetworkOutput()
    {
        int inputs = aIPlayer.network.inputs;

        // Create a new list of inputs
        input.Clear();

        visionPoints = inputs - 1;
        // Rotate around the car and cast rays to see how far each wall is at that rotation
        for (int i = 0; i < visionPoints; i++)
        {
            // Cosine space the angles of the vectors at which a wall distance is measured
            angle = (1 - Mathf.Cos(i * Mathf.PI / visionPoints)) * Mathf.PI;

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
        output = aIPlayer.network.Update(input);

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

    // Getters for the fitnessTracker and trackManager
    public FitnessTracker GetFitnessTracker()
    {
        return fitnessTracker;
    }

    // Methods to set the players 
    public void SetHumanPlayer(HumanPlayer humanPlayer)
    {
        this.humanPlayer = humanPlayer;
        aIPlayer = null;
        isActive = true;
    }

    public void SetAiPlayer(AIPlayer aiPlayer)
    {
        this.aIPlayer = aiPlayer;
        humanPlayer = null;
        isActive = true;
    }
}
