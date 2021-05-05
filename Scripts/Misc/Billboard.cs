using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour {

    [SerializeField]
    private Transform m_lookTarget;

	// Update is called once per frame
	void Update () {

        // Early-out
        if (m_lookTarget == null)
            return;

        transform.LookAt(transform.position + m_lookTarget.transform.rotation * Vector3.back, m_lookTarget.rotation * Vector3.up);
    }
}
