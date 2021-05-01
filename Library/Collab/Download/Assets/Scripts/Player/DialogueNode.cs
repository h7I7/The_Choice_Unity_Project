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

    [TextArea(3, 12)]
    public List<string> dialogueText;
    public List<Options> options;

    [SerializeField]
    private bool m_firstDialogue;
    public bool FirstDialogue
    {
        get { return m_firstDialogue; }
    }
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

    [SerializeField]
    private List<DNode> m_dialogue;
    public List<DNode> Dialogue
    {
        get { return m_dialogue; }
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
}
