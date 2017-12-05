using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{

    public GameObject panel;
    public GameObject perceptron;
    public GameObject link;
    public GameObject loopLink;

    Genome genome;
    NeuralNetwork network;

    Innovations innovations;
    // Use this for initialization
    void Start()
    {
        genome = new Genome(0, 8, 4, new Genome.MutParameters());
        network = genome.CreateNetwork();

        network.VisualizeNetwork(panel.GetComponent<RectTransform>(), perceptron, link, loopLink, false);

        innovations = new Innovations(genome.GetConnectionGenes(), genome.GetPerceptronGenes());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            genome.AddNeuron(1, ref innovations, 3);
            network.DestroyNetwork();
            network = genome.CreateNetwork();
            network.VisualizeNetwork(panel.GetComponent<RectTransform>(), perceptron, link, loopLink, false);
            print(genome.GetPerceptronGenes().Count);
        }
        if (Input.GetMouseButtonDown(1))
        {
            genome.AddLink(1, .5f, ref innovations, 3, 3);
            network.DestroyNetwork();
            network = genome.CreateNetwork();
            network.VisualizeNetwork(panel.GetComponent<RectTransform>(), perceptron, link, loopLink, false);
            print("done");
        }
    }
}
