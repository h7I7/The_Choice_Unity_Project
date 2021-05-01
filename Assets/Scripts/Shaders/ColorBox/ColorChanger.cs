using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ColorChanger : MonoBehaviour {

    [SerializeField]
    private string m_propertyName;

    [SerializeField]
    private float m_changeSpeed;

    [SerializeField]
    private Light m_lightEmitter;

	// Update is called once per frame
	void Update () {
        Color c = HSBColor.ToColor(new HSBColor(Mathf.PingPong(Time.time * m_changeSpeed, 1f), 1f, 1f));

        GetComponent<Renderer>().material.SetColor(m_propertyName, c);
        if (m_lightEmitter != null)
            m_lightEmitter.color = c;
	}
}
