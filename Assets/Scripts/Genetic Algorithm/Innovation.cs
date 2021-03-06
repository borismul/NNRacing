﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InnovationType { Perceptron, Connection}
public class Innovation
{
    public InnovationType type;
    public int ID;

    public int percepIn;
    public int percepOut;

    public int percepID;
    public NodeType percepType;
    public Vector2 splitValues;

    public Innovation (int percepIn, int percepOut, InnovationType type, int ID)
    {
        this.percepIn = percepIn;
        this.percepOut = percepOut;
        this.type = type;
        this.ID = ID;
        percepID = 0;
        splitValues = new Vector2(0, 0);
        percepType = NodeType.none;
    }

    public Innovation(NodeGene node, int ID, int percepID)
    {
        this.ID = ID;
        this.percepID = percepID;
        splitValues = node.splitValues;
        percepType = node.type;
        percepIn = -1;
        percepOut = -1;
    }

    public Innovation(int percepIn, int percepOut, InnovationType type, int ID, NodeType percepType, Vector2 splitValues)
    {
        this.percepIn = percepIn;
        this.percepOut = percepOut;
        this.type = type;
        this.ID = ID;
        percepID = 0;
        this.percepType = percepType;
        this.splitValues = splitValues;

    }

}

public class Innovations
{
    List<Innovation> innovations;
    int nextNeuronID;
    int nextInnovationNumber;

    public Innovations()
    {
        nextNeuronID = 0;
        nextInnovationNumber = 0;

        innovations = new List<Innovation>();
    }

    public Innovations(List<ConnectionGene> links, List<NodeGene> perceptrons)
    {
        nextNeuronID = 0;
        nextInnovationNumber = 0;

        innovations = new List<Innovation>();

        for(int i = 0; i < perceptrons.Count; i++)
        {
            innovations.Add(new Innovation(perceptrons[i], nextInnovationNumber, nextNeuronID));
            nextInnovationNumber++;
            nextNeuronID++;
        }

        for (int i = 0; i < links.Count; i++)
        {
            innovations.Add(new Innovation(links[i].from, links[i].to, InnovationType.Connection, nextInnovationNumber));
            nextInnovationNumber++;
        }
    }

    public void AddInnovation(NodeGene perceptron)
    {
        innovations.Add(new Innovation(perceptron, nextInnovationNumber, nextNeuronID));
        nextInnovationNumber++;
        nextNeuronID++;
    }

    public int CheckInnovation(int from, int to, InnovationType type)
    {
        for(int i = 0; i < innovations.Count; i++)
        {
            if (innovations[i].percepIn == from && innovations[i].percepOut == to && innovations[i].type == type)
            {
                return innovations[i].ID;
            }
        }

        return -1;
    }

    public int CheckInnovation(NodeGene perceptron)
    {
        for (int i = 0; i < innovations.Count; i++)
        {
            if (innovations[i].percepID == perceptron.ID)
                return innovations[i].ID;
        }

        return -1;
    }

    public int CreateNewInnovation(int from, int to, InnovationType type)
    {
        Innovation innovation = new Innovation(from, to, type, nextInnovationNumber);
        if (type == InnovationType.Perceptron)
        {
            innovation.percepID = nextNeuronID;
            nextNeuronID++;
        }

        innovations.Add(innovation);
        nextInnovationNumber++;

        return nextNeuronID - 1;
    }

    public int CreateNewInnovation(int from, int to , InnovationType innovType, NodeType nodeType, Vector2 splitValues)
    {
        Innovation innovation = new Innovation(from, to, innovType, nextInnovationNumber, nodeType, splitValues);

        if(innovType == InnovationType.Perceptron)
        {
            innovation.percepID = nextNeuronID;
            nextNeuronID++;
        }

        innovations.Add(innovation);
        nextInnovationNumber++;

        return (nextNeuronID - 1);
    }

    public Genome CreateNeuronFromID(int ID, Genome genome)
    {

        for(int i = 0; i < innovations.Count; i++)
        {
            if(innovations[i].percepID == ID)
            {
                genome.AddNodeGene(innovations[i].percepID, innovations[i].percepType, false, innovations[i].splitValues, Random.Range(-1, 1));
                return genome;
            }
        }

        genome.AddNodeGene(0, NodeType.Hidden, false, Vector2.zero, Random.Range(-1, 1));
        return genome;
    }

    public int GetNeuronID(int inv)
    {
        return innovations[inv].percepID;
    }

    public void Flush()
    {
        innovations.Clear();
    }

    public int NextNumber(int num = 0)
    {
        nextInnovationNumber += num;

        return nextInnovationNumber;
    }
}
