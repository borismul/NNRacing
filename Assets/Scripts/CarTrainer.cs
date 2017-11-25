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

    // A list of neural networks which is currently running
    public List<NeuralNetwork> generationNetworks = new List<NeuralNetwork>();

    // List of fitnesses in this generation
    List<float> fitnesses = new List<float>();

    void Awake()
    {
        Application.targetFrameRate = 60;
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
        InitializeNetworks();
        StartCoroutine("Train");
    }

    // train the neural networks environment
    IEnumerator Train()
    {
        // Create the initial neural networks
        generationNetworks = ga.CreateNetworks();

        // Training loop. Keep the training running as long as desired.
        while (true)
        {
            // This pauses the training 
            yield return StartCoroutine(Pause());

            curGeneration++;
            fitnesses.Clear();
            float sum = 0;

            // add all new networks to the networks list
            raceManager.ResetPlayers();
            for (int individual = 0; individual < generationNetworks.Count; individual++)
            {
                raceManager.AddAIPlayer(individual.ToString(), generationNetworks[individual]);
            }
            yield return StartCoroutine(raceManager.StartRace());

            // Get the fitnesses of the networks and calculate the sum of all fitnesses
            fitnesses = raceManager.GetFitnesses();

            for (int i = 0; i < fitnesses.Count; i++)
            {
                if (fitnesses[i] > bestFitnessSoFar)
                    bestFitnessSoFar = fitnesses[i];

                sum += fitnesses[i];
            }

            // Restart the simulation if non of the cars has a fitness higher than 0
            if (sum == 0)
            {
                InitializeNetworks();
                generationNetworks = ga.CreateNetworks();
                continue;
            }

            // Update the best and average fitness on the UI
            UIController.instance.UpdateUI(bestFitnessSoFar, sum / ga.populationSize, curGeneration);

            // Determine the population for the next generation based on the fitnesses of the current networks
            generationNetworks = ga.DoGeneration(fitnesses);
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
        ga = new GeneticAlgorithm(GA_Parameters.populationSize, GA_Parameters.inputs, 1, new int[1] { 10 }, GA_Parameters.outputs);
    }
}
