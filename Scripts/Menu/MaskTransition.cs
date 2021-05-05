using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskTransition : MonoBehaviour {

    //////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////
    #region Variables
    // An instance of this script
    public static List<MaskTransition> instances;

    // Variables for lerping the position of this object
    [SerializeField]
    private float m_moveSpeed;

    private Vector3 m_startingPos;
    private Vector3 m_targetPosOffset;

    // Time keeper
    private float m_t;

    #endregion

    //////////////////////////////////////////////////
    // Functions
    //////////////////////////////////////////////////
    #region Functions
    void Awake()
    {
        // Setting the instance and adding this script to the instances
        if (instances == null)
        {
            instances = new List<MaskTransition>();
            instances.Add(this);
        }
        else
            instances.Add(this);
    }

    void Start()
    {
        // Setting the time keeper
        m_t = Time.time;

        // Setting the starting position of this object
        m_startingPos = transform.localPosition;
        // Setting the target pos
        m_targetPosOffset = Vector3.zero;
    }

	// Update is called once per frame
	void Update () {

        // Lerping the position of this object
        transform.localPosition = Vector3.Lerp(transform.localPosition, m_startingPos + m_targetPosOffset, (Time.time - m_t) * Time.deltaTime * m_moveSpeed);
	}

    public void ResetTargetPosOffet()
    {
        m_targetPosOffset = Vector3.zero;
        m_t = Time.time;
    }

    public void SetTargetPosOffset(Vector3 a_pos)
    {
        m_targetPosOffset = a_pos;
        m_t = Time.time;
    }

    #endregion
}
