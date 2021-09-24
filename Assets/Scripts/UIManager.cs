using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    // drone information
    public GameObject[] droneCams;
    public int selectedDrone = 1;
    public DroneManager droneManager;

    // UI Canvases (menus)
    public GameObject multiViewCanvas;                  // multi-camera canvas
    public GameObject pathViewCanvas;                   // standard drone view camera

    // control
    public SteamVR_Action_Boolean switchCamLeft;
    public SteamVR_Action_Boolean switchCamRight;
    public SteamVR_Action_Boolean toggleCameraCanvas;
    public SteamVR_Input_Sources handType;

    // UI Elements
    public TMP_Text[] droneLabels;                      // labels for each of the drone views
    public RawImage[] camViews;                         // cameras mini-views
    public Texture[] camTextures;                       // drone cameras textures
    private Color[] droneColors;                        // colors of drones
    public Image[] warnings;                            // warning images - current view and 4 mini-views
    public Image[] stops;                               // stop images - current view and 4 mini-views
    public Text currentDroneLabel;                      // Text display drone currently in use


    private void Awake()
    {
        droneColors = new Color[4];
        // get the drone colors
        for (int i = 0; i < droneCams.Length; i++)
        {
            droneColors[i] = droneCams[i].transform.root.Find("DroneBody").gameObject.GetComponent<MeshRenderer>().materials[1].color;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Attach CameraRig to current drone
        this.gameObject.transform.parent = droneCams[selectedDrone].transform; 
        this.gameObject.transform.localPosition = Vector3.zero;

        // set view outline colors to drone views and labels
        for (int camIndx = 0; camIndx < camViews.Length; camIndx++) {
            camViews[camIndx].gameObject.GetComponent<Outline>().effectColor = droneColors[camIndx];
            droneLabels[camIndx].color = droneColors[camIndx];
        }

        // Subscribe event listeners
        switchCamLeft.AddOnStateDownListener(TriggerDownLeft, handType);
        switchCamRight.AddOnStateDownListener(TriggerDownRight, handType);
        toggleCameraCanvas.AddOnStateDownListener(ToggleCameraCanvas, handType);

        // update UI
        updateUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Switch view to previous drone in order
    public void TriggerDownLeft(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        selectedDrone = selectedDrone == 0 ? 3 : (selectedDrone - 1);
        this.gameObject.transform.parent = droneCams[selectedDrone].transform;
        this.gameObject.transform.localPosition = Vector3.zero;
        updateUI();
    }

    //Switch view to next drone in order
    public void TriggerDownRight(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        selectedDrone = (selectedDrone + 1) % 4;
        this.gameObject.transform.parent = droneCams[selectedDrone].transform;
        this.gameObject.transform.localPosition = Vector3.zero;
        updateUI();
    }

    public void ToggleCameraCanvas(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        pathViewCanvas.SetActive(multiViewCanvas.activeInHierarchy);
        multiViewCanvas.SetActive(!multiViewCanvas.activeInHierarchy);
    }

    //Switch view to drone n
    public void selectDrone(int n)
    {
        this.gameObject.transform.parent = droneCams[n].transform;
        this.gameObject.transform.localPosition = Vector3.zero;
        selectedDrone = n;
        updateUI();
    }

    /**
    Antiquated code: no longer having only 3/4 views, switching to showing all 4 views regardless
    private void updateUI()
    {
        //update current drone indicator
        droneInUse.text = (selectedDrone + 1).ToString();
        droneInUse.color = droneColors[selectedDrone];

        int m = 0;
        //update mini-views
        for (int n = 0; n < 4; n++)
        {
            if (n != selectedDrone)
            {
                camViews[m].texture = camTextures[n];
                camViews[m].gameObject.GetComponent<Outline>().effectColor = droneColors[n];
                m++;
            }
        }
        UpdateDefectWarnings();
    }
    */

    // sets the label for current drone, then calls UpdateViewWarnings to change UI
    private void updateUI() 
    {
        currentDroneLabel.text = "Drone " + (selectedDrone + 1);
        UpdateViewWarnings();
    }

    // updates the defect and stop warnings for current view
    public void UpdateViewWarnings()
    {
        int currStatus = droneManager.GetStatus(selectedDrone);
        warnings[warnings.Length - 1].gameObject.SetActive((currStatus & 1) != 0);
        stops[stops.Length - 1].gameObject.SetActive((currStatus & 2) != 0);
    }

    //Update UI of defect warnings for minimap and current view
    public void UpdateCamWarnings()
    {
        for (int n = 0; n < droneCams.Length; n++)
        {
            int currStatus = droneManager.GetStatus(n);
            // other warnings corespond with their respective indicies
            warnings[n].gameObject.SetActive((currStatus & 1) != 0);
            stops[n].gameObject.SetActive((currStatus & 2) != 0);
        }
        UpdateViewWarnings();
    }
}
