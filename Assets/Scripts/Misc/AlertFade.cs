using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class AlertFade : MonoBehaviour {

    [SerializeField]
    private float m_fadeDelay;

    [SerializeField]
    private float m_fadeSpeedTime;

    private Text m_txt;
    private float m_t;

    void Awake()
    {
        m_txt = GetComponent<Text>();
    }

    public void ShowAlert(string a_message)
    {
        m_t = Time.time;
        m_txt.text = a_message;

        Color txtColor = m_txt.color;
        txtColor.a = 1;
        m_txt.color = txtColor;
    }

	void Update () {
        // If we need to fade the text
		if (m_txt.color.a > 0f)
        {
            // If the fade delay has expire
            if (Time.time - m_t > m_fadeDelay)
            {
                // Start lerping the text alpha
                Color txtColor = m_txt.color;
                txtColor.a = Mathf.Lerp(1f, 0f, (Time.time - m_fadeDelay - m_t) * m_fadeSpeedTime);
                m_txt.color = txtColor;
            }
        }
	}
}
