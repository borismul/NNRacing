using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CarTrainer : MonoBehaviour
{
    public List<CarController> carControllers = new List<CarController>();
    public GameObject carPrefab;
    public GameObject ghostCarPrefab;

    public static CarTrainer instance;

    public bool stop = false;

    public bool pause;
    public bool isPaused;

    public int curGeneration;

    float realTimePassed = 0;
    float startTime = 0;
    float ingameTimePassed = 0;

    float bestFitnessTemp;

    public static System.Random rand = new System.Random();

    public List<NeuralNetwork> generationNetworks = new List<NeuralNetwork>();

    public GeneticAlgorithm ga;

    List<float> fitnesses = new List<float>();

    float avgDTime;

    float curTime;

    bool watch = false;

    float timeLeft;

    float curSpeed = 0;

    Vector3 outOfTheWay = new Vector3(100000, 100000, 100000);

    void Awake()
    {
        Application.targetFrameRate = 60;

        Application.runInBackground = true;
        instance = this;
    }


    public void StartSim()
    {
        InitializeNetworks();
        StartCoroutine("Simulate");
    }

    IEnumerator Simulate()
    {
        avgDTime = 1f / 60;
        generationNetworks = ga.CreateNetworks();
        List<NeuralNetwork> networks = new List<NeuralNetwork>();

        // For each generation
        while (true)
        {
            yield return StartCoroutine(Pause());

            if (stop)
                yield break;

            curGeneration++;
            fitnesses.Clear();
            float sum = 0;

            // For each individual in a group
            for (int individual = 0; individual < generationNetworks.Count; individual++)
            {
                if (individual >= generationNetworks.Count)
                    continue;

                networks.Add(generationNetworks[individual]);
            }
            yield return StartCoroutine(CreateCars(networks, false, false, 0));

            // Get the fitnesses of the networks and calculate the sum of all fitnesses
            for (int i = 0; i < carControllers.Count; i++)
            {
                if (!carControllers[i].isActive)
                    continue;

                fitnesses.Add(carControllers[i].GetFitnessTracker().GetFitness());

                if (carControllers[i].GetFitnessTracker().GetFitness() > bestFitnessTemp)
                    bestFitnessTemp = carControllers[i].GetFitnessTracker().GetFitness();

                sum += carControllers[i].GetFitnessTracker().GetFitness();
            }


            networks.Clear();
            if (sum == 0)
            {
                InitializeNetworks();
                generationNetworks = ga.CreateNetworks();
                continue;
            }
            UIController.instance.UpdateUI(bestFitnessTemp, curGeneration, 0, 0);
            
            // Update the best and average fitness on the UI
            UIController.instance.UpdateUI(bestFitnessTemp, sum/ga.populationSize, curGeneration, 0, 0, true);

            // Determine the population for the next generation based on the fitnesses of the current networks
            generationNetworks = ga.DoGeneration(fitnesses);
        }
    }

    IEnumerator Pause()
    {
        while (pause || isPaused)
        {
            if (pause)
            {
                pause = false;
            }
            isPaused = true;
            yield return null;
        }
    }

    IEnumerator Race()
    {
        // variable used to check if new frame is needed
        realTimePassed = 0;
        startTime = Time.realtimeSinceStartup;
        realTimePassed = Time.realtimeSinceStartup;
        curTime = Time.realtimeSinceStartup;
        bool shouldStop = true;
        ingameTimePassed = 0;
        timeLeft = GA_Parameters.simulationTime;
        curSpeed = 0;
        int countSinceFrame = 0;
        // For each second
        for (int k = 0; k < GA_Parameters.simulationTime; k++)
        {
            // For each frame
            for (int l = 0; l < GA_Parameters.fps; l++)
            {
                countSinceFrame++;

                if (Time.realtimeSinceStartup - curTime > 1f/GA_Parameters.fps)
                {
                    yield return null;
                    curSpeed = 1f / Time.deltaTime / GA_Parameters.fps * countSinceFrame;
                    countSinceFrame = 0;
                    curTime = Time.realtimeSinceStartup;
                }

                shouldStop = true;
                ingameTimePassed += (1f / GA_Parameters.fps);
                timeLeft -= (1f / GA_Parameters.fps);
                // for each car in the group
                for (int carControllerindex = 0; carControllerindex < carControllers.Count; carControllerindex++)
                {
                    CarController currentCarController = carControllers[carControllerindex];

                    if (!currentCarController.isActive)
                        continue;

                    realTimePassed = Time.realtimeSinceStartup - startTime;

                    if (watch)
                    {
                        if (currentCarController.carFollowCamera.isActiveAndEnabled)
                            currentCarController.carFollowCamera.UpdateTransform();

                        if (carControllerindex == 0)
                        {

                            if (ingameTimePassed / GA_Parameters.updateRate - realTimePassed > 0)
                            {
                                yield return new WaitForSeconds(ingameTimePassed / GA_Parameters.updateRate - realTimePassed);
                                curSpeed = GA_Parameters.updateRate;
                                curTime = Time.realtimeSinceStartup;
                                countSinceFrame = 0;

                            }
                            else
                            {
                                ingameTimePassed -= ingameTimePassed / GA_Parameters.updateRate - realTimePassed;

                            }
                        }
                    }

                    if (currentCarController.GetCurrentTrack() >= currentCarController.GetTrackManager().GetTrackCount())
                        continue;

                    // Update the car to its next position. If it returns a false it starts the next track (if possible)
                    if (!currentCarController.UpdateCar(1f / GA_Parameters.fps, GA_Parameters.stopAtCrash))
                        currentCarController.GetTrackManager().SelectNextTrack();

                    shouldStop = false;
                }
                if (shouldStop)
                {
                    UIController.instance.runningPlay = false;
                    yield break;
                }
            }
        }
        UIController.instance.runningPlay = false;
    }

    public IEnumerator CreateCars(List<NeuralNetwork> networks, bool challenge, bool play, float startTime)
    {
        if (challenge || play)
        {
            SetUpdateRate(1f, true);
        }

        for (int i = 0; i < carControllers.Count; i++)
        {
            carControllers[i].transform.position = outOfTheWay;
            carControllers[i].SetNewNetwork(null, false, true);
        }

        for (int i = 0; i < networks.Count; i++)
        {
            GameObject car;
            if (carControllers.Count - 1 < i)
            {
                if (!challenge)
                    car = Instantiate(carPrefab);
                else
                {
                    if (networks[i] == null)
                        car = Instantiate(carPrefab);
                    else
                    {
                        car = Instantiate(carPrefab);
                    }
                }
                carControllers.Add(car.GetComponent<CarController>());
            }


            carControllers[i].GetTrackManager().SelectTrack(0);
            carControllers[i].SetNewNetwork(networks[i], play || challenge, false);
            carControllers[i].carFollowCamera.UpdateTransform();

        }

        yield return new WaitForSeconds(startTime);

        Coroutine routine = StartCoroutine(Race());

        if (challenge || play)
            UIController.instance.activeRoutines.Add(routine);

        System.GC.Collect();

        yield return routine;
    }

    void InitializeNetworks()
    {
        pause = false;
        ga = new GeneticAlgorithm(GA_Parameters.populationSize, GA_Parameters.inputs, 2, new int[2] { 8, 6 }, GA_Parameters.outputs);
    }

    public void SetUpdateRate(float updateRate, bool watch)
    {
        if (GA_Parameters.updateRate == 0)
            GA_Parameters.updateRate = 1;

        realTimePassed = Time.realtimeSinceStartup - startTime;

        if (updateRate != 1)
        {
            float ratio = updateRate / GA_Parameters.updateRate;
            ingameTimePassed *= ratio;
        }
        else
        {
            ingameTimePassed = realTimePassed;
        }
        this.watch = watch;

        GA_Parameters.updateRate = updateRate;

        }

        public void Reset()
    {


        stop = true;
        pause = true;

        StartCoroutine(_Reset());

    }

    IEnumerator _Reset()
    {
        while (!isPaused)
        {
            yield return null;
        }


        for (int i = 0; i < carControllers.Count; i++)
        {
            carControllers[i].transform.position = outOfTheWay;
            carControllers[i].SetNewNetwork(null, false, true);
        }

        carControllers = new List<CarController>();

        curGeneration = 0;

        realTimePassed = 0;
        startTime = 0;

        bestFitnessTemp = 0;

    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    public float GetCurSpeed()
    {
        return curSpeed;
    }
}
