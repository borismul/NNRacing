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
                    bool recurrent = false,
                    float actResponse = 0.5f)
    {
        this.ID = ID;
        this.type = type;
        this.recurrent = recurrent;
        this.splitValues = splitValues;
        this.actResponse = actResponse;
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
}