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

    // Music Manager
    private MusicManager m_musicManager;

    // Objectives manager
    private ObjectiveManager m_objectiveManager;

    // Background loading
    private AsyncOperation m_nextScene;
    private AsyncOperation m_previousScene;

    // Scene data object
    [SerializeField]
    private SceneData m_sceneData;
    public SceneData SceneData
    {
        get { return m_sceneData; }
    }

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

        // Getting the music manager
        m_musicManager = GetComponent<MusicManager>();

        // Getting the objective manager
        m_objectiveManager = GetComponent<ObjectiveManager>();

        m_nextScene = new AsyncOperation();
        m_previousScene = new AsyncOperation();
    }

    // Called when a level is loaded
    public void SetSceneData(SceneData a_data)
    {
        m_sceneData = a_data;

        m_objectiveManager.LoadNewLevelObjectives();
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
        while (m_sceneData == null)
            yield return null;

        if (m_sceneData.NextScene >= 0 && m_sceneData.NextScene <= SceneManager.sceneCountInBuildSettings)
        {
            // Load the scene in the background
            m_nextScene = SceneManager.LoadSceneAsync(m_sceneData.NextScene);
            // This can be set to true to load the next scene
            m_nextScene.allowSceneActivation = false;

            while (m_nextScene.progress < 0.9f)
            {
                yield return null;
            }
        }

        if (m_sceneData.PreviousScene >= 0 && m_sceneData.PreviousScene <= SceneManager.sceneCountInBuildSettings)
        {
            // Load the scene in the background
            m_previousScene = SceneManager.LoadSceneAsync(m_sceneData.PreviousScene);
            // This can be set to true to load the next scene
            m_previousScene.allowSceneActivation = false;
            
            while (m_previousScene.progress < 0.9f)
            {
                yield return null;
            }
        }
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
