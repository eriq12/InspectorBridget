using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Valve.VR.Extras;

public class LaserHandler : MonoBehaviour
{
    private SteamVR_LaserPointer laserPointer;
    public bool automatic = true;

    void Awake()
    {
        laserPointer = FindObjectOfType<SteamVR_LaserPointer>();
        laserPointer.PointerIn += PointerInside;
        laserPointer.PointerOut += PointerOutside;
        laserPointer.PointerClick += PointerClick;
    }

    private void OnEnable()
    {
        if (!laserPointer)
            laserPointer = FindObjectOfType<SteamVR_LaserPointer>();
        if (automatic)
            TurnOnlaser();
    }

    private void OnDisable()
    {
        EventSystem.current.SetSelectedGameObject(null);
        laserPointer.active = false;
    }

    public void PointerClick(object sender, PointerEventArgs e)
    {
        if (e.target.gameObject.GetComponent<Button>())
        {
            e.target.gameObject.GetComponent<Button>().OnPointerClick(new PointerEventData(EventSystem.current));
        }
    }

    public void PointerInside(object sender, PointerEventArgs e)
    {
        if (e.target.gameObject.GetComponent<Button>())
        {
            if (e.target)
            {
                /*
                Button button = e.target.gameObject.GetComponent<Button>();
                if (button)
                    button.Select();
                */
                Outline ol = e.target.gameObject.GetComponent<Outline>();
                Color col = ol.effectColor;
                col.a = 1;
                ol.effectColor = col;
            }
        }
    }

    public void PointerOutside(object sender, PointerEventArgs e)
    {
        if (e.target.gameObject.GetComponent<Button>())
        {
            EventSystem.current.SetSelectedGameObject(null);
            Outline ol = e.target.gameObject.GetComponent<Outline>();
            Color col = ol.effectColor;
            col.a = 0;
            ol.effectColor = col;
        }
    }

    public void TurnOnlaser()
    {
        laserPointer.active = true;
    }
}