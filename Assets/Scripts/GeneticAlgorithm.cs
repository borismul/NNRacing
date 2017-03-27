using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneticAlgorithm : MonoBehaviour
{
    public int numGenerations;
    public int populationSize;
    public int carsPerFrame;
    public int carUpdateRate;

    public float simulationTime;
    public int fps;

    public float NNWeightsMutationChance;
    public float NNMutationChance;
    public float cloneChance;
   
    public NewCarController carController;

    public List<Generation> generations = new List<Generation>();
    List<float> input;

    public NeuralNetwork currentNetwork;
    float currentTime;

    public static GeneticAlgorithm instance;

    float maxFitness;
    float avgFitness;

    public bool stop = false;

    public bool pause;
    public bool isPaused;

    public int curGeneration;

    void Awake()
    {
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
        for(int i = 0; i < numGenerations; i++)
        {
            curGeneration = i;
            while (pause || isPaused)
            {
                if (pause)
                {
                    pause = false;
                }
                isPaused = true;
                yield return null;
            }

            maxFitness = 0;
            for(int j = 0; j < generations[i].networks.Count; j++)
            {
                currentNetwork = generations[i].networks[j];
                currentNetwork.Fitness = 0;

                DistanceTracker.instance.CompleteReset();
                carController.Reset();
                for (int tracks = 0; tracks < 2; tracks++)
                {
                    currentTime = 0;
                    stop = false;
                    for (int k = 0; k < simulationTime; k++)
                    {
                        for (int l = 0; l < fps; l++)
                        {
                            DistanceTracker.instance.UpdateDistance(currentTime);
                            currentTime += 1f / (float)fps;
                            SetOutput();
                            if (!carController.UpdateCar(1f / (float)fps))
                            {
                                DistanceTracker.instance.Penalty(0.9f);
                                carController.Reset();
                            }
                            else if(!DistanceTracker.instance.UpdateDistance(currentTime) || stop)
                            {
                                stop = true;
                                break;
                            }

                            if (l != 0 && carUpdateRate != 0 && l % carUpdateRate == 0)
                            {
                                for(int m = 0; m < (float)60/fps / carUpdateRate; m++)
                                {
                                    //UIController.instance.UpdateUI(maxFitness, i, j, curFitness + Mathf.Clamp(DistanceTracker.instance.distance, 0, Mathf.Infinity), currentTime);
                                    yield return null;
                                    if (carUpdateRate == 0)
                                        break;
                                }
                            }
                        }

                        if (stop)
                            break;
                    }

                    StopRace();
                }
                if (carsPerFrame == 0 || j % carsPerFrame == 0)
                {
                    UIController.instance.UpdateUI(maxFitness, i, j, 0, 0);
                    yield return null;
                }
            }
            UIController.instance.UpdateUI(maxFitness, avgFitness, i, 0, 0, 0, true);
            CreateNextGen(i);
        }
    }

    IEnumerator Race()
    {
        currentTime = 0;
        stop = false;
        for (int k = 0; k < simulationTime; k++)
        {
            for (int l = 0; l < fps; l++)
            {
                DistanceTracker.instance.UpdateDistance(currentTime);
                currentTime += 1f / (float)fps;
                SetOutput();
                if ((!carController.UpdateCar(1f / (float)fps)) || stop)
                {
                    stop = true;
                    break;
                }

                if (l != 0 && carUpdateRate != 0 && l % carUpdateRate == 0)
                    yield return null;
            }

            if (stop)
                break;
        }

        StopRace();

    }

    void InitializeNetworks()
    {
        List<NeuralNetwork> networks = new List<NeuralNetwork>();

        for (int i = 0; i < populationSize; i++)
        {
            networks.Add(new NeuralNetwork(7, 8,8, 4));
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

        networks.AddRange(generations[gen].networks.GetRange(0, populationSize / 10));


        // Produce children
        for (int i = 0; i < Mathf.RoundToInt((populationSize *(9f/10))); i++)
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

    void GetInput()
    {
        input = carController.GetInput(currentNetwork.GetLayers()[0].Count);
        input = input.GetRange(0, currentNetwork.GetLayers()[0].Count);
    }

    public void SetOutput()
    {
        GetInput();
        List<float> output = currentNetwork.GetOutput(input);
        List<List<Perceptron>> iets = currentNetwork.GetLayers();

        try
        {
            for (int i = 0; i < output.Count; i++)
            {
                if (output[i] > 0f)
                    output[i] = 1;
                else
                    output[i] = -1;
            }


            carController.SetOuput(output[0] - output[1], output[2] - output[3]);
        }
        catch (System.Exception e)
        {
            StopRace();
        }

        //try
        //{
        //    for (int i = 0; i < output.Count; i++)
        //{
        //        output[i] -= 0.5f;
        //        output[i] *= 2;
        //}


        //    carController.SetOuput(output[0], output[1]);
        //}
        //catch(System.Exception e)
        //{
        //    StopRace();
        //}

    }

    public void StopRace()
    {
        currentNetwork.Fitness = DistanceTracker.instance.distance;

        if (currentNetwork.Fitness > maxFitness)
            maxFitness = currentNetwork.Fitness;

        DistanceTracker.instance.NextTrack();
        carController.Reset();

        stop = true;
    }
}
