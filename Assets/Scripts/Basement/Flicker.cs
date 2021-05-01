using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Flicker : MonoBehaviour {

    [SerializeField]
    private bool m_permanentlyGoOut = false;

    private AudioSource m_aSrc;
    private Light m_light;

    private float m_t;
    private float m_cooldown;

	// Use this for initialization
	void Start () {
        m_aSrc = GetComponent<AudioSource>();
        m_light = GetComponent<Light>();
	}
	
	public void ToggleFlicker(AudioClip a_clip, Vector2 a_pitchRange, Vector2 m_cooldownRange)
    {
        if (m_permanentlyGoOut && !m_light.enabled)
            return; // Early out if permanently disabled

        if (Time.time - m_t > m_cooldown)
        {
            m_t = Time.time;
            m_light.enabled = !m_light.enabled;
            m_aSrc.pitch = UnityEngine.Random.Range(a_pitchRange.x, a_pitchRange.y);
            m_aSrc.PlayOneShot(a_clip);

            m_cooldown = UnityEngine.Random.Range(m_cooldownRange.x, m_cooldownRange.y);
        }
    }
}
