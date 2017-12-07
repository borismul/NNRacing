using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public enum ViewType { MenuView, TopView, AICarView, HumanCarView }

    public static RaceManager raceManager;

    CameraController cameraController;

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
    public ViewType curViewType = ViewType.MenuView;

    float currentFrame = 0;
    int currentCar = 0;

    object carLocker = new object();
    object frameLocker = new object();
    object[] carsLocker = new object[GA_Parameters.populationSize];
    object simLock = new object();

    bool curDone = false;

    bool[] carDone;

    int frameSkip = 0;
    int framesSkipped = 0;

    void Awake()
    {
        raceManager = this;
    }

    void Start()
    {
        cameraController = CameraController.instance;

        carsLocker = new object[GA_Parameters.populationSize];
        carDone = new bool[GA_Parameters.populationSize];

        for (int i = 0; i < carsLocker.Length; i++)
        {
            carsLocker[i] = new object();
            carDone[i] = false;
        }

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

    public IEnumerator StartRace(bool threaded, List<string> tracks, bool fancy)
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            UIController.instance.UpdateUI(i, tracks.Count);

            TrackManager.trackManager.trackName = tracks[i];
            TrackManager.trackManager.LoadTrack(fancy);

            CreateCars();

            AddPlayers();
            SetCarsReady(threaded, i == 0);
            AdjustViewSettings();

            yield return null;

            if (threaded)
            {
                Coroutine routine = StartCoroutine(ThreadedRace(humanPlayers.Count > 0));
                UIController.instance.activeRoutines.Add(routine);
                yield return routine;
            }
            else
            {
                Coroutine routine = StartCoroutine(Race(humanPlayers.Count > 0));
                UIController.instance.activeRoutines.Add(routine);
                yield return routine;
            }
        }
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

    void SetCarsReady(bool threaded, bool completeReset)
    {
        for (int i = 0; i < cars.Count; i++)
        {
            lock (carsLocker[i])
            {
                if (cars[i].isActive)
                {
                    cars[i].trackManager.SetTrack(TrackManager.trackManager.GetTrack());
                    cars[i].Reset(!completeReset);
                    cars[i].threaded = threaded;

                    if (!threaded)
                        continue;

                    if (i == 0)
                    {
                        cars[i].trackSphereMat.color = new Color(255, 215, 0);
                        cars[i].trailren.material.color = new Color(255, 215, 0);
                    }
                    else
                    {
                        cars[i].trackSphereMat.color = Color.blue;
                        cars[i].trailren.material.color = Color.blue;
                    }
                }
            }
        }
    }

    void AddPlayers()
    {
        int count = 0;
        foreach (HumanPlayer humanPlayer in humanPlayers)
        {
            lock (carsLocker[count])
            {
                cars[count].gameObject.SetActive(true);
                cars[count].SetHumanPlayer(humanPlayer);
            }
            count++;
        }

        foreach (AIPlayer aiPlayer in aiPlayers)
        {
            lock (carsLocker[count])
            {
                cars[count].gameObject.SetActive(true);
                cars[count].SetAiPlayer(aiPlayer);
            }
            count++;
        }

        for (int i = count; i < cars.Count; i++)
        {
            lock (carsLocker[i])
            {
                cars[i].SetAiPlayer(null);
                cars[i].SetHumanPlayer(null);
                cars[i].isActive = false;
                cars[i].gameObject.SetActive(false);
            }
        }
    }

    void AdjustViewSettings()
    {
        if (curViewType == ViewType.MenuView)
        {
            GA_Parameters.updateRate = 0;
            cameraController.SetFollowCars(null);
            cameraController.gameObject.SetActive(false);
        }
        else if (curViewType == ViewType.HumanCarView || curViewType == ViewType.AICarView)
        {
            GA_Parameters.updateRate = 1;
            List<CarController> carControllers = new List<CarController>();

            if (curViewType == ViewType.HumanCarView)
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
    IEnumerator Race(bool forceCompleteRace)
    {
        if (forceCompleteRace)
        {
            cameraController.UpdateTransform();
            Coroutine routine = StartCoroutine(cameraController.raceCanvas.CountDown());
            UIController.instance.activeRoutines.Add(routine);
            yield return routine;
        }

        ResetRaceParameters();

        // If a we want to do a complete race the race will only stop if everyone finishes
        float extraTime = 0;
        if(forceCompleteRace)
            extraTime = 1000000000;

        // For each second
        for (int k = 0; k < GA_Parameters.simulationTime + extraTime; k++)
        {
            // For each frame in that second
            for (int l = 0; l < GA_Parameters.fps; l++)
            {
                countSinceFrame++;
                ingameTimePassed += (1f / GA_Parameters.fps);

                float waitTime = 0;
                float dTime = 0;
                if (framesSkipped >= frameSkip)
                {

                    // Show a frame if the framerate drops under the training framerate
                    while (Time.realtimeSinceStartup - curTime < (1f / GA_Parameters.fps) * (frameSkip + 1))
                    {
                        waitTime += 1;
                    }

                    framesSkipped = 0;
                    dTime = Time.realtimeSinceStartup - curTime;
                    curTime = Time.realtimeSinceStartup;

                    yield return null;
                }
                else
                {
                    framesSkipped++;
                    waitTime = 1;
                }
                if (waitTime == 0)
                    frameSkip++;
                else if (waitTime > 100)
                {
                    if(frameSkip > 0)
                        frameSkip--;
                }

                print(1f / dTime);
                // If a camera is following a car update its transform
                CameraController.instance.UpdateTransform();

                // for each car in the group
                for (int carControllerindex = 0; carControllerindex < cars.Count; carControllerindex++)
                {
                    // Get the cars controller
                    CarController currentCarController = cars[carControllerindex];

                    // Check if its active and if car is still racing, if not continue
                    if (currentCarController.finished || (currentCarController.humanPlayer == null && currentCarController.aIPlayer == null))
                        continue;

                    // Update the car to its next position.
                    currentCarController.UpdateCar(1f/GA_Parameters.fps, forceCompleteRace, GA_Parameters.simulationTime + extraTime);

                    currentCarController.UpdateCar();

                    // if a car is still racing don't end the race
                    stopCurRace = false;
                }

                // If simulating to fast a pause is set up here
                //Coroutine limitRoutine = StartCoroutine(LimitPlaybackSpeed());

                //if (limitRoutine != null)
                //    yield return limitRoutine;

                //if (stopCurRace)
                //{
                //    yield return null;
                //    yield break;
                //}

            }
        }
    }

    IEnumerator ThreadedRace(bool forceCompleteRace)
    {
        int extraTime = 0;

            if (forceCompleteRace)
            {
                // Make sure the camera is at the right position when counting down
                cameraController.UpdateTransform();

                // Start the countdown coroutine and make sure it is added to the coroutines list
                Coroutine routine = StartCoroutine(cameraController.raceCanvas.CountDown());
                UIController.instance.activeRoutines.Add(routine);

                // Show a frame
                yield return routine;

                // If a we want to do a complete race the race will only stop if everyone finishes
                extraTime = 1000000000;
            }

            ResetRaceParameters();
            CreateThreadActions((int)(GA_Parameters.simulationTime + extraTime));

        // For each second
        for (int k = 0; k < GA_Parameters.simulationTime + extraTime; k++)
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
                stopCurRace = true;
                // for each car in the group
                for (int carControllerindex = 0; carControllerindex < cars.Count; carControllerindex++)
                {

                    // Get the cars controller
                    CarController currentCarController = cars[carControllerindex];

                    if (currentCarController.IsDone() && carDone[carControllerindex])
                    {
                        continue;
                    }

                    while (!currentCarController.UpdateCar())
                    {
                        yield return null;

                        curSpeed = countSinceFrame / (GA_Parameters.fps * (Time.realtimeSinceStartup - curTime));

                        // reset these values
                        countSinceFrame = 0;
                        curTime = Time.realtimeSinceStartup;
                    }

                    if (currentCarController.IsDone() && carControllerindex != 0 && !carDone[carControllerindex])
                    {
                        currentCarController.trackSphereMat.color = Color.red;
                        currentCarController.trailren.material.color = Color.red;

                        carDone[carControllerindex] = true;
                    }
                    else if (currentCarController.IsDone() && carControllerindex == 0 && !carDone[carControllerindex])
                        carDone[carControllerindex] = true;


                    stopCurRace = false;
                }



                // If a camera is following a car update its transform
                CameraController.instance.UpdateTransform();

                // If simulating to fast a pause is set up here
                Coroutine limitRoutine = StartCoroutine(LimitPlaybackSpeed());

                if (limitRoutine != null)
                    yield return limitRoutine;
            }

            if (stopCurRace || MyThreadPool.GetWaitingThreads() == MyThreadPool.workers.Length)
            {
                curDone = true;

                while (MyThreadPool.GetWaitingThreads() != MyThreadPool.workers.Length)
                {
                    yield return null;
                }

                yield break;
            }

        }
        curDone = true;

        while (MyThreadPool.GetWaitingThreads() != MyThreadPool.workers.Length)
        {
            yield return null;
        }

    }
    
    void ThreadedRace(object args)
    {
        float raceTime = (int)args;
        CarController car;
        int carIndex = 0;

        object frameLocker = new object();

        while (true)
        {
            
            lock (carLocker)
            {
                if (currentCar < cars.Count - 1)
                {
                    currentCar++;
                }
                else
                {
                    currentCar = 0;
                }
                car = cars[currentCar];
                carIndex = currentCar;
            }
            

            lock (carsLocker[carIndex])
            {

                if (MyThreadPool.abort || curDone)
                {
                    break;
                }

                if (!car.isActive)
                    continue;

                // Update the car to its next position.
                if (!car.UpdateCar(1f / GA_Parameters.fps, !GA_Parameters.stopAtCrash, raceTime))
                    car.isActive = false;
            }
            
        }
    }

    void CreateThreadActions(int raceTime)
    {
        object args = raceTime;

        for (int i = 0; i < MyThreadPool.workers.Length; i++)
        {
            WaitCallback callback = new WaitCallback(ThreadedRace);
            MyThreadPool.AddActionToQueue(callback, args);
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
        currentCar = 0;

        lock(frameLocker)
            currentFrame = 0;

        curDone = false;

        for (int i = 0; i < carDone.Length; i++)
        {
            carDone[i] = false;
        }
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

    public float GetTotalTime()
    {
        return ingameTimePassed;
    }

    public float GetCurSpeed()
    {
        return curSpeed;
    }

    public void SetViewSettings(ViewType view, bool updateNow)
    {
        curViewType = view;
        if(updateNow)
            AdjustViewSettings();
    }

    public List<CarController> GetCurrentCompetingCars()
    {
        List<CarController> currentCars = new List<CarController>();

        for (int i = 0; i < cars.Count; i++)
            if (cars[i].isActiveAndEnabled)
                currentCars.Add(cars[i]);

        return currentCars;
    }
}
