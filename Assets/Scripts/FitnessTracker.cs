using UnityEngine;
using System.Collections.Generic;
using B83.ExpressionParser;

public class FitnessTracker : MonoBehaviour
{

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
        discreteDistance += trackManager.CheckSetDone(carController.transform.position);

        distance = discreteDistance - trackManager.currentTrack.CheckDistance(carController.transform.position);
        this.time += time;

        if ((laps > 0 && !trackManager.currentTrack.hasLaps) || laps == GA_Parameters.laps)
        {
            distance = discreteDistance;
            return false;

        }

        if (trackManager.currentTrack.CheckDistance(carController.transform.position) > 20)
        {
            if (!stopAtCrash)
                carController.Reset();
            else
                return false;
        }



        return true;
    }

    public double[] CreateInputArray()
    {
        List<double> inputs = new List<double>();
        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i] == "x")
                inputs.Add(distance);
            else if (keys[i] == "t")
                inputs.Add(time);
            else if (keys[i] == "c")
                inputs.Add(crashes);
            else if (keys[i] == "l")
                inputs.Add(totalLaps);
        }
        return inputs.ToArray();
    }

    public float GetFitness()
    {
        double[] inputs = CreateInputArray();

        float fitness = (float)fitnessDelegate.Invoke(inputs);
        return Mathf.Clamp(fitness, 0, Mathf.Infinity);
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
