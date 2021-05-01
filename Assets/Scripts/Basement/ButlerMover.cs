using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Unit))]
public class ButlerMover : MonoBehaviour {

    [SerializeField]
    private bool m_active;

    [SerializeField]
    [Range(0.1f, 5f)]
    private float m_range;

    [SerializeField]
    private List<Light> m_lightsToCheck;

    [SerializeField]
    private Transform m_playerTansform;
    private PlayerController m_playerController;

    [SerializeField]
    private Transform m_lookPoint;

    private Unit m_unit;
    private int m_lightsOn;

    [SerializeField]
    private FlickerEffect m_lightController;

    private AudioSource m_aSrc;

    [SerializeField]
    private float m_endTime;

    private bool m_beginEnd = false;
    private bool m_endCredit = false;
    private float m_t;

    [Space(5)]
    [SerializeField]
    private AudioSource m_aSrc2;
    [SerializeField]
    private AudioClip m_jumpScare;
    [Range(0f, 1f)]
    [SerializeField]
    private float m_jumpScareVolume;
    private bool m_jumpScareSoundPlayed = false;

	// Use this for initialization
	void Start () {
        m_unit = GetComponent<Unit>();
        m_playerController = m_playerTansform.GetComponent<PlayerController>();
        m_aSrc = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {

        if (m_beginEnd)
        {
            // Pause footsteps
            m_aSrc.Pause();

            // Do a jumpscare sound
            if (!m_jumpScareSoundPlayed)
            {
                // Play jump scare clip
                m_aSrc2.PlayOneShot(m_jumpScare, m_jumpScareVolume);
                m_jumpScareSoundPlayed = true;
            }
            
            // Looking towards position
            Quaternion dir = Camera.main.transform.rotation;
            dir.SetLookRotation((m_lookPoint.position - Camera.main.transform.position).normalized);
            Camera.main.transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, dir, (Time.time - m_t) * Time.deltaTime * 12);

            // If we are looking closely enough to the butler then instantly fade out and go to the next level
            if (Quaternion.Dot(Camera.main.transform.rotation, dir) > 0.99f)
            {
                GameObject fade = GameObject.FindGameObjectWithTag("Fader");
                fade.GetComponent<Image>().color = Color.black;
                Color col = fade.transform.GetChild(0).GetComponent<Text>().color;
                col.a = 1;
                fade.transform.GetChild(0).GetComponent<Text>().color = col;
                fade.transform.GetChild(0).GetComponent<Text>().text = "THIS IS THE END";

                m_t = Time.time;

                m_beginEnd = false;
                m_endCredit = true;
            }

            return;
        }

        if (m_endCredit)
        {
            
            if (Time.time - m_t > m_endTime)
                m_playerTansform.GetComponent<InteractionController>().NextLevel();
            return;
        }

        if (!m_active)
            return; // Early out

        m_lightsOn = 0;

        foreach(Light l in m_lightsToCheck)
        {
            if (l.enabled)
                m_lightsOn++;
        }

        if (m_lightsOn > 0)
        {
            m_unit.stopMoving = true;

            m_aSrc.Pause();
        }
        else
        {
            m_unit.stopMoving = false;

            m_aSrc.UnPause();
        }

        // If we are close enough to the player
        if (Vector3.Distance(transform.position, m_playerTansform.position) < m_range)
        {
            m_playerController.enabled = false;
            m_lightController.m_allLightsOn = true;
            m_unit.enabled = false;

            m_beginEnd = true;
        }
	}

    public void ActivateChase()
    {
        m_active = true;

        m_aSrc.Play();
        m_aSrc.Pause();
    }
}
