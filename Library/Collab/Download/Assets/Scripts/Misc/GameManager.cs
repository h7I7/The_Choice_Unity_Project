using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    //////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////
    #region Variables
    // An instance of this object
    public static GameManager instance;

    // Background loading
    private AsyncOperation m_nextScene;
    private AsyncOperation m_previousScene;

    // Scene data object
    private SceneData m_sceneData;

    // Scenes ready to load tracker
    private bool m_scenesReady = false;
    #endregion

    //////////////////////////////////////////////////
    // Functions
    //////////////////////////////////////////////////
    #region Functions
    void Awake()
    {
        // Set the instance
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // Don't destroy on load
        DontDestroyOnLoad(this);

        m_nextScene =       new AsyncOperation();
        m_previousScene =   new AsyncOperation();
    }

    void Start()
    {
        m_sceneData = GameObject.FindGameObjectWithTag("SceneData").GetComponent<SceneData>();

        // On scene start load the next scene
        //int currentSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;

        //PreloadScenes(currentSceneBuildIndex);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    void OnLevelFinishedLoading(Scene a_scene, LoadSceneMode a_mode)
    {
        PreloadScenes();
    }

    // Quit the game
    public void Quit()
    {
        Application.Quit();
    }

    public void PreloadScenes()
    {
        m_scenesReady = false;

        // If there is no enumerator running/level loading then start loading one otherwise return false
        StartCoroutine(PreloadSceneIEnum());
    }

    private IEnumerator PreloadSceneIEnum()
    {
        // This can be set to true to load the next scene
        m_nextScene.allowSceneActivation = false;
        // Load the scene in the background
        m_nextScene = SceneManager.LoadSceneAsync(m_sceneData.NextScene);

        while (m_nextScene.progress < 0.9f)
        {
            yield return null;
        }

        //if (a_sceneBuildIndex - 1 > 0)
        //{
        //    // Load the scene in the background
        //    m_previousScene = SceneManager.LoadSceneAsync(a_sceneBuildIndex - 1);
        //    // This can be set to true to load the next scene
        //    m_previousScene.allowSceneActivation = false;
        //
        //    while (m_previousScene.progress < 0.9f)
        //    {
        //        yield return null;
        //    }
        //}
        //else
        //    m_previousScene = null;
    }

    public void ActivateNextScene()
    {
        m_nextScene.allowSceneActivation = true;
    }

    public void ActivatePreviousScene()
    {
        m_previousScene.allowSceneActivation = true;
    }
    #endregion
}
