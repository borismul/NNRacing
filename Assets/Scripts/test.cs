//using UnityEngine;
//using System.Collections.Generic;
//using System.Collections;
//using UnityEngine.UI;

//public class test : MonoBehaviour
//{
//    public GameObject perceptron;
//    public GameObject hSpace;
//    public GameObject vSpace;
//    public GameObject line;

//    NeuralNetwork network;
//    NeuralNetwork network2;
//    NeuralNetwork network3;

//    Gene gene1;
//    Gene gene2;
//    Gene gene3;

//    void Start()
//    {
//        network = new NeuralNetwork(10, 0.1f, 3, 8, 7, 6, 3);
//        gene1 = Gene.Encode(network);
//        network2 = new NeuralNetwork(10, 0f, 3, 4, 6, 7, 3);
//        gene2 = Gene.Encode(network2);
//        network.GetOutput(new List<float>() { 0.4f, 0.3f, 0.2f });

//        //network.VisualizeNetwork(perceptron, hSpace, vSpace, line, "mom");
//        network2.GetOutput(new List<float>() { 0.4f, 0.3f, 0.2f});

//        //network2.VisualizeNetwork(perceptron, hSpace, vSpace, line,"dad");

//        network3 = Gene.MakeChild(gene2, gene1, 0f);

//        gene3 = Gene.Encode(network3);

//        network3.GetOutput(new List<float>() { 0.4f, 0.3f, 0.2f });

//        network3.VisualizeNetwork(perceptron, hSpace, vSpace, line, "child");


//    }
//    void Update()
//    {
//        //network2.GetOutput(new List<float>() { Mathf.Cos(Time.realtimeSinceStartup * 200 * Mathf.Deg2Rad), Mathf.Cos(Time.realtimeSinceStartup * 30 * Mathf.Deg2Rad), Mathf.Cos(Time.realtimeSinceStartup * 50 * Mathf.Deg2Rad), Mathf.Cos(Time.realtimeSinceStartup * 150 * Mathf.Deg2Rad), Mathf.Sin(Time.realtimeSinceStartup*100 * Mathf.Deg2Rad) });
//    }
//}


