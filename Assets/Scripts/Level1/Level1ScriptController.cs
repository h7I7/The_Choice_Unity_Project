using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1ScriptController : MonoBehaviour {

    //\===========================================================================================
    //\ Variables
    //\===========================================================================================

    #region Variables
    private GameObject m_player;        // Game object of the player
    [SerializeField]
    private ColourShaderUpdater m_clm;  // The colour shader manager 
    #endregion

    //\===========================================================================================
    //\ Unity Methods
    //\===========================================================================================

    #region Unity Methods
    private void Awake()
    {
        // Getting the player object
        m_player = GameObject.FindGameObjectWithTag("Player");
    }

    // Use this for initialization
    void Start () {
		
	}
    #endregion
}
