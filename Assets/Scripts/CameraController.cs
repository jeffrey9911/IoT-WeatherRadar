using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : MonoBehaviour
{
    public Transform centerPoint;
    public Transform centerLookAt;

    public float radius = 5f;
    public float speed = 1.0f;

    private float rotAngle = 0f;


    private void Update()
    {
        float x = centerPoint.position.x + radius * Mathf.Cos(rotAngle);
        float z = centerPoint.position.z + radius * Mathf.Sin(rotAngle);

        transform.position = new Vector3(x, transform.position.y, z);

        rotAngle += speed * Time.deltaTime;

        transform.LookAt(centerLookAt);

        if(rotAngle >= Mathf.PI * 2f)
        {
            rotAngle = 0.0f;
        }
    }
}
