using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject[] droneCams;
    public GameObject multiViewCanvas; //multi-camera canvas
    public int selectedDrone = 1;
    public SteamVR_Action_Boolean switchCamLeft;
    public SteamVR_Action_Boolean switchCamRight;
    public SteamVR_Action_Boolean toggleCameraCanvas;
    public SteamVR_Input_Sources handType;
    public TMP_Text droneInUse; //Text display drone currently in use
    public RawImage[] camViews; //cameras mini-views
    public Texture[] camTextures; //drone cameras textures
    private Color[] droneColors; //colors of drones
    public Image[] warnings; //warning images - current view and 3 mini-views
    

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
        this.gameObject.transform.parent = droneCams[selectedDrone].transform; //Attach CameraRig to current drone
        this.gameObject.transform.localPosition = Vector3.zero;
        updateUI();
        switchCamLeft.AddOnStateDownListener(TriggerDownLeft, handType);
        switchCamRight.AddOnStateDownListener(TriggerDownRight, handType);
        toggleCameraCanvas.AddOnStateDownListener(ToggleCameraCanvas, handType);
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

    //Update UI of defect warnings
    public void UpdateDefectWarnings()
    {
        bool[] defects = DroneManager.defectWarnings; //record of currently active defects
        for (int n = 0; n < defects.Length; n++)
        {
            if (n == selectedDrone)
            {
                warnings[0].gameObject.SetActive(defects[n]);
            }
            else
            {
                warnings[n > selectedDrone ? n : n + 1].gameObject.SetActive(defects[n]);
            }
        }
    }
}
