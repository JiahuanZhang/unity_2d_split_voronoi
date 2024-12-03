using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo1Clip : MonoBehaviour
{
    Vector3 dir;
    float time = 3f;
    static System.Random random = new System.Random();

    // Start is called before the first frame update
    void Start()
    {

    }

    public void UpdatePos(float deltaTime)
    {
        time += deltaTime;
        if (time >= 3)
        {
            dir = new Vector3(((float)random.NextDouble() - 0.5f) * 2, ((float)random.NextDouble() - 0.5f) * 2, 0);
        }
        transform.position += dir * deltaTime * 0.2f;
    }


    public void UpdatePos2(float deltaTime)
    {
        time += deltaTime;
        transform.position += Vector3.down * deltaTime * 0.3f;
    }
}
