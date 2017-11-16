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

    float dTime = 0;
    float startTime = 0;

    float bestFitnessTemp;

    public static System.Random rand = new System.Random();

    public List<NeuralNetwork> generationNetworks = new List<NeuralNetwork>();

    public GeneticAlgorithm ga;

    List<float> fitnesses = new List<float>();

    float avgDTime;

    void Awake()
    {
        Application.targetFrameRate = 60;

        Application.runInBackground = true;
        instance = this;
        //Random.InitState((int)System.DateTime.Now.Ticks);
    }

    void Update()
    {
        // Update the averge delta time (used to calculate the cars that are simulated per frame)
        avgDTime += Time.deltaTime;
        avgDTime /= 2;

    }

    public void StartSim()
    {
        InitializeNetworks();
        StartCoroutine("Simulate");
    }

    IEnumerator Simulate()
    {
        GA_Parameters.carsPerFrame = 1;
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
            int curCarsPerFrame = GA_Parameters.carsPerFrame;

            // For each group of individuals in a generation
            for (int individuals = 0; individuals < generationNetworks.Count; individuals += curCarsPerFrame)
            {
                curCarsPerFrame = GA_Parameters.carsPerFrame;

                // For each individual in a group
                for (int individual = 0; individual < curCarsPerFrame; individual++)
                {
                    if (individual + individuals >= generationNetworks.Count)
                        continue;

                    networks.Add(generationNetworks[individuals + individual]);
                }
                yield return StartCoroutine(CreateCars(networks, false, false, 0));

                // Update the cars per frame so the framerate is not too low
                DetermineCarsPerFrame();

                // Get the fitnesses of the networks and calculate the sum of all fitnesses
                for (int i = 0; i < carControllers.Count; i++)
                {
                    fitnesses.Add(carControllers[i].GetFitnessTracker().GetFitness());

                    if (carControllers[i].GetFitnessTracker().GetFitness() > bestFitnessTemp)
                        bestFitnessTemp = carControllers[i].GetFitnessTracker().GetFitness();

                    sum += carControllers[i].GetFitnessTracker().GetFitness();
                }
                networks.Clear();
                UIController.instance.UpdateUI(bestFitnessTemp, curGeneration, (individuals), 0, 0);
            }
            // Update the best and average fitness on the UI
            UIController.instance.UpdateUI(bestFitnessTemp, sum/ga.populationSize, curGeneration, 0, 0, 0, true);

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
        dTime = 0;
        startTime = Time.realtimeSinceStartup;
        dTime = Time.realtimeSinceStartup;

        // For each second
        for (int k = 0; k < GA_Parameters.simulationTime; k++)
        {
            // For each frame
            for (int l = 0; l < GA_Parameters.fps; l++)
            {
                bool shouldStop = true;

                // for each car in the group
                for (int carControllerindex = 0; carControllerindex < carControllers.Count; carControllerindex++)
                {
                    CarController currentCarController = carControllers[carControllerindex];

                    // If the car didn't finish all its tracks
                    if (currentCarController.GetCurrentTrack() < currentCarController.GetTrackManager().GetTrackCount())
                    {
                        // Update the car to its next position. If it returns a false it starts the next track (if possible)
                        if (!currentCarController.UpdateCar(1f / GA_Parameters.fps, GA_Parameters.stopAtCrash))
                            currentCarController.GetTrackManager().SelectNextTrack();

                        shouldStop = false;
                    }

                    if (carControllerindex == carControllers.Count - 1 && GA_Parameters.carUpdateRate != 0)
                    {
                        currentCarController.carFollowCamera.UpdateTransform();

                        if (dTime - startTime - (k + 1f / GA_Parameters.fps * l) / GA_Parameters.carUpdateRate < 0)
                        {
                            if(-(dTime - startTime - (k + 1f / GA_Parameters.fps * l) / GA_Parameters.carUpdateRate) > 0)
                                yield return new WaitForSeconds(-(dTime - startTime - (k + 1f / GA_Parameters.fps * l) / GA_Parameters.carUpdateRate));

                            dTime = Time.realtimeSinceStartup;
                        }
                        else
                            yield return null;
                    }
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
            SetUpdateRate(1);
            for (int i = 0; i < carControllers.Count; i++)
            {
                Destroy(carControllers[0].gameObject);
                carControllers.RemoveAt(0);
                i--;
            }
        }

        for (int i = 0; i < carControllers.Count; i++)
        {
            Destroy(carControllers[0].gameObject);
            carControllers.RemoveAt(0);
            i--;
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
                        car = Instantiate(ghostCarPrefab);
                    }
                }
                carControllers.Add(car.GetComponent<CarController>());
            }

            carControllers[i].GetTrackManager().SelectTrack(0);
            carControllers[i].SetNewNetwork(networks[i], play || challenge);
            carControllers[i].carFollowCamera.UpdateTransform();

        }

        yield return new WaitForSeconds(startTime);

        Coroutine routine = StartCoroutine(Race());

        if (challenge || play)
            UIController.instance.activeRoutines.Add(routine);

        yield return routine;
    }

    void DetermineCarsPerFrame()
    {
        if (!isPaused && avgDTime > 1f / 20)
        {
            if (GA_Parameters.carsPerFrame > 1)
                GA_Parameters.carsPerFrame--;
        }
        else if (!isPaused && avgDTime < 1f / 20)
            GA_Parameters.carsPerFrame++;
    }

    void InitializeNetworks()
    {
        pause = false;
        ga = new GeneticAlgorithm(GA_Parameters.populationSize, GA_Parameters.inputs, 1, new int[1] { 10 }, GA_Parameters.outputs);
    }

    public void SetUpdateRate(int updateRate)
    {
        GA_Parameters.carUpdateRate = updateRate;

        dTime = 0;
        startTime = Time.realtimeSinceStartup;
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
            Destroy(carControllers[i].carFollowCamera);
            Destroy(carControllers[i]);
        }

        carControllers = new List<CarController>();

        curGeneration = 0;

        dTime = 0;
        startTime = 0;

        bestFitnessTemp = 0;

        System.Random rand = new System.Random();

        List<NeuralNetwork> generationNetworks = new List<NeuralNetwork>();
    }
}
