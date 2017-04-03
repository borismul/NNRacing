using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneticAlgorithm : MonoBehaviour
{
    public int numGenerations;
    public int populationSize;
    public int carsPerFrame;
    public int carUpdateRate;
    public int laps = 1;
    public bool stopAtCrash;

    public float accSpeed;
    public float breakSpeed = -150;
    public float turnSpeed;
    public float maxVelocity;

    public float simulationTime;
    public int fps;

    public float NNWeightsMutationChance;
    public float NNMutationChance;
    public float cloneChance;
   
    public List<CarController> carControllers = new List<CarController>();
    public GameObject carPrefab;
    public GameObject ghostCarPrefab;
    public List<Generation> generations = new List<Generation>();

    public NeuralNetwork currentNetwork;

    public static GeneticAlgorithm instance;

    public float maxFitness;
    float avgFitness;

    public bool stop = false;

    public bool pause;
    public bool isPaused;

    public int curGeneration;

    float dTime = 0;
    float startTime = 0;
    int frameCount = 0;

    void Awake()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        instance = this;
    }

    public void StartSim()
    {
        InitializeNetworks();
        StartCoroutine("Simulate");
    }

    IEnumerator Simulate()
    {
        List<NeuralNetwork> networks = new List<NeuralNetwork>();

        // For each generation
        for (curGeneration = 0; curGeneration < numGenerations; curGeneration++)
        {
            yield return StartCoroutine(Pause());

            // For each group of individuals in a generation
            for (int individuals = 0; individuals < generations[curGeneration].networks.Count / carsPerFrame; individuals++)
            {
                // For each individual in a group
                for (int individual = 0; individual < carsPerFrame; individual++)
                {
                    networks.Add(generations[curGeneration].networks[individuals * carsPerFrame + individual]);
                }
                yield return StartCoroutine(CreateCars(networks, false, false, 0));

                for (int i = 0; i < carControllers.Count; i++)
                    carControllers[i].SaveFitness();

                networks.Clear();
                int a = (individuals + 1) * carsPerFrame + 1;
                UIController.instance.UpdateUI(maxFitness, curGeneration, (individuals + 1) * carsPerFrame + 1, 0, 0);
                yield return null;
            }
            UIController.instance.UpdateUI(maxFitness, avgFitness, curGeneration, 0, 0, 0, true);
            CreateNextGen(curGeneration);
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
        frameCount = 0;
        // For each second
        for (int k = 0; k < simulationTime; k++)
        {
            // For each frame
            for (int l = 0; l < fps; l++)
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
                        if (!currentCarController.UpdateCar(1f / fps, stopAtCrash))
                            currentCarController.GetTrackManager().SelectNextTrack();

                        shouldStop = false;
                    }

                    if (carControllerindex == carControllers.Count - 1 && carUpdateRate != 0)
                    {
                        if (dTime - startTime - (k + 1f / fps * l) / carUpdateRate < 0)
                        {
                            dTime = Time.realtimeSinceStartup;
                            yield return null;
                            frameCount = 0;
                        }
                        frameCount++;
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
        List<CarController> carcontroller = carControllers;
        if (challenge || play)
        {
            SetUpdateRate(1);
            for (int i = 0; i < carControllers.Count; i++)
            {
                Destroy(carControllers[carControllers.Count - 1].gameObject);
                carControllers.RemoveAt(carControllers.Count - 1);
                i--;
            }
        }

        for (int i = 0; i < carControllers.Count - networks.Count; i++)
        {
            Destroy(carControllers[networks.Count].gameObject);
            carControllers.RemoveAt(networks.Count);
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
        }

        yield return new WaitForSeconds(startTime);

        Coroutine routine = StartCoroutine(Race());

        if (challenge || play)
            UIController.instance.activeRoutines.Add(routine);

        yield return routine;
    }

    void InitializeNetworks()
    {
        List<NeuralNetwork> networks = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            networks.Add(new NeuralNetwork(8, 6, 4));
        }

        generations.Add(new Generation(networks));
    }

    void CreateNextGen(int gen)
    {
        List<NeuralNetwork> networks = new List<NeuralNetwork>();
        List<float> dinges = new List<float>();
        // Calculate sum of all fitness in generation
        float sum = 0;
        for (int i = 0; i < populationSize; i++)
        {
            sum += generations[gen].networks[i].Fitness;
        }

        // Determine average en maximum fitness in this generation
        avgFitness = sum / populationSize;
        generations[gen].SetFitness(maxFitness, avgFitness);

        //// normalize the fitnesses so their sum is 1
        //if (sum > 0)
        //{
        //    for (int i = 0; i < populationSize; i++)
        //    {
        //        generations[gen].networks[i].Fitness /= sum;
        //        dinges.Add(generations[gen].networks[i].Fitness);
        //    }
        //}

        generations[gen].Order();

        if (sum == 0)
        {
            generations.Add(new Generation(generations[gen].networks));
            return;
        }


        NeuralNetwork dad = null;
        NeuralNetwork mom = null;

        networks.AddRange(generations[gen].networks.GetRange(0, populationSize /10 * 4));

        // Produce children
        for (int i = 0; i < Mathf.RoundToInt((populationSize/10 * 6)+1); i++)
        {
            dad = null;
            mom = null;

            while (dad == null || mom == null)
            {
                int boundary = (generations[gen].networks.Count) * (generations[gen].networks.Count + 1)/2;
                int dadNum = NeuralNetwork.rand.Next(-1, boundary -1);
                int momNum = NeuralNetwork.rand.Next(-1, boundary -1);

                sum = 0;
                for (int j = 0; j < generations[gen].networks.Count; j++)
                {
                    sum += j;

                    if (dadNum < sum)
                    {
                        dad = generations[gen].networks[generations[gen].networks.Count - 1 - j];
                        dadNum = int.MaxValue;

                        //if (dad != null && dad.Equals(mom))
                        //{
                        //    dad = null;
                        //    mom = null;
                        //    break;
                        //}
                    }
                    if (momNum < sum)
                    {


                        mom = generations[gen].networks[generations[gen].networks.Count - 1 - j];
                        momNum = int.MaxValue;

                        //    if (dad != null && dad.Equals(mom))
                        //    {
                        //        dad = null;
                        //        mom = null;
                        //        break;
                        //    }
                    }
                }
            }

            float clonePar = (float)NeuralNetwork.rand.NextDouble();

            if (clonePar > cloneChance)
            {
                try
                { 
                    networks.Add(Gene.MakeChild(Gene.Encode(dad), Gene.Encode(mom), NNMutationChance, NNWeightsMutationChance));
                }
                catch (System.Exception e)
                {
                    i--;
                }
            }
            else
                networks.Add(Gene.MakeChild(Gene.Encode(dad), Gene.Encode(dad), NNMutationChance, NNWeightsMutationChance));
        }

        Generation generation = new Generation(networks);
        generations.Add(generation);
    }

    public void SetUpdateRate(int updateRate)
    {
        carUpdateRate = updateRate;

        dTime = 0;
        startTime = Time.realtimeSinceStartup;
        frameCount = 0;
    }

}
