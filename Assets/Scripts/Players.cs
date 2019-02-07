using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer {

    public string name;
    public KeyCode[] controls;
    public int chosenCar;

    public HumanPlayer(string name, KeyCode[] controls)
    {
        this.name = name;
        this.controls = controls;
    }

}

public class AIPlayer
{
    public string name;
    public NeuralNetwork network;
    public int assignedCar;

    public AIPlayer(string name, NeuralNetwork network)
    {
        this.name = name;
        this.network = network;
    }

    public void SetPlayer(string name, NeuralNetwork network)
    {
        this.name = name;
        this.network = network;
    }
}
