using UnityEngine;
using System.Collections.Generic;

public class Perceptron
{
    public List<PerceptronConnection> inputConnections;
    public List<PerceptronConnection> outputConnections;
    public float theta;
    NeuralNetwork network;

    public int layer;
    public int number;
    public int absoluteNumber;

    public float output = 0;

    public Perceptron(int layer, int number, int absoluteNumber, NeuralNetwork network)
    {
        this.inputConnections = new List<PerceptronConnection>();
        this.outputConnections = new List<PerceptronConnection>();
        this.layer = layer;
        this.number = number;
        this.network = network;
        this.absoluteNumber = absoluteNumber;
        theta = Random.Range(-1,1f);
    }

    public Perceptron(int layer, int number, int absoluteNumber, float theta, NeuralNetwork network)
    {
        this.inputConnections = new List<PerceptronConnection>();
        this.outputConnections = new List<PerceptronConnection>();
        this.layer = layer;
        this.number = number;
        this.network = network;
        this.absoluteNumber = absoluteNumber;
        this.theta = theta;
    }

    public void LinkPerceptrons(Perceptron outPerceptron, out bool success)
    {
        PerceptronConnection connection;
        if (outPerceptron.number != number || outPerceptron.layer != layer)
        {
            connection = new PerceptronConnection(outPerceptron, this, 0);
        }
        else
        {
            connection = new PerceptronConnection(outPerceptron, null, 0);
        }

        bool containsConnection = false;

        foreach (PerceptronConnection connectionCheck in inputConnections)
        {
            if (connectionCheck.Equals(connection))
            {
                containsConnection = true;
            }
        }

        foreach (PerceptronConnection connectionCheck in outputConnections)
        {
            if (connectionCheck.Equals(connection))
            {
                containsConnection = true;
            }
        }

        if (!containsConnection)
        {
            outPerceptron.inputConnections.Add(connection);
            outputConnections.Add(connection);
        }

        success = !containsConnection;
    }

    public void LinkPerceptrons(Perceptron outPerceptron)
    {
        PerceptronConnection connection;
        if (outPerceptron.number != number || outPerceptron.layer != layer)
        {
            connection = new PerceptronConnection(outPerceptron, this, 0);
        }
        else
        {
            connection = new PerceptronConnection(outPerceptron, null, 0);
        }

        bool containsConnection = false;

        foreach (PerceptronConnection connectionCheck in inputConnections)
        {
            if (connectionCheck.Equals(connection))
            {
                containsConnection = true;
            }
        }

        foreach (PerceptronConnection connectionCheck in outputConnections)
        {
            if (connectionCheck.Equals(connection))
            {
                containsConnection = true;
            }
        }

        if (!containsConnection)
        {
            outPerceptron.inputConnections.Add(connection);
            if (outPerceptron.number != number || outPerceptron.layer != layer)
            {
                outputConnections.Add(connection);
            }
        }
    }

    public void LinkPerceptrons(Perceptron outPerceptron, float weight)
    {
        PerceptronConnection connection;
        if (outPerceptron.number != number || outPerceptron.layer != layer)
        {
            connection = new PerceptronConnection(outPerceptron, this, weight);
        }
        else
        {
            connection = new PerceptronConnection(outPerceptron, null, weight);
        }

        bool containsConnection = false;

        foreach (PerceptronConnection connectionCheck in inputConnections)
        {
            if (connectionCheck.Equals(connection))
            {
                containsConnection = true;
            }
        }

        foreach (PerceptronConnection connectionCheck in outputConnections)
        {
            if (connectionCheck.Equals(connection))
            {
                containsConnection = true;
            }
        }

        if (!containsConnection)
        {
            outPerceptron.inputConnections.Add(connection);
            if (outPerceptron.number != number || outPerceptron.layer != layer)
            {
                outputConnections.Add(connection);
            }
        }
    }

    public void InitializeWeights()
    {
        for (int i = 0; i < inputConnections.Count; i++)
        {
            float curWeight = Random.Range(-1f,1f);
            inputConnections[i].weight = curWeight;
        }

        NormalizeWeights();
    }

    public void NormalizeWeights()
    {
        //float sum = 0;
        //for (int i = 0; i < inputConnections.Count; i++)
        //    sum += inputConnections[i].weight;

        //for (int i = 0; i < inputConnections.Count; i++)
        //{
        //    inputConnections[i].weight /= sum;
        //}
    }

    public void DetermineOutput(float input)
    { 
        output = Step(input) * inputConnections[0].weight;
    }

    public void DetermineOutput()
    {
        float sum = 0;
        for (int i = 0; i < inputConnections.Count; i++)
        {
            PerceptronConnection connection = inputConnections[i];
            sum += connection.inputPerceptron.output * connection.weight;
        }

        output = Step(sum);
    }

    public bool Equals(Perceptron other)
    {
        if (other == null)
            return false;

        return (layer == other.layer && number == other.number);
    }

    float Sigmoid(float x)
    {
        return (1 / (1 + Mathf.Exp(-x))) - theta;
    }

    float Step(float x)
    {
        if (x - theta > 0)
            return 1;

        return -1;

    }
}
