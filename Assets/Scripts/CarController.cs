using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Threading;

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
    Vector3[] wheelsRelativePosition = new Vector3[4];
    public ParticleSystem[] wheelPs;
    public GameObject carFollowCameraPrefab;
    public GameObject trackSphere;
    public Material trackSphereMat;

    public CameraController carFollowCamera;

    public static List<CameraController> cameras = new List<CameraController>();

    // Car variables that determine the movement of the car
    float acc;
    float turnAcc;
    public float turn;
    float curRotAngle;
    float prevRotAngle;
    float jitterPenalty;

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
    RaycastHit hit;
    Vector3 direction;
    float curdist;
    int visionPoints;
    List<float> output;
    public TrailRenderer trailren;
    Texture2D groundTexture;

    // Stuff that is needed to see how well a car did after finishing
    public bool finished;
    float totalTime;
    float finishedPosition;


    int skip = 0;
    List<Vector3> positions = new List<Vector3>();
    List<Quaternion> rotations = new List<Quaternion>();

    Vector3 position;
    Quaternion rotation;

    // Thread lockers
    object positionsLock = new object();
    object rotationsLock = new object();

    public bool threaded = false;

    List<Vector3> linePositions = new List<Vector3>();

    public static int doneCounter = 0;
    public static object doneLocker = new object();

    void Awake()
    {
        // Get used components
        col = GetComponent<BoxCollider>();
        fitnessTracker = GetComponent<FitnessTracker>();
        trackManager = GetComponent<CarTrackController>();

        for (int i = 0; i < 4; i++)
            wheelsRelativePosition[i] = wheelPs[i].transform.position - transform.position;

        trackSphereMat = trackSphere.GetComponent<Renderer>().material;

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

        else if (Input.GetKey(humanPlayer.controls[1]))
            acc = -breakSpeed;

        else
            acc = 0;

        if (Input.GetKey(humanPlayer.controls[2]))
            turn = -turnSpeed;

        else if (Input.GetKey(humanPlayer.controls[3]))
            turn = turnSpeed;

        else
            turn = 0;
    }

    // Method that updates the car postition and fitness. If a something happens that should stop the simulation,
    // false is returned, else true.
    public bool UpdateCar(float deltaTime, bool doNotStopAtCrash, float maxTime)
    {

        totalTime += deltaTime;

        if (totalTime > maxTime)
            return false;

        // If the current neural network is misformed stop simulation
        if (aIPlayer != null && !CalculateNetworkOutput() && !doNotStopAtCrash)
            return false;

        if (aIPlayer == null)
            GetInputs();

        // Move the car
        Move(deltaTime);



        // If the car has a collision
        if (OnGrass() == 4 || ((velocity.x == 0 && velocity.y == 0) && aIPlayer != null))
        {
            // If the car has to stop at a crash stop the simulation
            if (threaded && !doNotStopAtCrash)
            {
                fitnessTracker.UpdateFitness(deltaTime, true, position);
                //trackSphere.GetComponent<Renderer>().material.color = Color.red;
                //trackSphere.transform.position = trackSphere.transform.position - new Vector3(0, 1, 0);
                acc = 0;
                return false;
            }

            // Else reset the car and add a crash to the fitnessTracker
            Reset(true);
            fitnessTracker.AddCrash();
        }

        // Update the fitness tracker, checks if the number of laps has been completed and if so, stops the simulation
        if (!fitnessTracker.UpdateFitness(deltaTime, !doNotStopAtCrash, position))
        {
            acc = 0;
            return false;
        }
        // If everything went well let simulation continue
        return true;
    }

    // Method that moves and rotates the car to its new position
    void Move(float deltaTime)
    {
        // Acceleration caused by forces
        float engine = acc;
        float airDrag = (velocity.magnitude * velocity.magnitude) * (accSpeed - 3) / (maxSpeed * maxSpeed);
        float wheelDrag = 3;
        float grassDrag = OnGrass() * .05f * (velocity.magnitude * velocity.magnitude);

        // Determine an accelation vector
        accVector = (engine - airDrag - wheelDrag - grassDrag) * (rotation * Vector3.forward);

        // Add it to the velocity
        velocity += accVector * deltaTime;

        // If the velocity reached its maximum speed, reduce it to the maximum velocity
        if (velocity.magnitude > maxSpeed)
        {
            velocity /= (velocity.magnitude + 0.001f);
            velocity *= maxSpeed;
        }

        if (Vector3.Dot(velocity, rotation * Vector3.forward) < 0f)
            velocity = Vector3.zero;

        turnAcc = (turn + turnAcc*4) / 5;

        // Determine the rotation angle of the car
        curRotAngle += turnAcc * deltaTime;

        if (curRotAngle == 0)
            curRotAngle = 0.001f;

        Quaternion rot = Quaternion.Euler(0, curRotAngle, 0);

        // Determine the difference in rotation since last update
        float rotDiff = curRotAngle - prevRotAngle;
        Quaternion rotVector = Quaternion.Euler(0, rotDiff, 0);

        // Rotate the velocity vector by this amount
        velocity = rotVector * velocity;

        if (aIPlayer != null && (velocity.x == 0 && velocity.y == 0) && threaded == false)
            velocity = transform.forward * maxSpeed / 10;

        // Set the new position and rotation
        position += velocity * deltaTime;
        rotation = rot;

        lock(positionsLock)
            positions.Add(position);

        lock(rotationsLock)
            rotations.Add(rotation);

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
        if (Physics.Raycast(transform.position, TR - transform.position, out hit, (TR - transform.position).magnitude/6, mask))
            return true;

        Vector3 TL = transform.TransformPoint(new Vector3(size.x / 2, col.center.y, -size.z / 2));

        if (Physics.Raycast(transform.position, TL - transform.position, out hit, (TL - transform.position).magnitude/6, mask))
            return true;

        Vector3 BL = transform.TransformPoint(new Vector3(-size.x / 2, col.center.y, -size.z / 2));

        if (Physics.Raycast(transform.position, BL - transform.position, out hit, (BL - transform.position).magnitude/6, mask))
            return true;

        Vector3 BR = transform.TransformPoint(new Vector3(-size.x / 2, col.center.y, size.z / 2));

        if (Physics.Raycast(transform.position, BR - transform.position, out hit, (BR - transform.position).magnitude/6, mask))
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
        for (int i = 0; i < wheels.Length; i++)
        {
            if (TrackManager.trackManager.HasGrass(position + rotation *  wheelsRelativePosition[i]))
            {
                wheelsOnGrass++;

                if (!threaded & GA_Parameters.updateRate == 1 && acc != 0)
                {
                    var ps = wheelPs[i].emission;

                    ps.rateOverTime = 100;

                }
            }
            else
            {
                if (!threaded && GA_Parameters.updateRate == 1)
                {
                    var ps = wheelPs[i].emission;
                    ps.rateOverTime = 0;
                }
            }
        }
        return wheelsOnGrass;
    }

    // Method that gets all inputs for the neural network and then calculates the output of the network
    public bool CalculateNetworkOutput()
    {
        InputWithoutRaycast();

        return SetOutput(input);
    }

    void InputWithoutRaycast()
    {
        int inputs = aIPlayer.network.inputs;

        // Create a new list of inputs
        input.Clear();

        visionPoints = inputs - 1;
        // Rotate around the car and cast rays to see how far each wall is at that rotation
        for (int i = 0; i < visionPoints; i++)
        {
            //Space the angles closer and closer as they point point more and more forward
            float angle;
            if (i - (float)visionPoints / 2 < 0)
                angle = Mathf.PI / 1.5f / (i + 1) - Mathf.PI / 1.5f / ((float)visionPoints / 2 + 1);
            else
                angle = -Mathf.PI / 1.5f / (i + 1 - (float)visionPoints / 2) + Mathf.PI / 1.5f / ((float)visionPoints / 2 + 1);

            curdist = TrackManager.WallDistance(position, rotation, angle * Mathf.Rad2Deg);

            // Add as input
            input.Add(((curdist - 10) / 25));
        }

        // add the velocity as input
        input.Add((velocity.magnitude - 15) / 5);
    }

    // Get the output of the neural network and set it to the inputs for the cars
    public bool SetOutput(List<float> input)
    {
        jitterPenalty -= jitterPenalty * 0.1f * 1f/GA_Parameters.fps;
        output = aIPlayer.network.Update(input);
        if (output == null || output.Count < 6)
            return false;

        float maxOutput = 0;
        int index = 0;
        for (int i = 0; i < 3; i++)
        {
            if (output[i] > maxOutput)
            {
                index = i;
                maxOutput = output[i];
            }
        }

        for (int i = 0; i < 3; i++)
        {
            if (index != i)
                output[i] = 0;
            else
                output[i] = 1;
        }
        maxOutput = 0;
        index = 0;
        for (int i = 3; i < 6; i++)
        {
            if (output[i] > maxOutput)
            {
                index = i;
                maxOutput = output[i];
            }
        }
        for (int i = 3; i < 6; i++)
        {
            if (index != i)
                output[i] = 0;
            else
                output[i] = 1;
        }
        float newTurn = (output[4] - output[5]) * turnSpeed;
        acc = output[0] * accSpeed - output[1] * breakSpeed;

        if (Mathf.Abs(newTurn - turn) > 1.5f * turnSpeed)
            jitterPenalty += 0.5f;

        turn = newTurn;

        if (jitterPenalty > 2f)
            return false;

        //if (output[0] > 0.5f)
        //    acc = (output[0] - 0.5f) * accSpeed;
        //else
        //    acc = (output[0] - 0.5f) * breakSpeed;

        //turn = (output[1] - 0.5f) * turnSpeed;


        return true;
    }

    // Method that resets the car to the start of the track (used by a thread)
    public void ThreadReset(bool softReset)
    {
        lock (positionsLock)
        {
            if(!softReset)
                positions.Clear();

            positions.Add(trackManager.CurrentPosition());
        }
        lock (rotationsLock)
        {
            if(!softReset)
                rotations.Clear();

            rotations.Add(trackManager.CurrentRotation());
        }

        curRotAngle = rotations[0].eulerAngles.y;
        prevRotAngle = rotations[0].eulerAngles.y;

        velocity = Vector3.zero;
        acc = 0;
        turn = 0;
        turnAcc = 0;

        finished = false;
        jitterPenalty = 0;

        trailren.Clear();
        
    }

    // Method that resets the car to the start of the track (used by the main unity thread)
    public void Reset(bool softReset)
    {
        if (!softReset)
            fitnessTracker.Reset();

        transform.position = trackManager.CurrentPosition();
        transform.rotation = trackManager.CurrentRotation();

        position = transform.position;
        rotation = transform.rotation;

        trailren.Clear();
        
        trackSphere.transform.position = transform.position;


        for (int i = 0; i < wheelPs.Length; i++)
        {
            wheelPs[i].Clear();
            var ps = wheelPs[i].emission;
            ps.rateOverTime = 0;
        }

        curRotAngle = transform.rotation.eulerAngles.y;
        prevRotAngle = transform.rotation.eulerAngles.y;

        velocity = Vector3.zero;
        acc = 0;
        turn = 0;
        turnAcc = 0;

        finished = false;
        jitterPenalty = 0;
        totalTime = 0;
    }

    public bool UpdateCar()
    {
        lock (positionsLock)
        {
            lock (rotationsLock)
            {
                if (rotations.Count > 0 && positions.Count > 0)
                {
                    transform.position = positions[0];
                    positions.RemoveAt(0);

                    transform.rotation = rotations[0];
                    rotations.RemoveAt(0);

                    return true;
                }
                else
                {
                    if (isActive)
                    {
                        return false;
                    }
                    else return true;
                }
            }
        }
    }

    public bool IsDone()
    {
        if (positions.Count == 0 && !isActive)
            return true;

        return false;
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

    public int GetCurrentLap()
    {
        return Mathf.Min((int)(fitnessTracker.laps + 1), GA_Parameters.laps);
    }

    public float GetCurrentVelocity()
    {
        return velocity.magnitude*2;
    }

    public int GetPosition()
    {
        List<CarController> cars = RaceManager.raceManager.GetCurrentCompetingCars();
        int position = 1;
        for(int i = 0; i < cars.Count; i++)
        {
            if (cars[i] != this && fitnessTracker.distance < cars[i].fitnessTracker.distance && !finished)
                position++;
            else if (finished && cars[i].finished)
                if (fitnessTracker.time > cars[i].fitnessTracker.time)
                    position++;
        }

        return position;
    }

    public int GetCompetitors()
    {
        return RaceManager.raceManager.GetCurrentCompetingCars().Count;
    }

    public float GetFinishTime()
    {
        return fitnessTracker.GetFinishTime();
    }

    public void SetFinished()
    {
        finished = true;
    }
}
