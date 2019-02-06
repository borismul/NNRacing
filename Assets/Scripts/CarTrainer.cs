using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CarTrainer : MonoBehaviour
{

    // Spawnable car
    public GameObject carPrefab;

    // singleton of the cartrainer
    public static CarTrainer instance;

    // Reference to the genetic algorithm
    public GeneticAlgorithm ga;

    // Reference to the race manager
    public RaceManager raceManager;

    // boolean that says the simulation to pause
    public bool pause;

    // boolean that says if it is really stopped now
    public bool isPaused;

    // boolean that can be set to stop the current race if all cars have crashed
    bool stopCurGen;

    // The best fitness up to now
    float bestFitnessSoFar;

    // the current generation we'er in
    public int curGeneration;

    public int bestGeneration;

    // A list of neural networks which is currently running
    public List<NeuralNetwork> generationNetworks = new List<NeuralNetwork>();

    // List of fitnesses in this generation
    List<float> fitnesses = new List<float>();

    public GameObject trackPrefab;

    public List<TrackManager> trackManagers = new List<TrackManager>();

    void Awake()
    {
        //Application.targetFrameRate = 60;
        Application.runInBackground = true;
        instance = this;
    }

    void Start()
    {
        raceManager = RaceManager.raceManager;


    }



    // This method starts the training of the neural networks
    public void StartSim()
    {
        pause = false;
        isPaused = false;
        InitializeNetworks();
        MyThreadPool.StartThreadPool(10);
        StartCoroutine("Train");
    }

    // train the neural networks environment
    IEnumerator Train()
    {
        bool complexify = true;
        int index = 0;

        // Create the initial neural networks
        generationNetworks = ga.CreateNetworks();
        raceManager.Reset();

        FitnessTracker.totLength = 0;
        for(int i = 0; i < LoadTrackManager.instance.selectedTrackNames.Count; i++)
        {
            trackManagers.Add(Instantiate(trackPrefab).GetComponent<TrackManager>());
            trackManagers[i].Initialize(LoadTrackManager.instance.selectedTrackNames[i]);
            trackManagers[i].gameObject.SetActive(false);
            
        }
        TrackManager.SetTotalLength();

        // Training loop. Keep the training running as long as desired.
        while (true)
        {
            // This pauses the training 
            yield return StartCoroutine(Pause());

            fitnesses.Clear();
            float sum = 0;

            // add all new networks to the networks list
            raceManager.ResetPlayers();
            for (int individual = 0; individual < generationNetworks.Count; individual++)
            {
                raceManager.AddAIPlayer(individual.ToString(), generationNetworks[individual]);
            }
            yield return StartCoroutine(raceManager.StartRace(true, trackManagers, false));

            // Get the fitnesses of the networks and calculate the sum of all fitnesses
            fitnesses = raceManager.GetFitnesses();
            int bestRacerIndex = 0;
            bool updateTime = false;
            for (int i = 0; i < fitnesses.Count; i++)
            {
                if (fitnesses[i] >= bestFitnessSoFar)
                {
                    bestFitnessSoFar = fitnesses[i];
                    bestRacerIndex = i;
                    updateTime = true;
                    bestGeneration = curGeneration;
                }

                if (fitnesses[i] < 0)
                {
                    fitnesses[i] = 0;
                }
                sum += fitnesses[i];
            }
            float currentBestFitness = 0;
            for (int i = 0; i < fitnesses.Count; i++)
            {
                if (fitnesses[i] >= currentBestFitness)
                {
                    currentBestFitness = fitnesses[i];
                }
            }

            curGeneration++;

            // Restart the simulation if non of the cars has a fitness higher than 0
            if (sum == 0)
            {
                curGeneration = 0;
                InitializeNetworks();
                generationNetworks = ga.CreateNetworks();
                continue;
            }
            float totalTime = raceManager.GetCurrentCompetingCars()[bestRacerIndex].totalTime;
            if (raceManager.GetCurrentCompetingCars()[bestRacerIndex].GetFitnessTracker().finished != LoadTrackManager.instance.selectedTrackNames.Count)
                totalTime = -1;

            // Update the best and average fitness on the UI
            UIController.instance.UpdateUI(bestFitnessSoFar, currentBestFitness, sum / ga.populationSize, curGeneration, totalTime, updateTime);

            // Determine the population for the next generation based on the fitnesses of the current networks
            generationNetworks = ga.DoGeneration(fitnesses, complexify);

            index++;
            if (index > 50)
            {
                index = 0;
                if (complexify && Random.Range(0f, 1f) < 0.3f)
                    complexify = !complexify;
                else if (!complexify)
                    complexify = true;

            }
        }
    }

    // Method that pauses the training
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

    void InitializeNetworks()
    {
        pause = false;
        ga = new GeneticAlgorithm(GA_Parameters.populationSize, GA_Parameters.inputs + 1, GA_Parameters.outputs);
    }

    private void OnDestroy()
    {
        MyThreadPool.DestroyThreadPool();
    }

    private void OnApplicationQuit()
    {
        MyThreadPool.DestroyThreadPool();
    }
}
