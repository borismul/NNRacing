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
    public float TotalLapDistance;

    public static List<string> keys;

    public static ExpressionDelegate fitnessDelegate;

    public CarTrackController trackManager;

    public CarController carController;

    float currentFitness;

    void Awake()
    {
        trackManager = GetComponent<CarTrackController>();
        carController = GetComponent<CarController>();
    }

    public bool UpdateFitness(float time, bool stopAtCrash, Vector3 position)
    {
        float added = trackManager.CheckSetDone(position);
        discreteDistance += added;

        distance = discreteDistance - trackManager.CheckDistance(position, false);
        this.time += time;

        if ((laps > 0 && !trackManager.track.hasLaps) || laps == GA_Parameters.laps)
        {

            carController.SetFinished();
            distance = discreteDistance;
            return false;

        }

        if (trackManager.CheckDistance(position, true) > 20)
        {
            if (!stopAtCrash)
                carController.Reset(true, true);
            else
            {
                return false;
            }
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
                inputs.Add(laps);
            else if (keys[i] == "L")
                inputs.Add(trackManager.track.length);

        }

        return inputs.ToArray();
    }

    public float GetFitness()
    {
        if (time == 0 && distance == 0)
            time = 1;

        double[] inputs = CreateInputArray();
        float fitness = (float)fitnessDelegate.Invoke(inputs);
        return Mathf.Clamp(fitness, 0, Mathf.Infinity);
    }

    public void AddCrash()
    {
        crashes++;
    }

    public void Reset()
    {
        discreteDistance = 0;
        distance = 0;
        time = 0;
        currentFitness = 0;
        totalLaps = 0;
        TotalLapDistance = 0;
        laps = 0;
        crashes = 0;
        
    }

    public float GetFinishTime()
    {
        if (laps == GA_Parameters.laps)
            return time;
        else
            return -1;
    }

}
