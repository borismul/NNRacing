using UnityEngine;
using System.Collections.Generic;

public class Brain : MonoBehaviour {

    float distForw;
    float distLeft;
    float distRight;
    float velocity;

    float acc;
    float turn;

    NeuralNetwork network;

    public GameObject perceptron;
    public GameObject hSpace;
    public GameObject vSpace;
    public GameObject line;


    // Use this for initialization
    void Start () {
        network = new NeuralNetwork(4, 7, 2);
        network.VisualizeNetwork(perceptron, hSpace, vSpace, line, "FirstTry");
    }

    // Update is called once per frame
    void Update ()
    {
        GetInput();
        GetOutput();
        GetComponent<CarController>().SetInputs(acc, turn);
	}

    void GetInput()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity);
        distForw = Mathf.Clamp(hit.distance, 0, 30)/30;

        Physics.Raycast(transform.position, transform.forward + (2f / 3f) * transform.right, out hit, Mathf.Infinity);
        distRight = Mathf.Clamp(hit.distance, 0, 30) / 30;
        Debug.DrawRay(transform.position, transform.forward + (2f / 3f) * transform.right);

        Physics.Raycast(transform.position, transform.forward - (2f / 3f) * transform.right, out hit, Mathf.Infinity);
        distLeft = Mathf.Clamp(hit.distance, 0, 30) / 30;
        Debug.DrawRay(transform.position, transform.forward - (2f/3f) * transform.right);

        velocity = GetComponent<CarController>().velocity.magnitude/5;
    }

    void GetOutput()
    {
        List<float> output = network.GetOutput(new List<float>() { distForw, distLeft, distRight, velocity });

        acc = output[0];
        turn = output[1];
    }
}
