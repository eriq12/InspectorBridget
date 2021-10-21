using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public GameObject[] drones;
    public LineRenderer[] pathDisplay;

    #region drone path info
    // -=- desire to depricate in favor of objects to allow for adding points midway
    public Vector3[][] paths = new Vector3[4][];            // array of input positions
    public Vector3[][] headings = new Vector3[4][];         // array of input headings
    private int[][] defects = new int[4][];                 // array of input defect flags
    private int[][] stops = new int[4][];                   // array of input stop flags
    private int[] progress = new int[4];                    // array of progress of each drone
    public int[] status = new int[4];                       // array of drone status
                                                            /** status number meanings:
                                                             *  0 : Proceed with Path
                                                             *  1 : Defect Detected
                                                             *  2 : Stop
                                                             *  3 : Defect and Stop
                                                             **/
    private int[] numPoints = new int[4];                   // number of input positions of each drone
    [SerializeField]
    private int numVisualPoints;                            // number of points on drone path visual
    // for determining path progress
    private float[] pathLength = new float[4];              // full distance of path lengths
    [SerializeField]
    private float[][] pathPercPartial = new float[4][];     // partial percentages, should sum to one
    [SerializeField]
    private float[][] pathPercCumula = new float[4][];      // cumulative 
    private float[] pathProgress = new float[4];            // holds progress as a float from 0 to 1 for each path
    private float travelUnit = 5.0f;                        // decided unit of travel per second
    // assumes that there are only two colors
    [SerializeField]
    private float darkenPercentage = 0.2f;
    private Gradient[] pathGradients = new Gradient[4];
    private GradientColorKey[][] pathVisualColors;
    private GradientAlphaKey[] defaultAlphaKeys;
    #endregion

    public UIManager ui;

    // Start is called before the first frame update
    void Start()
    {
        // set alpha keys
        defaultAlphaKeys = new GradientAlphaKey[1];
        defaultAlphaKeys[0].alpha = 1.0f;
        defaultAlphaKeys[0].time = 0.0f;

        // set colors
        pathVisualColors = new GradientColorKey[4][];
        for (int path = 0; path < paths.Length; path++)
        {
            // initialize array
            pathVisualColors[path] = new GradientColorKey[2];
            // get default color
            pathVisualColors[path][1].color = ui.GetPathColor(path);
            pathVisualColors[path][1].time = 0.0f;
            // interpolate
            pathVisualColors[path][0].color = Vector4.Lerp(pathVisualColors[path][1].color, Color.black, darkenPercentage);
            pathVisualColors[path][1].time = 0.0f;

            // set gradients and set to Line Renderer
            pathGradients[path] = new Gradient();
            pathGradients[path].SetKeys(pathVisualColors[path], defaultAlphaKeys);
            pathDisplay[path].colorGradient = pathGradients[path];
        }

        LoadPath();
        InitializePathVisual();
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
        // read csv
        string[] lines = System.IO.File.ReadAllLines("drone_path.csv");

        // read the amount of points per each path
        string[] metaInput = lines[0].Split(',');
        for (int pathIndx = 0; pathIndx < paths.Length; pathIndx++)
        {
            int amt = int.Parse(metaInput[pathIndx]) + 1; // holds the amount of datapoints that should appear per path + starting point
            paths[pathIndx] = new Vector3[amt];
            headings[pathIndx] = new Vector3[amt];
            stops[pathIndx] = new int[amt];
            defects[pathIndx] = new int[amt];
        }

        // add starting points
        for (int pthIdx = 0; pthIdx < paths.Length; pthIdx++)
        {
            paths[pthIdx][0] = drones[pthIdx].transform.position;
            headings[pthIdx][0] = drones[pthIdx].transform.eulerAngles;
            stops[pthIdx][0] = 0;
            defects[pthIdx][0] = 0;
            numPoints[pthIdx]++;
        }

        // process each line
        for (int lineNum = 1; lineNum < lines.Length; lineNum++)
        {
            string[] input = lines[lineNum].Split(',');
            int droneNum = int.Parse(input[0]) - 1;
            paths[droneNum][numPoints[droneNum]] = new Vector3(float.Parse(input[1]), float.Parse(input[2]), float.Parse(input[3]));
            headings[droneNum][numPoints[droneNum]] = new Vector3(float.Parse(input[4]), float.Parse(input[5]), float.Parse(input[6]));
            stops[droneNum][numPoints[droneNum]] = int.Parse(input[7]);
            defects[droneNum][numPoints[droneNum]] = int.Parse(input[8]);
            numPoints[droneNum]++;
        }

        // calculate lengths and progress for each line
        for (int pathIndx = 0; pathIndx < drones.Length; pathIndx++)
        {
            pathPercPartial[pathIndx] = new float[paths[pathIndx].Length];
            pathPercCumula[pathIndx] = new float[paths[pathIndx].Length];
            for (int pointIndx = 1; pointIndx < paths[pathIndx].Length; pointIndx++)
            {
                if (paths[pathIndx][pointIndx] == null)
                {
                    break;
                }
                // get the lengths
                pathPercPartial[pathIndx][pointIndx] = Vector3.Distance(paths[pathIndx][pointIndx], paths[pathIndx][pointIndx - 1]);
                pathLength[pathIndx] += pathPercPartial[pathIndx][pointIndx];
                pathPercCumula[pathIndx][pointIndx] = pathLength[pathIndx];
            }
        }
        for (int pathIndx = 0; pathIndx < drones.Length; pathIndx++)
        {
            for (int pointIndx = 1; pointIndx < paths[pathIndx].Length; pointIndx++)
            {
                pathPercPartial[pathIndx][pointIndx] /= pathLength[pathIndx];
                pathPercCumula[pathIndx][pointIndx] /= pathLength[pathIndx];
            }
        }
    }

    #region path visual methods

    /**
     * takes the positions for paths and moves them to 
     **/
    void InitializePathVisual()
    {
        // path index so conversion to for loop is easier
        for(int pathIndx = 0; pathIndx < drones.Length; pathIndx++) {
            // hold positions of the path
            Vector3[] pathPos = new Vector3[paths[pathIndx].Length];
            for (int i = 0; i < pathPos.Length; i++)
            {
                // change the global positions to relative positons (as line renderer uses local positions)
                pathPos[i] = pathDisplay[pathIndx].transform.InverseTransformPoint(paths[pathIndx][i]);
            }
            Vector3[] pathVisPos = new Vector3[numVisualPoints];
            float step = 1.0f / pathVisPos.Length;
            int pointIndx = -1;
            float progress = 0;
            for(int i = 0; i < pathVisPos.Length; i++, progress += step)
            {
                if (progress >= pathPercCumula[pathIndx][pointIndx + 1])
                {
                    pathVisPos[i] = pathPos[++pointIndx];
                }
                else
                {
                    float percOfSegment = Mathf.InverseLerp(pathPercCumula[pathIndx][pointIndx], pathPercCumula[pathIndx][pointIndx + 1], progress);
                    pathVisPos[i] = Vector3.Lerp(pathPos[pointIndx], pathPos[pointIndx + 1], percOfSegment);
                }
            }
            // set length for points to use and set them
            pathDisplay[pathIndx].positionCount = pathVisPos.Length;
            pathDisplay[pathIndx].SetPositions(pathVisPos);
        }
    }

    /**
     * updates the designated path
     */
    void UpdatePathVisual(int path)
    {
        pathVisualColors[path][1].time = Mathf.Clamp(pathProgress[path], 0.0f, 1.0f);
        pathVisualColors[path][0].time = Mathf.Max(0, pathVisualColors[path][1].time - 0.0001f);
        pathGradients[path].SetKeys(pathVisualColors[path], defaultAlphaKeys);
        pathDisplay[path].colorGradient = pathGradients[path];
    }

    #endregion

    #region drone flight control helpers

    public void StopPath(int pathNumber)
    {
        status[pathNumber] |= 2;
        ui.UpdateCamWarnings();
    }

    public void StartMarkingDefect(int pathNumber)
    {
        status[pathNumber] |= 1;
        ui.UpdateCamWarnings();
    }

    public void ClearDefect(int pathNumber)
    {
        status[pathNumber] &= ~1;
        ui.UpdateCamWarnings();
    }

    public void ProceedWithPath(int pathNumber)
    {
        status[pathNumber] = 0;
        ui.UpdateCamWarnings();
    }

    #endregion

    #region status helpers
    // these methods return status (either full or specific partial)
    // doing != 0 as doing Convert.ToBoolean() method will likely take more computation to perform
    public int GetStatus(int pathNumber) 
    {
        return status[pathNumber];
    }

    public bool IsStopped(int pathNumber)
    {
        return (status[pathNumber] & 2) != 0;
    }

    public bool IsCheckDefect(int pathNumber)
    {
        return (status[pathNumber] & 1) != 0;
    }

    #endregion

    //move drone n
    void MoveDrone(int n)
    {
        if ((status[n] == 0) && (progress[n] < numPoints[n])) // if this drone does not have any problems/force stops and has not reached the last point
        {
            Vector3 currPos = drones[n].transform.position;
            drones[n].transform.position = Vector3.MoveTowards(currPos, paths[n][progress[n]], travelUnit * Time.deltaTime); //move towards next point

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

            Vector3 rotation = headings[n][progress[n]] - drones[n].transform.eulerAngles;
            drones[n].transform.Rotate(rotation * Time.deltaTime);

            if (drones[n].transform.position == paths[n][progress[n]])
            {
                // calculate actual distance travelled
                // check for defects or stops
                if (defects[n][progress[n]] != 0) // defect found
                {
                    Debug.Log("drone " + n + " found defect");
                    StartMarkingDefect(n);
                    //defects[n, progress[n]] = 0;
                }
                if (stops[n][progress[n]] != 0) // planned stop
                {
                    Debug.Log("planned stop for drone " + n);
                    StopPath(n);
                    //defects[n, progress[n]] = 0;
                }
                progress[n]++; // set the next position
            }
            pathProgress[n] += Vector3.Distance(currPos, drones[n].transform.position) / pathLength[n];
            Debug.LogFormat("Drone {0} is {1} of the path completed", n, pathProgress[n]);
            UpdatePathVisual(n);
        }
    }

}
