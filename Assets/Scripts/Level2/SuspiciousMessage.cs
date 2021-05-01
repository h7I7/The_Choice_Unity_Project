using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspiciousMessage : MonoBehaviour {

    [SerializeField]
    private int m_indexToCheckFrom;
    [SerializeField]
    private int m_indexToCheckTo;

    private ObjectiveManager m_objectiveManager;

    [SerializeField]
    private bool m_activated = false;
    private Transform m_cube;

    void Awake()
    {
        // Get objective manager
        m_objectiveManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<ObjectiveManager>();

        // Get colour cube transform
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("InteractablePickup"))
        {
            if (obj.name == "ColourCube")
            {
                m_cube = obj.transform;
                break;
            }
        }
    }

    // When the cube gets close enough to the painting
    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name == "ColorCube")
        {
            m_cubeColliding = true;
        }
    }

    private bool m_cubeColliding = false;

	// Update is called once per frame
	void Update () {
        if (m_activated)
            return; // Early out

        // If everything is set up and the cube is close enough then go to the next objective
        if (m_objectiveManager.ObjectivesIndex >= m_indexToCheckFrom && m_objectiveManager.ObjectivesIndex <= m_indexToCheckTo)
        {
            if (m_cubeColliding)
            {
                m_activated = true;
                m_objectiveManager.NextObjective(-1, true);
            }
        }
	}
}