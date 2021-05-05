using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Button))]
public class ButtonTransitions : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

    //////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////
    #region Variables
    // Game objects
    [SerializeField]
    private Text[] m_btnText;

    // Transform
    RectTransform rectTransform;

    // Time keeper
    private float m_t;

    // Scaling variables
    private Vector2 m_startingScale;
    private float m_fontStartingSize;

    [Space(10)]
    [SerializeField]
    private float m_rectScaleAmount;
    [SerializeField]
    private float m_fontScaleAmount;

    private Vector2 m_scaleTarget;
    private float m_fontScaleTarget;

    [SerializeField]
    private float m_scaleSpeed;

    // Mask transition variables
    [Space(10)]
    [SerializeField]
    private Vector3 m_maskPosOffsetOnMouseOver;

    // Unity action
    [Space(10)]
    [SerializeField]
    private UnityEvent m_onClick;

    // Button disabling
    [SerializeField]
    private bool m_disableButton = false;
    #endregion

    //////////////////////////////////////////////////
    // Functions
    //////////////////////////////////////////////////
    #region Functions
    void Awake()
    {
        // Setting the rect transform
        rectTransform = GetComponent<RectTransform>();

        // Setting the original scale amount
        m_startingScale = rectTransform.sizeDelta;
        m_fontStartingSize = m_btnText[0].fontSize;
        // Setting the scale target to the original amount
        m_scaleTarget = m_startingScale;
        m_fontScaleTarget = m_fontStartingSize;

        m_t = Time.time;
    }

	void Update () {

        if (m_disableButton)
        {
            m_scaleTarget = rectTransform.sizeDelta;
            foreach (Text txt in m_btnText)
            {
                m_fontScaleTarget = txt.fontSize;
            }
        }

        // Lerping the button scale
        rectTransform.sizeDelta = Vector2.Lerp(rectTransform.sizeDelta, m_scaleTarget, (Time.time - m_t) * Time.deltaTime * m_scaleSpeed);
        foreach (Text txt in m_btnText)
        {
            txt.fontSize = Mathf.CeilToInt(Mathf.Lerp(txt.fontSize, m_fontScaleTarget, (Time.time - m_t) * Time.deltaTime * m_scaleSpeed));
        }
    }

    public void OnPointerEnter(PointerEventData a_eventData)
    {
        // Early out
        if (m_disableButton)
            return;

        // Reseting the time keeper
        m_t = Time.time;

        // Setting the scale target
        m_scaleTarget = m_rectScaleAmount * m_startingScale;
        m_fontScaleTarget = m_fontScaleAmount * m_fontStartingSize;

        // Setting the mask position offset
        foreach (MaskTransition mask in MaskTransition.instances)
        {
            mask.SetTargetPosOffset(m_maskPosOffsetOnMouseOver);
        }
    }

    public void OnPointerExit(PointerEventData a_eventData)
    {
        // Early out
        if (m_disableButton)
            return;

        // Reseting the time keeper
        m_t = Time.time;

        // Setting the scale target
        m_scaleTarget = m_startingScale;
        m_fontScaleTarget = m_fontStartingSize;

        // Resetting the mask position offset
        foreach (MaskTransition mask in MaskTransition.instances)
        {
            mask.ResetTargetPosOffet();
        }
    }

    public void OnPointerClick(PointerEventData a_eventData)
    {
        // Early out
        if (m_disableButton)
            return;

        m_onClick.Invoke();
    }

    public void EnableButton()
    {
        m_disableButton = false;
    }

    public void DisableButton()
    {
        m_disableButton = true;
    }
    #endregion
}
