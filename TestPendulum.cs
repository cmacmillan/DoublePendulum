using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPendulum : MonoBehaviour
{
    public float theta1;
    public float theta1Vel;

    public float theta2;
    public float theta2Vel;

    public float L1=1;
    public float L2=1;

    public float m1=1;
    public float m2=1;

    public float g=-1;

    public GameObject sphere1;
    public GameObject sphere2;

    float GetTheta1Accel()
    {
        float num = (-g * (2 * m1 + m2) * Mathf.Sin(theta1)) -
                    (m2 * g * Mathf.Sin(theta1 - 2 * theta2)) -
                    (2 * Mathf.Sin(theta1 - theta2) * m2 * (theta2Vel * theta2Vel * L2 + theta1Vel * theta1Vel * L1 * Mathf.Cos(theta1 - theta2)));
        float denom = L1 * (2 * m1 + m2 - m2 * Mathf.Cos(2*theta1-2*theta2));
        return num / denom;
    }
    float GetTheta2Accel()
    {
        float num = (2 * Mathf.Sin(theta1 - theta2) * (theta1Vel * theta1Vel * L1 * (m1 + m2) + g * (m1 + m2) * Mathf.Cos(theta1) + theta2Vel * theta2Vel * L2 * m2 * Mathf.Cos(theta1 - theta2)));
        float denom =L2*(2*m1+m2-m2*Mathf.Cos(2*theta1-2*theta2));
        return num / denom;
    }

    void UpdateThetas(float delta)
    {
        theta1 += theta1Vel*delta;
        theta1Vel += GetTheta1Accel() * delta;

        theta2 += theta2Vel*delta;
        theta2Vel += GetTheta2Accel() * delta;
    }


    void Start()
    {

    }

    void Update()
    {
        UpdateThetas(Time.deltaTime);
        sphere1.transform.position = this.transform.position + new Vector3(Mathf.Sin(theta1) * L1, Mathf.Cos(theta1) * L1, 0);
        sphere2.transform.position = sphere1.transform.position + new Vector3(Mathf.Sin(theta2) * L2, Mathf.Cos(theta2) * L2, 0);
    }
}
