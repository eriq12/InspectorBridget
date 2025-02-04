﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public GameObject[] drones;
    public Vector3[,] paths = new Vector3[4,20]; //array of input positions
    public Vector3[,] headings = new Vector3[4, 20]; //array of input headings
    private int[,] defects = new int[4,20]; //array of input defect flags
    private int[] progress = new int[4]; //array of progress of each drone
    private int[] numPoints = new int[4]; //number of input positions of each drone
    public UIManager ui;
    public static bool[] defectWarnings = new bool[4]; //keep track of defect warnings for each drone. Can be accessed from other classes

    // Start is called before the first frame update
    void Start()
    {
        LoadPath();
    }

    // Update is called once per frame
    void Update()
    {
        MoveDrone(0);
        MoveDrone(1);
        MoveDrone(2);
        MoveDrone(3);
    }

    private void LoadPath()
    {
        //read csv
        string[] lines = System.IO.File.ReadAllLines("drone_path.csv");
        //process each line
        foreach (string line in lines)
        {
            string[] input = line.Split(',');
            int droneNum = int.Parse(input[0]) - 1;
            paths[droneNum, numPoints[droneNum]] = new Vector3(float.Parse(input[1]), float.Parse(input[2]), float.Parse(input[3]));
            headings[droneNum, numPoints[droneNum]] = new Vector3(float.Parse(input[4]), float.Parse(input[5]), float.Parse(input[6]));
            defects[droneNum, numPoints[droneNum]] = int.Parse(input[7]);
            numPoints[droneNum]++;
        }
    }
    //move drone n
    void MoveDrone(int n)
    {
        if (progress[n] < numPoints[n]) // if this drone has not reached the last point
        {
            Vector3 currPos = drones[n].transform.position;
            drones[n].transform.position = Vector3.MoveTowards(currPos, paths[n, progress[n]], 5 * Time.deltaTime); //move towards next point

            /*
            Vector3 offset = paths[n, progress[n]] - currPos;
            offset.y = 0;
            float angle = Vector3.Angle(drones[n].transform.forward, offset);

            if (angle > 10)
            {
                drones[n].transform.Rotate(Vector3.up, angle);
                //Debug.Log(angle);
            }
            */

            Vector3 rotation = headings[n, progress[n]] - drones[n].transform.eulerAngles;
            drones[n].transform.Rotate(rotation * Time.deltaTime);

            if (drones[n].transform.position == paths[n, progress[n]])
            {
                if (defects[n, progress[n]] == 0) //no defect found
                {
                    progress[n]++;
                }
                else //defect found
                {
                    Debug.Log("drone " + n + " found defect");
                    defectWarnings[n] = true;
                    //defects[n, progress[n]] = 0;
                    ui.UpdateDefectWarnings();
                }
            }
        }
    }

}
