using UnityEngine;
using System.Collections.Generic;
using B83.ExpressionParser;

public class FitnessTracker : MonoBehaviour {

    public static bool[] isInput;

    public float discreteDistance;

    public float distance;
    public float time;
    public int crashes;
    public int laps;
    public int totalLaps;

    public static List<string> keys;

    public static ExpressionDelegate fitnessDelegate;

    public TrackManager trackManager;

    public CarController carController;

    float currentFitness;

    void Awake()
    {
        trackManager = GetComponent<TrackManager>();
        carController = GetComponent<CarController>();
    }

    public bool UpdateFitness(float time, bool stopAtCrash)
    {
        if ((laps > 0 && !trackManager.currentTrack.hasLaps) || laps == GeneticAlgorithm.instance.laps)
        {
            return false;
        }

        if (trackManager.currentTrack.CheckDistance(carController.transform.position) > 20)
        {
            if (!stopAtCrash)
                carController.Reset();
            else
                return false;
        }

        discreteDistance += trackManager.CheckSetDone(carController.transform.position);

        distance = discreteDistance - trackManager.currentTrack.CheckDistance(carController.transform.position);
        this.time += time;

        return true;
    }

    public double[] CreateInputArray()
    {
        double[] inputs = new double[4];
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i] == "x")
                inputs[i] = distance;
            else if (keys[i] == "t")
                inputs[i] = time;
            else if (keys[i] == "c")
                inputs[i] = crashes;
            else if (keys[i] == "l")
                inputs[i] = totalLaps;
        }
        return inputs;
    }

    public float GetFitness()
    {
        List<double> inputs = new List<double>();
        double[] maybeInputs = CreateInputArray();

        for (int i = 0; i < isInput.Length; i++)
        {
            if (isInput[i])
                inputs.Add(maybeInputs[i]);
        }

        float fitness = (float)fitnessDelegate.Invoke(inputs.ToArray());
        return fitness;
    }

    public void AddCrash()
    {
        crashes++;
    }

    public void SaveCurrentFitness()
    {
        totalLaps += laps;
        currentFitness += GetFitness();
    }

    public void Reset()
    {
        discreteDistance = 0;
        distance = 0;
        time = 0;
        laps = 0;
        crashes = 0;
        currentFitness = 0;
        totalLaps = 0;
    }

}
