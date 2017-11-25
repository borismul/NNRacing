using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public enum ViewType { MenuView, TopView, AICarView, HumanCarView }

    public static RaceManager raceManager;

    CameraController cameraController;

    List<string> tracks = new List<string>();
    List<HumanPlayer> humanPlayers = new List<HumanPlayer>();
    List<AIPlayer> aiPlayers = new List<AIPlayer>();
    List<CarController> cars = new List<CarController>();

    public GameObject carPrefab;
    public GameObject trackPrefab;

    int currentTrackNum;

    GameObject currentTrackObj;

    // variables to measure the time, used to scale the time and keep the framerate constant
    float realTimePassed = 0;
    float startTime = 0;
    float ingameTimePassed = 0;
    float buildupTime = 0;
    float curTime;
    int countSinceFrame = 0;

    // The speed of the simulation
    float curSpeed = 0;

    // boolean that can be set to stop the current race if all cars have crashed
    bool stopCurRace;

    // The current way the training or race is viewed
    ViewType curViewType = ViewType.MenuView;

    void Awake()
    {
        raceManager = this;
    }

    void Start()
    {
        cameraController = CameraController.instance;
    }

    public void SetTracks(List<Track> tracks)
    {
        //this.tracks = tracks;
    }

    void NextTrack()
    {
        currentTrackNum += 1;
    }

    public void AddHumanPlayer(string name, KeyCode[] controls)
    {
        humanPlayers.Add(new HumanPlayer(name, controls));
    }

    public void RemoveHumanPlayer(string name)
    {
        for (int i = 0; i < humanPlayers.Count; i++)
        {
            if (name == humanPlayers[i].name)
            {
                humanPlayers.RemoveAt(i);
                return;
            }
        }
    }

    public void RemoveAIPlayer(string name)
    {
        for (int i = 0; i < aiPlayers.Count; i++)
        {
            if (name == aiPlayers[i].name)
            {
                aiPlayers.RemoveAt(i);
                return;
            }
        }
    }

    public void AddAIPlayer(string name, NeuralNetwork network)
    {
        aiPlayers.Add(new AIPlayer(name, network));
    }

    public void ResetPlayers()
    {
        humanPlayers.Clear();
        aiPlayers.Clear();
    }

    public IEnumerator StartRace()
    {
        CreateCars();
        AddPlayers();
        SetCarsReady();
        yield return StartCoroutine(Race());
    }

    void CreateTrack()
    {
        if (currentTrackObj != null)
            Destroy(currentTrackObj);

        currentTrackObj = Instantiate(trackPrefab);
    }

    void CreateCars()
    {
        if (humanPlayers.Count + aiPlayers.Count > cars.Count)
        {
            while (humanPlayers.Count + aiPlayers.Count > cars.Count)
            {
                cars.Add(Instantiate(carPrefab).GetComponent<CarController>());
            }
        }
    }

    void SetCarsReady()
    {
        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].isActive)
            {
                cars[i].trackManager.SetTrack(TrackManager.trackManager.GetTrack());
                cars[i].Reset();
            }
        }
    }

    void AddPlayers()
    {
        int count = 0;

        foreach (HumanPlayer humanPlayer in humanPlayers)
        {
            cars[count].gameObject.SetActive(true);
            cars[count].SetHumanPlayer(humanPlayer);
            count++;
        }

        foreach (AIPlayer aiPlayer in aiPlayers)
        {
            cars[count].gameObject.SetActive(true);
            cars[count].SetAiPlayer(aiPlayer);
            count++;
        }

        for (int i = count; i < cars.Count; i++)
        {
            cars[i].isActive = false;
            cars[i].gameObject.SetActive(false);
        }
    }

    public void AdjustViewSettings(ViewType view)
    {
        if (view == ViewType.MenuView)
        {
            GA_Parameters.updateRate = 0;
            cameraController.SetFollowCars(null);
        }
        else if (view == ViewType.HumanCarView || view == ViewType.AICarView)
        {
            GA_Parameters.updateRate = 1;
            List<CarController> carControllers = new List<CarController>();

            if (view == ViewType.HumanCarView)
            {
                for(int i = 0; i < cars.Count; i++)
                {
                    if (cars[i].isActiveAndEnabled && cars[i].humanPlayer != null)
                        carControllers.Add(cars[i]);
                }
                cameraController.SetFollowCars(carControllers);
            }
            else
            {
                for (int i = 0; i < cars.Count; i++)
                {
                    if (cars[i].isActiveAndEnabled && cars[i].aIPlayer != null)
                        carControllers.Add(cars[i]);
                }
                cameraController.SetFollowCars(carControllers);
            }
        }
    }

    public List<float> GetFitnesses()
    {
        List<float> fitnesses = new List<float>();

        for (int i = 0; i < cars.Count; i++)
        {
            if(cars[i].isActiveAndEnabled)
                fitnesses.Add(cars[i].GetFitnessTracker().GetFitness());
        }

        return fitnesses;

    }

    // Method that starts a race
    IEnumerator Race()
    {
        ResetRaceParameters();

        // For each second
        for (int k = 0; k < GA_Parameters.simulationTime; k++)
        {
            // For each frame in that second
            for (int l = 0; l < GA_Parameters.fps; l++)
            {
                countSinceFrame++;
                ingameTimePassed += (1f / GA_Parameters.fps);

                // Show a frame if the framerate drops under the training framerate
                if (Time.realtimeSinceStartup - curTime > 1f / GA_Parameters.fps)
                {
                    yield return null;

                    // Calculate how fast the simulation currently is
                    curSpeed = countSinceFrame / (GA_Parameters.fps * (Time.realtimeSinceStartup - curTime));

                    // reset these values
                    countSinceFrame = 0;
                    curTime = Time.realtimeSinceStartup;
                }

                // this is kept true if all cars are crashed
                stopCurRace = true;

                // for each car in the group
                for (int carControllerindex = 0; carControllerindex < cars.Count; carControllerindex++)
                {
                    // Get the cars controller
                    CarController currentCarController = cars[carControllerindex];

                    // Check if its active and if car is still racing, if not continue
                    if (!currentCarController.isActive)
                        continue;

                    // Update the car to its next position. If it returns a false it starts the next track (if possible)
                    if (!currentCarController.UpdateCar(1f / GA_Parameters.fps, GA_Parameters.stopAtCrash))
                        currentCarController.isActive = false;

                    // if a car is still racing don't end the race
                    stopCurRace = false;
                }

                // If simulating to fast a pause is set up here
                Coroutine limitRoutine = StartCoroutine(LimitPlaybackSpeed());

                if (limitRoutine != null)
                    yield return limitRoutine;

                if (stopCurRace)
                    yield break;

            }
        }
    }

    // Method that resets all race parameters
    void ResetRaceParameters()
    {
        realTimePassed = 0;
        startTime = Time.realtimeSinceStartup;
        realTimePassed = Time.realtimeSinceStartup;
        curTime = Time.realtimeSinceStartup;
        ingameTimePassed = 0;
        curSpeed = 0;
        countSinceFrame = 0;
        buildupTime = 0;
        currentTrackNum = 0;
    }

    // Method that limits the playbackspeed if cpu power is forcing it to go beyond the desired
    IEnumerator LimitPlaybackSpeed()
    {
        //GA_Parameters.updateRate = 1;
        if (GA_Parameters.updateRate == 0)
            yield break;

        // Update the real time that has passed 
        realTimePassed = Time.realtimeSinceStartup - startTime;

        // Check whether the simulation is going to fast, if so wait as long as needed for the desired simulation speed
        if ((ingameTimePassed + buildupTime) / GA_Parameters.updateRate - realTimePassed > 0)
        {
            yield return new WaitForSeconds((ingameTimePassed + buildupTime) / GA_Parameters.updateRate - realTimePassed);

            // Simulation is going as fast as set so set the curSpeed to the desired speed
            curSpeed = GA_Parameters.updateRate;

            // Set this variable to the current time, it is used see how long ago the last frame was shown
            curTime = Time.realtimeSinceStartup;

            // Set the fps counter to 0 again
            countSinceFrame = 0;

        }
        // If it is going slower than desired store a second time variable that measures how much time we are behind on desired.
        // This is later added to the ingame time passed as otherwise the simulation might get faster than the set speed if enough cars have died.
        else
        {
            buildupTime -= (ingameTimePassed + buildupTime) / GA_Parameters.updateRate - realTimePassed;
        }

    }

    public void SetUpdateRate(float updateRate, bool watch)
    {
        if (GA_Parameters.updateRate == 0)
            GA_Parameters.updateRate = 1;

        realTimePassed = Time.realtimeSinceStartup - startTime;

        buildupTime = realTimePassed * updateRate - ingameTimePassed;

        GA_Parameters.updateRate = updateRate;

    }

    public float GetTimeLeft()
    {
        return GA_Parameters.simulationTime - ingameTimePassed;
    }

    public float GetCurSpeed()
    {
        return curSpeed;
    }
}
