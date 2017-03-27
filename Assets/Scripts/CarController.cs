using UnityEngine;
using System.Collections;

public class CarController : MonoBehaviour {

    float accelerate;
    float turn;


    float accSpeed = 10;
    float turnSpeed = 80;

    float maxSpeed = 15;

    public Vector3 velocity;
    float rotValue = 0;
    float prevRot;

    Vector3 prevPos;

    public bool isHumanControlled = true;

    Vector3 startPos;
    Quaternion startRot;

    BoxCollider col;

    public LayerMask mask;


	// Use this for initialization
	void Start ()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rotValue = transform.rotation.eulerAngles.y;
        col = GetComponent<BoxCollider>();
        prevPos = transform.position;
    }

    // Update is called once per frame
    void Update ()
    {
        GetInputs();
	}

    void FixedUpdate()
    {
        Move();
        if (HasCollision())
        {
            Reset();
            print("collision");
        }
    }

    void Move()
    {
        Vector3 acc = accelerate * accSpeed * transform.forward * Time.fixedDeltaTime;
        velocity += acc;
        if(velocity.magnitude > maxSpeed)
        {
            velocity /= (velocity.magnitude+0.001f);
            velocity *= maxSpeed;
        }
        rotValue += turn * turnSpeed * Time.fixedDeltaTime;
        Quaternion rot = Quaternion.Euler(0, rotValue, 0);
        float rotDiff = rotValue - prevRot;
        Quaternion rotVector = Quaternion.Euler(0, rotDiff, 0);
        velocity = rotVector * velocity;
        transform.position += velocity * Time.fixedDeltaTime;
        transform.rotation = rot;

        prevRot = rotValue;

        //if ((transform.position - DistanceTracker.instance.currentPoint.transform.position).magnitude < 0.1f && (Brain.time - Brain.instance.measureTime) > 3)
        //    Brain.instance.StopTest(true);
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
        if (isHumanControlled)
            return;
        accelerate = acc;
        this.turn = turn;
    }

    public bool HasCollision()
    {
        Vector3 size = col.size * 1.05f;

        Vector3 TR = transform.TransformPoint(new Vector3(size.x / 2, col.center.y, size.z / 2));
        RaycastHit hit;
        if (Physics.Raycast(transform.position, TR - transform.position, out hit, (TR - transform.position).magnitude, mask))
            return true;

        //Debug.DrawRay(transform.position, TR - transform.position, Color.red, Mathf.Infinity);

        Vector3 TL = transform.TransformPoint(new Vector3(size.x / 2, col.center.y, -size.z / 2));

        if (Physics.Raycast(transform.position, TL - transform.position, out hit, (TL - transform.position).magnitude, mask))
            return true;


        //Debug.DrawRay(transform.position, TL - transform.position, Color.red, Mathf.Infinity);

        Vector3 BL = transform.TransformPoint(new Vector3(-size.x / 2, col.center.y, -size.z / 2));

        if (Physics.Raycast(transform.position, BL - transform.position, out hit, (BL - transform.position).magnitude, mask))
            return true;

        //Debug.DrawRay(transform.position, BL - transform.position, Color.red, Mathf.Infinity);

        Vector3 BR = transform.TransformPoint(new Vector3(-size.x / 2, col.center.y, size.z / 2));

        if (Physics.Raycast(transform.position, BR - transform.position, out hit, (BR - transform.position).magnitude, mask))
            return true;

        //Debug.DrawRay(transform.position, BR - transform.position, Color.red, Mathf.Infinity);

        return false;
    }

    public void Reset()
    {
        transform.position = DistanceTracker.instance.currentPoint.transform.position;
        transform.rotation = Quaternion.LookRotation(DistanceTracker.instance.nextPoint.transform.position - DistanceTracker.instance.currentPoint.transform.position);
        rotValue = transform.rotation.eulerAngles.y;
        velocity = Vector3.zero;
        prevRot = transform.rotation.eulerAngles.y;
        prevPos = transform.position;

        accelerate = 0;
        turn = 0;

    }
}
