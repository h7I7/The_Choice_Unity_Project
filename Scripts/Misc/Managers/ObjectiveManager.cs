using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public delegate void VoidDelegate();

public class DisplayQueue
{
    public IEnumerator action;
    public VoidDelegate secondary;
    public bool overwrite;

    public DisplayQueue(IEnumerator a_action, bool a_overwrite = false, VoidDelegate a_secodary = null)
    {
        this.action = a_action;
        this.overwrite = a_overwrite;
        this.secondary = a_secodary;
    }
}

public class ObjectiveManager : MonoBehaviour
{

    [SerializeField]
    private float m_fadeSpeed = 1f;

    [Space(5)]
    [SerializeField]
    private float m_fadeInDelay = 1f;
    [SerializeField]
    private float m_fadeOutDelay = 0f;

    [Space(5)]
    [Range(0f, 1f)]
    [SerializeField]
    private float m_updateSoundVolume = 1f;
    [SerializeField]
    private AudioClip m_updateSound;

    private AudioSource m_aSrc;

    private float m_t = 0f;
    private bool m_textActive = false;

    [SerializeField]
    private int m_objectivesIndex = 0;
    public int ObjectivesIndex
    {
        get { return m_objectivesIndex; }
    }

    private bool m_fastTrackEum = false;
    private bool m_enumRunning = false;

    // I'm using queues for my displays as I was using no queue and the update
    // was determined by how fast the player could click in a single frame
    // allowing them to complete level 2 in about 5-10 seconds.
    // This way I can control everything and validate the updates
    private Queue<DisplayQueue> m_enumQueue;

    private void Awake()
    {
        m_aSrc = GetComponent<AudioSource>();

        m_enumQueue = new Queue<DisplayQueue>();
    }

    private void Update()
    {
        if (ObjectiveData.instance == null)
            return; // Early out

        // If the objective display is active
        if (m_textActive)
        {
            // If the correct time that it should be active for has elapsed
            if (Time.time - m_t > ObjectiveData.instance.Objectives[m_objectivesIndex].displayTime)
            {
                // Fade it out
                StartCoroutine(FadeOutEnum());
            }
        }

        // If there are enumerators to dequeue
        if (m_enumQueue.Count > 0)
        {
            bool keepCheckingQueue = false;

            while (keepCheckingQueue)
            {
                // This will check if we need to skip to a certain enumerator in the queue
                foreach (DisplayQueue dq1 in m_enumQueue)
                {
                    // If we need to skip to it
                    if (dq1.overwrite)
                    {
                        // Then for each item in the queue
                        foreach (DisplayQueue dq2 in m_enumQueue)
                        {
                            // If its the one we need to skip to then run it
                            if (dq1 == dq2)
                            {
                                dq1.overwrite = false;

                                StopCoroutines();

                                if (m_textActive)
                                    StartCoroutine(FadeOutEnum());
                                break;
                            }
                            // Else we need to dequeue the items in front
                            else
                            {
                                m_enumQueue.Dequeue();
                                keepCheckingQueue = true;
                                break;
                            }
                        }
                        break;
                    }

                    keepCheckingQueue = false;
                }
            }

            // If we don't have an enumerator running and there is not text active we can start the next item in the list
            if (!m_enumRunning && !m_textActive)
            {
                ActivateNextItem();
            }
        }
    }

    // Activating the next item in the queue
    private void ActivateNextItem()
    {
        DisplayQueue dq = m_enumQueue.Dequeue();

        StartCoroutine(dq.action);
        if (dq.secondary != null)
            dq.secondary.Invoke();
    }

    // Stopping all coroutines in this script
    private void StopCoroutines()
    {
        StopCoroutine(FadeInEnum());
        StopCoroutine(FadeOutEnum());
    }
    
    // Playing the update sound for objective s
    private void PlayUpdateSound()
    {
        m_aSrc.PlayOneShot(m_updateSound, m_updateSoundVolume);
    }

    // Loading new objectives when a new level is loaded
    public void LoadNewLevelObjectives()
    {
        // If we can find any objective UI
        if (GameObject.FindGameObjectWithTag("ObjectiveUI"))
        {
            // Set the UI
            m_objectivesIndex = 0;
            ObjectiveData.instance.ObjectiveText.text = ObjectiveData.instance.Objectives[0].objective;

            // Resetting the objective display and objective queue
            StopCoroutines();
            m_enumQueue = new Queue<DisplayQueue>();
            FadeOut(false, true);

            // Fade in the first objective
            FadeIn();
        }
    }


    // Changing the objective text to the next one
    public void NextObjective(int a_index = -1, bool overwrite = false)
    {
        if (a_index >= 0 && a_index < ObjectiveData.instance.Objectives.Count)
        {
            FadeOut();
            FadeIn(false, false, overwrite, new VoidDelegate(() => { SetObjectiveText(a_index); }));
        }
        else if (a_index == -1)
        {
            FadeOut();
            FadeIn(false, false, overwrite, new VoidDelegate(() => { SetObjectiveText(m_objectivesIndex + 1); }));
        }
    }

    // Setting all the objective data upon update
    private void SetObjectiveText(int a_index)
    {
        m_objectivesIndex = a_index;
        ObjectiveData.instance.ObjectiveText.text = ObjectiveData.instance.Objectives[m_objectivesIndex].objective;
        ObjectiveData.instance.Objectives[m_objectivesIndex].callback.Invoke();
        PlayUpdateSound();
    }

    // Setting up a fade out
    public void FadeOut(bool a_textOnly = false, bool a_fastTrack = false, bool a_overwrite = false, VoidDelegate a_seconday = null)
    {
        m_enumQueue.Enqueue(new DisplayQueue(FadeOutEnum(a_textOnly, a_fastTrack), a_overwrite, a_seconday));
    }

    // The fade out enumerator
    private IEnumerator FadeOutEnum(bool a_textOnly = false, bool a_fastTrack = false)
    {
        // Waiting for objective data
        while (ObjectiveData.instance == null)
            yield return null;

        m_enumRunning = true;

        if (!a_textOnly)
            yield return new WaitForSeconds(m_fadeOutDelay);

        float t = Time.time;

        Color fadeColour = ObjectiveData.instance.ObjectiveText.color;
        float startingAlpha = fadeColour.a;

        while (fadeColour.a > 0f)
        {
            if (a_fastTrack)
            {
                fadeColour.a = 0f;
            }

            fadeColour.a = Mathf.Lerp(startingAlpha, 0f, (Time.time - t) * m_fadeSpeed);

            if (!a_textOnly)
            {
                for (int i = 0; i < ObjectiveData.instance.ObjectiveParent.transform.childCount; ++i)
                {
                    if (ObjectiveData.instance.ObjectiveParent.transform.GetChild(i).GetComponent<Text>())
                        ObjectiveData.instance.ObjectiveParent.transform.GetChild(i).GetComponent<Text>().color = fadeColour;
                }
            }
            else
            {
                ObjectiveData.instance.ObjectiveText.color = fadeColour;
            }

            yield return null;
        }

        m_textActive = false;
        m_enumRunning = false;
    }

    // Setting up a fade in
    public void FadeIn(bool a_textOnly = false, bool a_fastTrack = false, bool a_overwrite = false, VoidDelegate a_seconday = null)
    {
        m_enumQueue.Enqueue(new DisplayQueue(FadeInEnum(a_textOnly, a_fastTrack), a_overwrite, a_seconday));
    }

    // Fading in enumerator
    private IEnumerator FadeInEnum(bool a_textOnly = false, bool a_fastTrack = false)
    {
        m_enumRunning = true;

        if (!a_textOnly)
            yield return new WaitForSeconds(m_fadeInDelay);

        if (m_objectivesIndex == 0 || a_textOnly)
            PlayUpdateSound();

        m_textActive = true;
        m_t = Time.time;

        while (ObjectiveData.instance.ObjectiveText == null)
            yield return null;

        Color fadeColour = ObjectiveData.instance.ObjectiveText.color;
        float startingAlpha = fadeColour.a;

        while (fadeColour.a < 1f)
        {
            if (a_fastTrack)
            {
                fadeColour.a = 1f;
            }

            fadeColour.a = Mathf.Lerp(startingAlpha, 1f, (Time.time - m_t) * m_fadeSpeed);

            if (!a_textOnly)
            {
                for (int i = 0; i < ObjectiveData.instance.ObjectiveParent.transform.childCount; ++i)
                {
                    if (ObjectiveData.instance.ObjectiveParent.transform.GetChild(i).GetComponent<Text>())
                        ObjectiveData.instance.ObjectiveParent.transform.GetChild(i).GetComponent<Text>().color = fadeColour;
                }
            }
            else
            {
                ObjectiveData.instance.ObjectiveText.color = fadeColour;
            }

            yield return null;
        }

        m_enumRunning = false;
    }
}