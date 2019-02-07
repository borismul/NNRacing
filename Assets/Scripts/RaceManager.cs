using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public enum ViewType { MenuView, TopView, AICarView, HumanCarView }

    public static RaceManager raceManager;
    object counterLocker = new object();

    CameraController cameraController;

    List<HumanPlayer> humanPlayers = new List<HumanPlayer>();
    List<AIPlayer> aiPlayers = new List<AIPlayer>();
    List<CarController> cars = new List<CarController>();

    public GameObject carPrefab;

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
    object framesPerThreadLocker = new object();
    object curDoneLocker = new object();
    object threadCounterLocker = new object();
    bool curDone = false;

    bool[] carDone;
    bool pause;
    int frameSkip = 0;
    int framesSkipped = 0;
    int threadCounter = 0;
    public bool canChangeFrames;
    bool training;
    float deltaTime;
    float maxDeltaTime;

    public PositionsInfoManager posManager;

    CustomFixedUpdate m_FixedUpdate;
    public bool runRace;
    bool finished = true;

    float simulationTime;

    List<CarController> currentCars = new List<CarController>();

    List<GameObject> trackPlanes = new List<GameObject>();

    WaitForSeconds wait = new WaitForSeconds(1);

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

    public void Reset()
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

    void FixedUpdate()
    {
        if(!training)
            FinishRace();

        if (!runRace || pause)
            return;

        Race(Time.fixedDeltaTime);
        cameraController.UpdateTransform(Time.fixedDeltaTime);

    }

    private void LateUpdate()
    {
        if (!runRace || pause)
            return;

    }

    void FinishRace()
    {
        if (!finished && !runRace)
        {
            List<CarController> currentCars = new List<CarController>();

            for (int i = 0; i < cars.Count; i++)
            {
                if (cars[i].humanPlayer != null || cars[i].aIPlayer != null)
                    currentCars.Add(cars[i]);
            }

            if(posManager!= null)
                posManager.CreatePositionList(currentCars);
            finished = true;
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

    public void AddAIPlayer(string name, NeuralNetwork network, int i)
    {
        if (i < aiPlayers.Count)
            aiPlayers[i].SetPlayer(name, network);
        else
            aiPlayers.Add(new AIPlayer(name, network));
    }

    public void FinishPlayers(int count)
    {

        for (int i = aiPlayers.Count - 1; i > count - 1; i--)
        {
            this.aiPlayers.RemoveAt(i);
        }
    }

    public void ResetPlayers()
    {
        humanPlayers.Clear();
        aiPlayers.Clear();
    }

    public IEnumerator StartRace(bool threaded, List<TrackManager> tracks, bool fancy)
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            training = true;
            runRace = false;

            UIController.instance.UpdateUI(i, tracks.Count);

            tracks[i].gameObject.SetActive(true);

            carsLocker = new object[GA_Parameters.populationSize];

            for (int cars = 0; cars < carsLocker.Length; cars++)
            {
                carsLocker[cars] = new object();
            }

            CreateCars(!threaded);
            AddPlayers();
            SetCarsReady(threaded, i == 0, i);
            AdjustViewSettings();

            yield return null;

            if (threaded)
            {
                training = true;
                runRace = false;
                Coroutine routine = StartCoroutine(ThreadedRace(humanPlayers.Count > 0));
                UIController.instance.activeRoutines.Add(routine);
                yield return routine;
                UpdateFitnesses();
            }
            else
            {
                if (humanPlayers.Count > 0)
                {
                    cameraController.UpdateTransform();
                    Coroutine routine = StartCoroutine(cameraController.raceCanvas.CountDown());
                    UIController.instance.activeRoutines.Add(routine);
                    yield return routine;
                }
                ResetRaceParameters();
                curTime = Time.realtimeSinceStartup;
                training = false;
                runRace = true;
            }

            tracks[i].gameObject.SetActive(false);

        }
    }

    public IEnumerator StartRace(bool threaded, List<string> tracks, bool fancy, bool createMap)
    {
        for (int i = 0; i < tracks.Count; i++)
        {
            training = true;
            runRace = false;

            //UIController.instance.UpdateUI(i, tracks.Count);
            //CarTrainer.instance.trackManagers[0].gameObject.SetActive(true);

            TrackManager.LoadTrack(fancy, tracks[i], createMap);


            //carsLocker = new object[GA_Parameters.populationSize];

            //for (int cars = 0; cars < carsLocker.Length; cars++)
            //{
            //    carsLocker[cars] = new object();
            //}

            CreateCars(!threaded);
            AddPlayers();
            SetCarsReady(threaded, i == 0, i);
            AdjustViewSettings();

            yield return null;

            if (threaded)
            {
                training = true;
                runRace = false;
                Coroutine routine = StartCoroutine(ThreadedRace(humanPlayers.Count > 0));
                UIController.instance.activeRoutines.Add(routine);
                yield return routine;
                UpdateFitnesses();
            }
            else
            {
                if (humanPlayers.Count > 0)
                {
                    cameraController.UpdateTransform();
                    Coroutine routine = StartCoroutine(cameraController.raceCanvas.CountDown());
                    UIController.instance.activeRoutines.Add(routine);
                    yield return routine;
                }
                ResetRaceParameters();
                curTime = Time.realtimeSinceStartup;
                training = false;
                runRace = true;
            }
        }
    }

    public IEnumerator TestTrack()
    {
        ResetPlayers();

        Genome genome = SaveableObjects.SaveableNeuralNetwork.LoadNetwork("Best Overall ");
        aiPlayers.Add(new AIPlayer("Tester", genome.CreateNetwork()));
        carsLocker = new object[1];
        carsLocker[0] = new object();

        CreateCars(true);
        AddPlayers();
        SetCarsReady(true, true, 0);

        ResetRaceParameters();
        CreateThreadActions((500));

        GA_Parameters.fps = 60;
        GA_Parameters.laps = 3;
        GA_Parameters.stopAtCrash = false;

        while (MyThreadPool.GetWaitingThreads() != MyThreadPool.workers.Length || cars[0].positions.Count > 0)
        {
            for(int i = 0; i < 40; i++)
                cars[0].UpdateCar();

            yield return null;
        }

    }

    void CreateCars(bool destroyFirst)
    {
        if (destroyFirst)
        {
            for (int i = 0; i < cars.Count; i++)
            {
                if(cars[i] != null)
                    Destroy(cars[i].gameObject);
            }

            cars.Clear();
        }

        if (humanPlayers.Count + aiPlayers.Count > cars.Count)
        {
            while (humanPlayers.Count + aiPlayers.Count > cars.Count)
            {
                cars.Add(Instantiate(carPrefab).GetComponent<CarController>());
            }
        }
    }

    void SetCarsReady(bool threaded, bool completeReset, int trackNum)
    {
        for (int i = 0; i < cars.Count; i++)
        {
            lock (carsLocker[i])
            {
                if (cars[i].GetActive())
                {
                    if (!cars[i].trackManager.SetTrack(trackNum))
                        cars[i].trackManager.SetTrack(TrackManager.trackManager.GetTrack());
                    cars[i].Reset(!completeReset, false);
                    cars[i].threaded = threaded;

                    if (!threaded)
                        continue;

                    if (cars[i].aIPlayer.network.bestOfAll)
                    {
                        cars[i].trackSphereMat.color = new Color(255, 215, 0);
                        cars[i].trailren.material.color = new Color(255, 215, 0);
                    }
                    else if (cars[i].aIPlayer.network.leader)
                    {
                        cars[i].trackSphereMat.color = new Color(192, 192, 192);
                        cars[i].trailren.material.color = new Color(192, 192, 192);
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
                cars[i].SetActive(false);
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
            canChangeFrames = true;
        }
        else if (curViewType == ViewType.HumanCarView || curViewType == ViewType.AICarView)
        {
            GA_Parameters.updateRate = 1;
            List<CarController> carControllers = new List<CarController>();

            if (curViewType == ViewType.HumanCarView || curViewType == ViewType.AICarView)
            {
                for (int i = 0; i < cars.Count; i++)
                {
                    if (cars[i].isActiveAndEnabled && (cars[i].humanPlayer != null || cars[i].aIPlayer != null))
                        carControllers.Add(cars[i]);
                }
                cameraController.SetFollowCars(carControllers);

            }
            
        }
    }

    void UpdateFitnesses()
    {
        for(int i = 0; i < cars.Count; i++)
        {
            if (cars[i].aIPlayer != null)
                cars[i].EvalTotalFitness();
        }
    }

    public List<float> GetFitnesses()
    {
        List<float> fitnesses = new List<float>();

        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].isActiveAndEnabled)
                fitnesses.Add(cars[i].totalFitness);
        }

        return fitnesses;

    }

    public float GetBestRaceTime()
    {
        float bestTime = 999999999999999999999999999f;

        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].isActiveAndEnabled)
            {
                if (cars[i].GetFitnessTracker().time < bestTime)
                    bestTime = cars[i].GetFitnessTracker().time;
            }
        }

        return bestTime;
    }

    // Method that starts a race
    void Race(float deltaTime)
    {
        ingameTimePassed += deltaTime;

        stopCurRace = true;

        // for each car in the group
        for (int carControllerindex = 0; carControllerindex < cars.Count; carControllerindex++)
        {
            // Get the cars controller
            CarController currentCarController = cars[carControllerindex];

            // Check if its active and if car is still racing, if not continue
            if (currentCarController.finished || (currentCarController.humanPlayer == null && currentCarController.aIPlayer == null))
                continue;

            // Update the car to its next position.
            if (!currentCarController.UpdateCar(deltaTime, true, Mathf.Infinity))
            {
                continue;
            }

            if (!currentCarController.UpdateCar())
                continue;

            // if a car is still racing don't end the race
            stopCurRace = false;
        }

        if (stopCurRace)
        {
            runRace = false;
        }



    }

    IEnumerator ThreadedRace(bool forceCompleteRace)
    {

        if (forceCompleteRace)
        {
            // Make sure the camera is at the right position when counting down
            cameraController.UpdateTransform();

            // Start the countdown coroutine and make sure it is added to the coroutines list
            Coroutine routine = StartCoroutine(cameraController.raceCanvas.CountDown());
            UIController.instance.activeRoutines.Add(routine);

            // Show a frame
            yield return routine;

        }

        ResetRaceParameters();
        simulationTime = GA_Parameters.simulationTime;
        CreateThreadActions((int)(simulationTime));
        //ThreadedRace((int)simulationTime);

        while (true)
        {

            ingameTimePassed += (1f / GA_Parameters.fps);

            // Show a frame if the framerate drops under the training framerate
            if (Time.realtimeSinceStartup - curTime > 1f / GA_Parameters.fps)
            {


                yield return null;

                if (countSinceFrame != 0)
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

                if (cars[carControllerindex].IsDone() && carDone[carControllerindex])
                {
                    continue;
                }

                int counter = 0;
                while (!cars[carControllerindex].UpdateCar())
                {
                    counter++;
                    if (counter > 10)
                        cars[carControllerindex].SetActive(false);
                    if (!cars[carControllerindex].GetActive())
                        break;


                    if (!carDone[carControllerindex])
                    {

                        yield return null;
                    }

                    if (countSinceFrame != 0)
                        curSpeed = countSinceFrame / (GA_Parameters.fps * (Time.realtimeSinceStartup - curTime));

                    // reset these values
                    countSinceFrame = 0;
                    curTime = Time.realtimeSinceStartup;
                }

                if (cars[carControllerindex].IsDone() && !carDone[carControllerindex])
                {
                    if (!cars[carControllerindex].aIPlayer.network.bestOfAll && !cars[carControllerindex].aIPlayer.network.leader)
                    {
                        cars[carControllerindex].trackSphereMat.color = Color.red;
                        cars[carControllerindex].trailren.material.color = Color.red;
                    }

                    carDone[carControllerindex] = true;
                }
                else if (cars[carControllerindex].IsDone() && carControllerindex == 0 && !carDone[carControllerindex])
                    carDone[carControllerindex] = true;

                if (carDone[carControllerindex])
                    continue;

                stopCurRace = false;
                

            }

            countSinceFrame++;


            // If a camera is following a car update its transform
            CameraController.instance.UpdateTransform();

            // If simulating to fast a pause is set up here
            Coroutine limitRoutine = StartCoroutine(LimitPlaybackSpeed());


            if (stopCurRace /*|| MyThreadPool.GetWaitingThreads() == MyThreadPool.workers.Length*/ )
            {
                lock (curDoneLocker)
                    curDone = true;

                while (MyThreadPool.GetWaitingThreads() != MyThreadPool.workers.Length)
                {
                    yield return null;
                }

                break;
            }

            if (limitRoutine != null)
                yield return limitRoutine;
        }

        while (MyThreadPool.GetWaitingThreads() != MyThreadPool.workers.Length )
        {
            
            yield return null;
        }
    }

    void ThreadedRace(object args)
    {
        float raceTime = (int)args;
        CarController car;
        int carIndex = 0;
        int counter = 0;
        object frameLocker = new object();

        while (true)
        {
            bool stop;

            lock (curDoneLocker)
                stop = curDone;

            lock (carLocker)
            {
                lock(counterLocker)
                    counter++;

                if (currentCar < cars.Count - 1)
                {
                    currentCar++;
                }
                else
                {
                    currentCar = 0;
                }
                carIndex = currentCar;
                car = cars[currentCar];
            }
            if (MyThreadPool.abort || stop || counter > 10000)
            {
                break;
            }


            lock (carsLocker[carIndex])
            {
                if (!car.GetActive())
                    continue;

                lock (counterLocker)
                    counter = 0;

                if (MyThreadPool.abort || stop)
                {
                    break;
                }

                lock (threadCounterLocker)
                    threadCounter = 0;

                int frames;

                if (canChangeFrames)
                    frames = (int)(raceTime * GA_Parameters.fps) + 1;
                else
                    frames = 1;

                if (MyThreadPool.abort || stop)
                {
                    break;
                }
   
                // Update the car to its next position.
                if (!car.UpdateCar(1f / GA_Parameters.fps, !GA_Parameters.stopAtCrash, raceTime))
                {
                    car.SetActive(false);
                }

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
        threadCounter = 0;
        realTimePassed = 0;
        startTime = Time.realtimeSinceStartup;
        realTimePassed = Time.realtimeSinceStartup;
        curTime = Time.realtimeSinceStartup;
        ingameTimePassed = 0;
        countSinceFrame = 0;
        buildupTime = 0;
        currentTrackNum = 0;
        finished = false;

        lock (frameLocker)
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
            float waitTime = (ingameTimePassed + buildupTime) / GA_Parameters.updateRate - realTimePassed;

            if (waitTime > 1)
                waitTime = 1;
            float totalWaitTime = 0;

            while (totalWaitTime < waitTime)
            {
                yield return null;
                totalWaitTime += Time.deltaTime;
            }

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

    public void Pause()
    {
        pause = true;
    }

    public void UnPause()
    {
        pause = false;
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
        return Mathf.Max(simulationTime - ingameTimePassed, 0);
    }

    public float GetTotalTime()
    {
        return ingameTimePassed;
    }

    public float GetCurSpeed()
    {
        return Mathf.Min(curSpeed, GA_Parameters.updateRate);
    }

    public void SetViewSettings(ViewType view, bool updateNow)
    {
        curViewType = view;
        if(updateNow)
            AdjustViewSettings();
    }

    public List<CarController> GetCurrentCompetingCars()
    {
        currentCars.Clear();

        for (int i = 0; i < cars.Count; i++)
            if (cars[i].isActiveAndEnabled)
                currentCars.Add(cars[i]);

        return currentCars;
    }

    public void ClearTrails()
    {
        for(int i = 0; i < cars.Count; i++)
        {
            cars[i].ClearTrail();
        }
    }

}
