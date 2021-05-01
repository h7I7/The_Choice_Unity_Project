using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour {

    //////////////////////////////////////////////////
    // Variables
    //////////////////////////////////////////////////
    #region Variables

    // An instance of this script
    private static FadeManager instance;
    public static FadeManager Instance
    {
        get { return instance; }
    }

    // Music Manager
    private MusicManager m_musicManager;

    // Objective manager
    private ObjectiveManager m_objectiveManager;

    // Player controller
    private GameObject m_player;

    // Canvas
    private Image m_fadeInOut;
    [SerializeField]
    private Color m_startColor;
    [SerializeField]
    private Color m_fadeToColor;
    [SerializeField]
    private float m_fadeSpeed;

    [Space(5)]
    [SerializeField]
    [Range(0f, 1f)]
    private float m_cameraZoomRate;
    private float m_cameraStartFOV = -1;

    // Synchronisation
    private bool m_ienumRunning = false; 

    #endregion

    //////////////////////////////////////////////////
    // Functions
    //////////////////////////////////////////////////
    #region Functions
    void Awake()
    {
        // If this is the only object if this type set the instance
        // if it isn't then destroy the game object attached to this script
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(this);

        // Getting the music manager
        m_musicManager = GetComponent<MusicManager>();

        // Getting the objective manager
        m_objectiveManager = GetComponent<ObjectiveManager>();

        // Find the fader object, helps when changing scenes
        if (m_fadeInOut == null)
            m_fadeInOut = GameObject.FindGameObjectWithTag("Fader").GetComponent<Image>();

        // Turn on the fade
        m_fadeInOut.gameObject.SetActive(true);
    }

    void Start ()
    {
        FadeInCanvas();

        // Subscribing to the scene manager
        SceneManager.activeSceneChanged += ChangedActiveScene;    
	}

    void ChangedActiveScene(Scene a_current, Scene a_next)
    {
        m_fadeInOut = GameObject.FindGameObjectWithTag("Fader").GetComponent<Image>();
        FadeInCanvas();
    }

    public void FadeInCanvas()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");

        if (m_player != null)
            m_player.GetComponent<PlayerController>().m_stopMovement = true;

        StopObjectCoroutines();

        if (!m_ienumRunning)
            StartCoroutine(FadeInCanvasImage());
    }

    public void FadeOutCanvas(bool m_loadNextScenes = false)
    {
        m_player = GameObject.FindGameObjectWithTag("Player");

        if (m_player != null)
            m_player.GetComponent<PlayerController>().m_stopMovement = true;

        StopObjectCoroutines();

        if (!m_ienumRunning)    
            StartCoroutine(FadeOutCanvasImage(m_loadNextScenes));

        // Changing music
        if (m_loadNextScenes)
        { 
            m_musicManager.UpdateLevelInt(GameManager.instance.SceneData.NextScene);
            m_objectiveManager.FadeOut();
        }
    }
    
    private void StopObjectCoroutines()
    {
        StopCoroutine(FadeInCanvasImage());
        StopCoroutine(FadeOutCanvasImage());
        m_ienumRunning = false;
    }

    private IEnumerator FadeInCanvasImage(UnityEvent a_event = null)
    {
        // Set the IEnumerator is running bool to true
        m_ienumRunning = true;

        // Mark the time
        float t = Time.time;

        if (m_cameraStartFOV > 0)
            Camera.main.fieldOfView = m_cameraStartFOV;

        while (m_fadeInOut.color != m_fadeToColor)
        {
            // Set the color
            Color lerp = Color.Lerp(m_startColor, m_fadeToColor, (Time.time - t) * m_fadeSpeed);

            // Lerps between two colors to fade
            m_fadeInOut.color = lerp;

            // Setting colour of all child objects
            foreach (GameObject child in GetChildren(m_fadeInOut.gameObject))
            {
                if (child.GetComponent<Image>())
                {
                    child.GetComponent<Image>().color = new Color(1 - lerp.r, 1- lerp.g, 1 - lerp.b, lerp.a); 
                }

                if (child.GetComponent<Text>())
                {
                    child.GetComponent<Text>().color = new Color(1 - lerp.r, 1 - lerp.g, 1 - lerp.b, lerp.a);
                }
            }

            yield return null;
        }

        // Invoke the event passed in
        if (a_event != null)
            a_event.Invoke();

        if (m_player != null)
            m_player.GetComponent<PlayerController>().m_stopMovement = false;

        // Set the IEnumerator is running bool to false
        m_ienumRunning = false;
    }

    private IEnumerator FadeOutCanvasImage(bool a_loadNextScenes = false, UnityEvent a_event = null)
    {
        // Set the IEnumerator is running bool to true
        m_ienumRunning = true;
        // Mark the time
        float t = Time.time;

        m_cameraStartFOV = Camera.main.fieldOfView;

        while (m_fadeInOut.color != m_startColor)
        {
            // Setting the color
            Color lerp = Color.Lerp(m_fadeToColor, m_startColor, (Time.time - t) * m_fadeSpeed);

            // Lerps between two colors to fade
            m_fadeInOut.color = lerp;

            // Setting colour of all child objects
            foreach (GameObject child in GetChildren(m_fadeInOut.gameObject))
            {
                if (child.GetComponent<Image>())
                {
                    child.GetComponent<Image>().color = new Color(1 - lerp.r, 1 - lerp.g, 1 - lerp.b, lerp.a);
                }

                if (child.GetComponent<Text>())
                {
                    child.GetComponent<Text>().color = new Color(1 - lerp.r, 1 - lerp.g, 1 - lerp.b, lerp.a);
                }
            }

            // Zooming in camera
            Camera.main.fieldOfView -= m_cameraZoomRate * Time.deltaTime;

            yield return null;
        }

        // Invoke the event passed in
        if (a_event != null)
            a_event.Invoke();

        if (m_player != null)
            m_player.GetComponent<PlayerController>().m_stopMovement = false;

        // Set the IEnumerator is running bool to false
        m_ienumRunning = false;

        // If we need to load the next scenes then load the next scenes
        if (a_loadNextScenes)
        {
            GameManager.instance.ActivateNextScene();
        }
    }

    private List<GameObject> GetChildren(GameObject a_obj)
    {
        List<GameObject> children = new List<GameObject>();

        for(int i = 0; i < a_obj.transform.childCount; ++i)
        {
            children.Add(a_obj.transform.GetChild(i).gameObject);

            if (a_obj.transform.GetChild(i).transform.childCount > 0)
            {
                children.AddRange(GetChildren(a_obj.transform.GetChild(i).gameObject));
            }
        }

        return children;
    }

    #endregion
}
