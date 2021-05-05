using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneData : MonoBehaviour {

    [SerializeField]
    private int m_nextScene;
    public int NextScene
    {
        get { return m_nextScene; }
    }

    [SerializeField]
    private int m_previousScene;
    public int PreviousScene
    {
        get { return m_previousScene; }
    }

    private void Start()
    {
        GameManager.instance.SetSceneData(this);
    }
}
