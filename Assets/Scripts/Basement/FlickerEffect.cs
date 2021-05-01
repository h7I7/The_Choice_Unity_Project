using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickerEffect : MonoBehaviour {

    [SerializeField]
    private bool m_enabled;
    public bool m_allLightsOn = false;

    [SerializeField]
    [Range(0f, 10f)]
    private float m_chanceToFlicker;

    [SerializeField]
    private Vector2 m_flickerPitchRange;
    [SerializeField]
    private Vector2 m_cooldownRange;

    private FullscreenShader m_playerFullscreenEffect;

    private Flicker[] m_lightArray;

    [SerializeField]
    private AudioClip m_flickerLightSound;

    void Start()
    {
        m_playerFullscreenEffect = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FullscreenShader>();
        m_lightArray = GetChildrenFlickerComponents();

        UnityEngine.Random.InitState((int)(Time.time * Time.deltaTime * 1000f));
    }

	// Update is called once per frame
	void Update () {

        // If all the lights need to be on
        if (m_allLightsOn)
        {
            foreach(Flicker light in m_lightArray)
            {
                light.GetComponent<Light>().enabled = true;
            }
            return;
        }

        if (!m_enabled)
            return; // Early out

        foreach (Flicker light in m_lightArray)
        {
            if (UnityEngine.Random.Range(0f, 100f) < m_chanceToFlicker)
            {
                light.ToggleFlicker(m_flickerLightSound, m_flickerPitchRange, m_cooldownRange);
            }
        }
	}

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            m_enabled = true;

            m_playerFullscreenEffect.enabled = false;
        }
    }

    // Get all the light components from the children on this object
    private Flicker[] GetChildrenFlickerComponents(GameObject a_obj = null)
    {
        GameObject obj = a_obj;

        if (a_obj == null)
            obj = gameObject;

        List<Flicker> flickerList = new List<Flicker>();

        for(int i = 0; i < transform.childCount; ++i)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.transform.childCount > 0)
                flickerList.AddRange(GetChildrenFlickerComponents(child));

            if (child.GetComponent<Flicker>())
                flickerList.Add(child.GetComponent<Flicker>());
        }

        return flickerList.ToArray();
    }
}
