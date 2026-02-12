using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinSpin : MonoBehaviour
{
    void Start()
    {
       t = GetComponent<Transform>();
    }

    void Update()
    {
        t.localEulerAngles = t.localEulerAngles + new Vector3(0, Time.deltaTime * 120.0f, 0);
    }

    Transform t;
}
