using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneController : MonoBehaviour
{
    public Transform[] rotors;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SpinRotors();
    }

    void SpinRotors()
    {
        for (int i = 0; i < rotors.Length; i++)
        {
            rotors[i].Rotate(Vector3.up, 100000f * Time.deltaTime, Space.Self);
        }
    }
}
