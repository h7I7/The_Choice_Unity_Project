using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ShowControls : MonoBehaviour {

    [SerializeField]
    private int m_objectiveToShow;

    private bool m_activated = false;

    private void OnTriggerEnter(Collider col)
    {
        if (col.tag == "Player" && !m_activated)
        {
            ObjectiveData.instance.UpdateObjectives(m_objectiveToShow);
            m_activated = true;
        }
    }

}
