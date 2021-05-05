using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoorBehaviour : MonoBehaviour {

    [SerializeField]
    private bool m_locked = true;
    public bool Locked
    {
        get { return m_locked; }
    }

    [SerializeField]
    [TextArea(3, 4)]
    private string m_doorLockedText;

    [SerializeField]
    private AlertFade m_alert;

    [SerializeField]
    private UnityEvent m_callback;

    public void Open()
    {
        if (m_locked)
        {
            m_alert.ShowAlert(m_doorLockedText);
        }
        else
        {
            m_callback.Invoke();
        }
    }

    public void Unlock()
    {
        m_locked = false;
    }
}
