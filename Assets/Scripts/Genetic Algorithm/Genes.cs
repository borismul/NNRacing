using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Sensor, Hidden, Output, Bias, Modulatory, none}
public class NodeGene
{
    public int ID;
    public NodeType type;
    public float actResponse;
    public bool recurrent;
    public Vector2 splitValues;

    public NodeGene(NodeType type,
                    int ID,
                    Vector2 splitValues,
                    bool recurrent = false)
    {
        this.ID = ID;
        this.type = type;
        this.recurrent = recurrent;
        this.splitValues = splitValues;
        this.actResponse = Random.Range(-1, 1);
    }

    public NodeGene(NodeGene gene)
    {
        this.ID = gene.ID;
        this.type = gene.type;
        this.recurrent = gene.recurrent;
        this.splitValues = gene.splitValues;
        this.actResponse = gene.actResponse;
    }

    public NodeGene(NodeType type,
                int ID,
                Vector2 splitValues,
                float actResponse,
                bool recurrent = false)
    {
        this.ID = ID;
        this.type = type;
        this.recurrent = recurrent;
        this.splitValues = splitValues;
        this.actResponse = actResponse;
    }

    public void SetGene(NodeGene gene)
    {
        this.ID = gene.ID;
        this.type = gene.type;
        this.recurrent = gene.recurrent;
        this.splitValues = gene.splitValues;
        this.actResponse = gene.actResponse;
    }
}

public class ConnectionGene
{
    public int from;
    public int to;
    public float weight;
    public bool enabled;
    public bool recurrent;
    public int innovNum;

    public ConnectionGene(  int from,
                            int to,
                            bool enabled,
                            int innovNum,
                            float weight, 
                            bool recurrent = false)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
        this.enabled = enabled;
        this.innovNum = innovNum;
        this.recurrent = recurrent;
    }

    public ConnectionGene(ConnectionGene gene)
    {
        this.from = gene.from;
        this.to = gene.to;
        this.weight = gene.weight;
        this.enabled = gene.enabled;
        this.innovNum = gene.innovNum;
        this.recurrent = gene.recurrent;
    }

    public void SetGene(ConnectionGene gene)
    {
        this.from = gene.from;
        this.to = gene.to;
        this.weight = gene.weight;
        this.enabled = gene.enabled;
        this.innovNum = gene.innovNum;
        this.recurrent = gene.recurrent;
    }
}