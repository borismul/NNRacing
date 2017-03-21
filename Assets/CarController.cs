using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour {

    int accelerate;
    int turn;

    Rigidbody r;

    float accSpeed = 10;
    float decSpeed = 40;
    float turnSpeed = 50;

    float maxSpeed = 5;

    public Vector3 velocity;
    float rotValue = 0;
    float prevRot;

    public bool isHumanControlled = true;

	// Use this for initialization
	void Start ()
    {
        r = GetComponent<Rigidbody>();
        rotValue = transform.rotation.eulerAngles.y;
	}
	
	// Update is called once per frame
	void Update ()
    {
        GetInputs();
        Move();
	}

    void Move()
    {
        Vector3 acc = accelerate * accSpeed * transform.forward * Time.deltaTime;
        velocity += acc;
        if(velocity.magnitude > maxSpeed)
        {
            velocity /= velocity.magnitude;
            velocity *= maxSpeed;
        }
        rotValue += turn * turnSpeed * Time.deltaTime;
        Quaternion rot = Quaternion.Euler(0, rotValue, 0);
        float rotDiff = rotValue - prevRot;
        Quaternion rotVector = Quaternion.Euler(0, rotDiff, 0);
        velocity = rotVector * velocity;
        transform.position += velocity * Time.deltaTime;
        transform.rotation = rot;

        prevRot = rotValue;
    }

    void GetInputs()
    {
        if (!isHumanControlled)
            return;
        // accelerate
        if (Input.GetKey(KeyCode.UpArrow))
            accelerate = 1;
        else if (Input.GetKey(KeyCode.DownArrow))
            accelerate = -1;
        else
            accelerate = 0;

        // turn
        if (Input.GetKey(KeyCode.LeftArrow))
            turn = -1;
        else if (Input.GetKey(KeyCode.RightArrow))
            turn = 1;
        else
            turn = 0;
    }

    public void SetInputs(float acc, float turn)
    {
        if (acc > 0.5f)
            accelerate = 1;
        else
            accelerate = -1;

        if (turn > 0.5f)
            this.turn = 1;
        else
            this.turn = -1;
    }
}
