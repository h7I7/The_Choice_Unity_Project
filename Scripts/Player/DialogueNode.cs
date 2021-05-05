using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class Options
{
    [TextArea(3, 4)]
    public string optionText;

    public bool hasBeenRead;
    [SerializeField]
    private bool m_returnOnceRead;
    public bool ReturnOnceRead
    {
        get { return m_returnOnceRead; }
    }

    public UnityEvent eventsOnDialogueFinish;

    public int nextID;
}

[System.Serializable]
public class DNode
{
    public int _ID;

    [Space(2)]
    [TextArea(3, 12)]
    public List<string> dialogueText;
    public List<Options> options;

    [Space(2)]
    [SerializeField]
    private bool m_firstDialogue;
    public bool FirstDialogue
    {
        get { return m_firstDialogue; }
    }
}

[System.Serializable]
public class Dialogues
{
    public List<DNode> dialogues;
}

public class DialogueNode : MonoBehaviour {

    [SerializeField]
    private string m_name;
    public string Name
    {
        get { return m_name; }
    }

    [SerializeField]
    private Vector2 m_voicePitch;
    public Vector2 VoicePitch
    {
        get { return m_voicePitch; }
    }

    [SerializeField]
    private AudioClip m_voiceSound;
    public AudioClip VoiceSound
    {
        get { return m_voiceSound; }
    }

    [SerializeField]
    private Transform m_lookPoint;
    public Transform LookPoint
    {
        get { return m_lookPoint; }
    }

    // All dialogues
    [SerializeField]
    private int m_currentDialogueIndex = 0;

    [SerializeField]
    private List<Dialogues> m_allDialogues;
    public List<Dialogues> AllDialogues
    {
        get { return m_allDialogues; }
    }

    // The current dialogue
    public List<DNode> Dialogue
    {
        get { return m_allDialogues[m_currentDialogueIndex].dialogues; }
    }

    private void Start()
    {
        if (m_lookPoint == null && transform.childCount > 0)
            m_lookPoint = transform.GetChild(0);
    }

    public void ChangeName(string a_name)
    {
        m_name = a_name;
    }

    public void ChangeDialogue(int a_index)
    {
        if (a_index >= 0 && a_index <= m_allDialogues.Count)
        {
            m_currentDialogueIndex = a_index;
        }
    }
}