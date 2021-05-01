using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class MusicSettings
{
    public int level;
    public float StartingTime;
    [Range(0f, 1f)]
    public float volume;
    public AudioClip song;
}

public class MusicManager : MonoBehaviour {

    private int m_level;

    private AudioSource m_aSrc;

    [SerializeField]
    [Range(0f, 1f)]
    private float m_globaVolume;
    private float m_realVolume;

    [SerializeField]
    private float m_musicFadeSpeed;

    [SerializeField]
    private List<MusicSettings> m_musicSettings;

    private void Awake()
    {
        m_aSrc = GetComponent<AudioSource>();
        m_level = SceneManager.GetActiveScene().buildIndex;

        m_realVolume = m_aSrc.volume;
        ChangeSong();
    }

    // Called when a level changes
    public void UpdateLevelInt(int a_level)
    {
        m_level = a_level;

        StartCoroutine(FadeMusicOut());
    }

    private IEnumerator FadeMusicOut()
    {
        float t = Time.time;

        while (m_aSrc.volume != 0)
        {
            m_aSrc.volume = Mathf.Lerp(m_realVolume, 0f, (Time.time - t) * m_musicFadeSpeed);

            yield return null;
        }

        ChangeSong();
    }

    private void ChangeSong()
    {
        m_aSrc.time = 0f;

        foreach(MusicSettings setting in m_musicSettings)
        {
            if (setting.level == m_level)
            {
                m_aSrc.clip = setting.song;
                m_realVolume = m_globaVolume * setting.volume;
                m_aSrc.Play();
                m_aSrc.time = setting.StartingTime;
                break;
            }
        }

        StartCoroutine(FadeMusicIn());
    }

    private IEnumerator FadeMusicIn()
    {
        float t = Time.time;

        while (m_aSrc.volume != m_realVolume)
        {
            m_aSrc.volume = Mathf.Lerp(0f, m_realVolume, (Time.time - t) * m_musicFadeSpeed);

            yield return null;
        }
    }
}
