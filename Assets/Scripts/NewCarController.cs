using UnityEngine;
using System.Collections.Generic;

public class NewCarController : MonoBehaviour {

    public float accSpeed;
    public float maxSpeed;
    public float turnSpeed;

    public LayerMask mask;

    float acc;
    float turn;
    float curRotAngle;
    float prevRotAngle;

    Vector3 velocity;

    BoxCollider col;

    Vector3 prevPos;

    void Awake()
    {
        col = GetComponent<BoxCollider>();
        prevPos = transform.position;
    }

    public bool UpdateCar(float deltaTime)
    {
        Move(deltaTime);
        if (HasCollision())
            return false;

        return true;
    }

    void Move(float deltaTime)
    {
        Vector3 accVector = acc * accSpeed * transform.forward * deltaTime;
        velocity += accVector;
        if (velocity.magnitude > maxSpeed)
        {
            velocity /= (velocity.magnitude + 0.001f);
            velocity *= maxSpeed;
        }
        curRotAngle += turn * turnSpeed * deltaTime;
        Quaternion rot = Quaternion.Euler(0, curRotAngle, 0);
        float rotDiff = curRotAngle - prevRotAngle;
        Quaternion rotVector = Quaternion.Euler(0, rotDiff, 0);
        velocity = rotVector * velocity;
        transform.position += velocity * deltaTime;
        transform.rotation = rot;

        prevRotAngle = curRotAngle;
    }

    public bool HasCollision()
    {
        Vector3 size = col.size * 1.05f;

        Vector3 TR = transform.TransformPoint(new Vector3(size.x / 2, col.center.y, size.z / 2));
        RaycastHit hit;
        if (Physics.Raycast(transform.position, TR - transform.position, out hit, (TR - transform.position).magnitude, mask))
            return true;
        Debug.DrawLine(transform.position, prevPos, Color.blue, 1);

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
        prevPos = transform.position;

        return false;
    }

    public void SetOuput(float acc, float turn)
    {
        this.acc = acc;
        this.turn = turn;
    }

    public void Reset()
    {
        transform.position = DistanceTracker.instance.currentPoint.transform.position;
        transform.rotation = Quaternion.LookRotation(DistanceTracker.instance.nextPoint.transform.position - DistanceTracker.instance.currentPoint.transform.position);
        curRotAngle = transform.rotation.eulerAngles.y;
        prevRotAngle = transform.rotation.eulerAngles.y;
        prevPos = transform.position;
        velocity = Vector3.zero;
        acc = 0;
        turn = 0;
    }

    public List<float> GetInput(int n)
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity);
        List<float> input = new List<float>();
        float numberVisionPoints = n - 1;
        float angle = 2 * Mathf.PI / numberVisionPoints;
        for (int i = 0; i < numberVisionPoints; i++)
        {
            float curdist;
            if (Physics.Raycast(transform.position, (Mathf.Cos(angle * i)  * transform.right + Mathf.Sin(angle * i) * transform.forward), out hit, Mathf.Infinity))
                curdist = Mathf.Clamp(hit.distance, 0, 100) / 100;
            else
                curdist = 100;

            //Debug.DrawRay(transform.position, (Mathf.Cos(angle * i)  * transform.right + Mathf.Sin(angle * i)  * transform.forward) * hit.distance, Color.red);

            input.Add(curdist);
        }

        input.Add(velocity.magnitude);
        return input;

    }
}
