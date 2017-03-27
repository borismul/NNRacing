using UnityEngine;
using System.Collections;

public class PerceptronConnection
{
    public Perceptron inputPerceptron;
    public Perceptron outputPerceptron;

    public float weight;

    public PerceptronConnection(Perceptron outPerceptron, Perceptron inPerceptron, float weight)
    {
        this.inputPerceptron = inPerceptron;
        this.outputPerceptron = outPerceptron;
        this.weight = weight;
    }

    public void RemoveConnection()
    {
        if(inputPerceptron != null)
            inputPerceptron.outputConnections.Remove(this);
        if (outputPerceptron != null)
            outputPerceptron.inputConnections.Remove(this);
    }

    public bool Equals(PerceptronConnection other)
    {
        if(inputPerceptron == null)
        {
            return ((other.inputPerceptron == null && outputPerceptron.Equals(other.outputPerceptron)) || other.outputPerceptron == null && outputPerceptron.Equals(other.inputPerceptron));
        }
        return (inputPerceptron.Equals(other.inputPerceptron) && outputPerceptron.Equals(other.outputPerceptron)) || (inputPerceptron.Equals(other.outputPerceptron) && outputPerceptron.Equals(other.inputPerceptron));
    }
}
