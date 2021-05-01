using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class ObjectiveDataStruct
{
    [TextArea(3, 4)]
    public string objective = "";
    public float displayTime = 10f;
    public UnityEvent callback;
}

// This script allows me to store some objective data in a gameobject on each level
public class ObjectiveData : MonoBehaviour {

    public static ObjectiveData instance;

    [SerializeField]
    private List<ObjectiveDataStruct> m_objectives;

    public List<ObjectiveDataStruct> Objectives
    {
        get { return m_objectives; }
    }

    public GameObject ObjectiveParent;
    public Text ObjectiveTitle;
    public Text ObjectiveText;

    private ObjectiveManager m_objectiveManager;

    void Awake()
    {
        instance = this;

        m_objectiveManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<ObjectiveManager>();
    }

    // Updating the objective
    public void UpdateObjectives(int a_index = -1)
    {
        m_objectiveManager.NextObjective(a_index);

    }
}
