using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class IrisMovement : MonoBehaviour {

    //////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////
    #region Variables
    // The screen resolution
    private Vector3 m_res;

    // The image of this object
    private Image m_img;

    // The scale which the iris can move
    [SerializeField]
    private Vector3 m_movementScale;

    // The bounds for which the iris can move
    [SerializeField]
    private Vector3 m_movementBounds;
    #endregion

    //////////////////////////////////////////////////
    // Functions
    //////////////////////////////////////////////////
    #region Functions
    void Awake()
    {
        // Setting resolution
        m_res = new Vector3(Camera.main.pixelWidth, Camera.main.pixelHeight, 0f);
        // Setting the image
        m_img = GetComponent<Image>();
    }
	
	// Update is called once per frame
	void Update () {
        // Getting the position the image would have to be to have the mouse in the center of the iris
        Vector3 mousePosToCenterOfImage = Input.mousePosition - (m_res * 0.5f) - new Vector3(0f, m_img.rectTransform.sizeDelta.y * 0.5f, 0f);
        // Setting the position of the iris image
        transform.localPosition = new Vector3(Mathf.Clamp((mousePosToCenterOfImage.x / m_res.x) * m_movementScale.x, -m_movementBounds.x, m_movementBounds.x), Mathf.Clamp((mousePosToCenterOfImage.y / m_res.y) * m_movementScale.y, -m_movementBounds.y, m_movementBounds.y), 0f);
	}
    #endregion
}
