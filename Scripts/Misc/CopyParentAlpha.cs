using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CopyParentAlpha : MonoBehaviour {

    // Used for text to copy the alpha of the parent object's text

    private Text m_t;

    void Start()
    {
        if (m_t == null)
            m_t = GetComponent<Text>();
    }

	// Update is called once per frame
	void Update () {

        Color temp = m_t.color;
        temp.a = transform.parent.GetComponent<Text>().color.a;
        m_t.color = temp;
	}
}
