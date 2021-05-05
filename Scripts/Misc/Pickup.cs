using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour {

    [SerializeField]
    private float m_flashSpeed;

    [SerializeField]
    private Color m_color1;
    [SerializeField]
    private Color m_color2;

    private Renderer rnd;

    void Start()
    {
        if (rnd == null)
            rnd = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void FixedUpdate () {

        if (m_color1 == null || m_color2 == null)
            return;

        rnd.material.SetColor("_OutlineColor", Color.LerpUnclamped(m_color1, m_color2, Mathf.PingPong(Time.time * Time.deltaTime * m_flashSpeed, 1f)));

	}
}
