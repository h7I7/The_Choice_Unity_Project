using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class InteractionSettings
{
    public string tagIdentifier;
    public Color highlightColor;
    public float highlightWidth;
    public Sprite cursorHoverImage;
    public UnityEvent callback;
}

public enum dialogueEnumerator
{
    Setup = 0,
    Talking,
    Options,
    Exit
}

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(AudioSource))]
public class InteractionController : MonoBehaviour
{
    // An instance of this object
    public static InteractionController instance;

    // GUI objects for cutscenes and prompts
    [Header("GUI Objects")]
    [SerializeField]
    private Image m_crosshair;

    [SerializeField]
    private Image m_hoverImage;

    [SerializeField]
    private Image m_dialogueBackground;

    [SerializeField]
    private Text m_dialogueName;

    [SerializeField]
    private Text m_dialogueText;

    [SerializeField]
    private Scrollbar m_dialogueScrollbar;

    [SerializeField]
    private GameObject m_dialogueButtonsContainer;

    [SerializeField]
    private GameObject m_dialogueButtonPrefab;

    [SerializeField]
    private GameObject m_pickupInstructions;

    // Raycasting options for interaction
    [Space(5f)]
    [Header("RayCasting Options")]
    [SerializeField]
    [Range(1f, 3f)]
    private float m_interactDistance = 2f;

    [SerializeField]
    private KeyCode m_interactKey;

    [SerializeField]
    private float m_lookTowardsSpeed;

    [SerializeField]
    private float m_lookTowardsFOV;
    private float m_FOV;

    [SerializeField]
    private bool m_debugRaycast = false;


    // Interaction settings
    [Space(5f)]
    [Header("RayCasting Options")]
    [SerializeField]
    private LayerMask m_layerMask;

    [SerializeField]
    private List<InteractionSettings> m_interactionSettings;

    [SerializeField]
    private Color m_highlightColor;
    [SerializeField]
    [Range(0.1f, 1.5f)]
    private float m_highlightWidth;
    private Color m_prevOutlineColor;
    private float m_prevOutlineWidth;

    // Dialogue settings
    [Space(5f)]
    [Header("Dialogue Options")]
    [SerializeField]
    private float m_textDelay;

    [SerializeField]
    private float m_timeBetweenSentances;

    [SerializeField]
    private float m_dialogueTransitionSpeed;


    // Where the camera should look at in a cutscene
    private Transform m_lookPoint;
    // The current dialogue node
    private DNode m_dialogue;
    // A stack of previous dialogues
    private Stack<DNode> m_dialoguePrev;
    // A string for dialogue text to oerwrite the current text
    private List<string> m_dialogueTextOverwrite;


    // The player camera
    private Transform m_playerCamera;


    // Raycasting variables
    private RaycastHit hitInRange;  // The raycast that has a maximum distance
    private RaycastHit hitInf;      // A raycast that extends forever

    // Object holding settings
    [Space(5f)]
    [Header("Object holding settings")]
    [SerializeField]
    private float m_minHoldDist = 1f;
    [SerializeField]
    private float m_maxHoldDist = 3f;
    private float m_heldDist = 2f;

    [SerializeField]
    private KeyCode m_rotateKey;

    [SerializeField]
    private float m_rotateSpeed;

    // Audio settings
    [Space(5f)]
    [Header("Audio settings")]
    // Audio source
    private AudioSource m_aSrc;

    // Dialogue variable
    private bool m_dialogueIEnumRunning = false;
    private bool m_skipSentance = false;
    private dialogueEnumerator m_dialogueEnum = dialogueEnumerator.Setup;

    private float m_interactObjectOutlineBeforeDialogue;

    enum InteractionState
    {
        None = 0,
        Talking,
        Lifting
    }

    private InteractionState m_interactionState;

    private void Start()
    {
        // Setting the instance
        if (instance == null)
            instance = this;

        // Setting the player camera variable
        m_playerCamera = transform.GetChild(0);

        // Setting the audio source
        if (m_aSrc == null)
            m_aSrc = GetComponent<AudioSource>();

        m_interactionState = InteractionState.None;
    }

    private void Update()
    {
        switch (m_interactionState)
        {
            case InteractionState.None:
                {
                    // Setting up the ray
                    Ray r = new Ray(m_playerCamera.position, m_playerCamera.forward);

                    if (hitInf.collider != null)
                    {
                        for (int i = 0; i < m_interactionSettings.Count; ++i)
                        {
                            if (hitInf.collider.tag == m_interactionSettings[i].tagIdentifier)
                            {
                                SetObjectMaterialFloat(hitInf.collider.gameObject, "_OutlineWidth", m_prevOutlineWidth);
                                SetObjectMaterialColor(hitInf.collider.gameObject, "_OutlineColor", m_prevOutlineColor);

                                //hitInf.collider.gameObject.GetComponent<Renderer>().material.SetFloat();
                                //hitInf.collider.gameObject.GetComponent<Renderer>().material.SetColor();
                            }
                        }
                    }

                    // Removing previous outlines and settings
                    if (hitInRange.collider != null)
                    {
                        for (int i = 0; i < m_interactionSettings.Count; ++i)
                        {
                            if (hitInRange.collider.tag == m_interactionSettings[i].tagIdentifier)
                            {
                                m_hoverImage.gameObject.SetActive(false);
                            }
                        }
                    }

                    // Raycasting for interactable objects
                    Physics.Raycast(r, out hitInRange, m_interactDistance, m_layerMask);
                    // Raycasting to see if we need to highlight objects in the cursor
                    Physics.Raycast(r, out hitInf, m_layerMask);

                    // Debug ray
                    if (m_debugRaycast)
                        Debug.DrawRay(r.origin, r.direction.normalized * m_interactDistance, Color.red);

                    if (hitInf.collider != null)
                    {
                        for (int i = 0; i < m_interactionSettings.Count; ++i)
                        {
                            if (hitInf.collider.tag == m_interactionSettings[i].tagIdentifier)
                            {
                                m_prevOutlineColor = GetObjectMaterialColor(hitInf.collider.gameObject, "_OutlineColor");
                                m_prevOutlineWidth = GetObjectMaterialFloat(hitInf.collider.gameObject, "_OutlineWidth");

                                SetObjectMaterialFloat(hitInf.collider.gameObject, "_OutlineWidth", m_interactionSettings[i].highlightWidth);
                                SetObjectMaterialColor(hitInf.collider.gameObject, "_OutlineColor", m_interactionSettings[i].highlightColor);
                            }
                        }
                    }

                    if (hitInRange.collider != null)
                    {
                        for (int i = 0; i < m_interactionSettings.Count; ++i)
                        {
                            if (hitInRange.collider.tag == m_interactionSettings[i].tagIdentifier)
                            {
                                m_hoverImage.sprite = m_interactionSettings[i].cursorHoverImage;
                                m_hoverImage.gameObject.SetActive(true);

                                if (Input.GetKeyDown(m_interactKey))
                                {
                                    m_interactionSettings[i].callback.Invoke();
                                }
                            }
                        }
                    }

                    break;
                }

            case InteractionState.Talking:
                {
                    // If there is an NPC talking and we press the interaction key then skip the dialogue
                    if (Input.GetKeyDown(m_interactKey) && m_dialogueEnum == dialogueEnumerator.Talking)
                    {
                        m_skipSentance = true;
                    }

                    // If there is no IEnumerator running
                    if (!m_dialogueIEnumRunning)
                    {
                        // Set the variable to true
                        m_dialogueIEnumRunning = true;

                        // Do once of the following
                        switch (m_dialogueEnum)
                        {
                            case dialogueEnumerator.Setup:
                                {
                                    StartCoroutine(SetupDialogue());
                                    break;
                                }

                            case dialogueEnumerator.Talking:
                                {
                                    StartCoroutine(DisplayDialogue());
                                    break;
                                }

                            case dialogueEnumerator.Options:
                                {
                                    DisplayOptions();
                                    break;
                                }

                            case dialogueEnumerator.Exit:
                                {
                                    StartCoroutine(EndDialogue());
                                    break;
                                }
                        }
                    }

                    break;
                }

            case InteractionState.Lifting:
                {
                    if (Input.GetKeyDown(m_interactKey))
                    {
                        // Starting the player controller movement
                        GetComponent<PlayerController>().m_stopMovement = false;
                        EndPickup();
                    }

                    // Rotating object while holding it
                    if (Input.GetKey(m_rotateKey))
                    {
                        // Stoping the player controller movement
                        GetComponent<PlayerController>().m_stopMovement = true;

                        // Setting the rotation for the object and applying
                        float rotX = -Input.GetAxis("Mouse X") * m_rotateSpeed;
                        float rotY = Input.GetAxis("Mouse Y") * m_rotateSpeed;

                        hitInRange.transform.RotateAroundLocal(m_playerCamera.up, Mathf.Deg2Rad * rotX);
                        hitInRange.transform.RotateAroundLocal(m_playerCamera.right, Mathf.Deg2Rad * rotY);

                        // Resetting the rotation variable
                        m_heldObjRot = hitInRange.transform.rotation;
                    }
                    else
                    {
                        // Starting the player controller movement
                        GetComponent<PlayerController>().m_stopMovement = false;
                    }

                    m_heldDist = Mathf.Clamp(m_heldDist + (Input.GetAxis("Mouse ScrollWheel") * 0.2f), m_minHoldDist, m_maxHoldDist);
                    Vector3 pos = m_playerCamera.position + (m_playerCamera.forward * m_heldDist);

                    float dist = Vector3.Distance(hitInRange.transform.position, pos);
                    hitInRange.collider.GetComponent<Rigidbody>().velocity = (pos - hitInRange.transform.position).normalized * dist * 10f;

                    hitInRange.collider.transform.rotation = m_heldObjRot;

                    break;
                }
        }
    }

    #region Material Setters and Getters
    private void SetObjectMaterialFloat(GameObject a_obj, string a_name, float a_amount)
    {
        Renderer rnd = a_obj.GetComponent<Renderer>();

        if (rnd != null)
            rnd.material.SetFloat(a_name, a_amount);

        if (a_obj.transform.childCount > 0)
        {
            for (int i = 0; i < a_obj.transform.childCount; ++i)
            {
                SetObjectMaterialFloat(a_obj.transform.GetChild(i).gameObject, a_name, a_amount);
            }
        }
    }

    private void SetObjectMaterialColor(GameObject a_obj, string a_name, Color a_color)
    {
        Renderer rnd = a_obj.GetComponent<Renderer>();

        if (rnd != null)
            rnd.material.SetColor(a_name, a_color);

        if (a_obj.transform.childCount > 0)
        {
            for (int i = 0; i < a_obj.transform.childCount; ++i)
            {
                SetObjectMaterialColor(a_obj.transform.GetChild(i).gameObject, a_name, a_color);
            }
        }
    }

    private float GetObjectMaterialFloat(GameObject a_obj, string a_name)
    {
        Renderer rnd = a_obj.GetComponent<Renderer>();

        if (rnd != null)
            return rnd.material.GetFloat(a_name);
        else
        {
            if (a_obj.transform.childCount > 0)
            {
                for(int i = 0; i < a_obj.transform.childCount; ++i)
                {
                    return GetObjectMaterialFloat(a_obj.transform.GetChild(i).gameObject, a_name);
                }
            }
        }

        return -1;
    }

    private Color GetObjectMaterialColor(GameObject a_obj, string a_name)
    {
        Renderer rnd = a_obj.GetComponent<Renderer>();

        if (rnd != null)
            return rnd.material.GetColor(a_name);
        else
        {
            if (a_obj.transform.childCount > 0)
            {
                for (int i = 0; i < a_obj.transform.childCount; ++i)
                {
                    return GetObjectMaterialColor(a_obj.transform.GetChild(i).gameObject, a_name);
                }
            }
        }

        return new Color();
    }
    #endregion

    #region Dialogue Code
    // This sets up the variables we need when entering dialogue
    public void StartDialogue()
    {
        // Set the interaction state
        m_interactionState = InteractionState.Talking;

        // Unlock the cursor
        Cursor.lockState = CursorLockMode.None;

        // Deactivate the crosshair
        m_crosshair.gameObject.SetActive(false);
        // Deactivate the hover image
        m_hoverImage.gameObject.SetActive(false);
        // Set the look point
        m_lookPoint = hitInRange.transform.gameObject.GetComponent<DialogueNode>().LookPoint;

        UpdateDialogueName();

        // Reset the text
        m_dialogueText.text = "";

        // Get the dialogue we beed
        m_dialogue = hitInRange.collider.GetComponent<DialogueNode>().Dialogue[0];

        // Creating a new stack for the previous dialogues
        m_dialoguePrev = new Stack<DNode>();

        // Getting the outline width
        m_interactObjectOutlineBeforeDialogue = GetObjectMaterialFloat(hitInRange.collider.gameObject, "_OutlineWidth");

        // Setting this enum so that we move into the setup
        // IEnumerator after this function
        m_dialogueEnum = dialogueEnumerator.Setup;
    }

    // Setting up dialogue
    // This just tweens the alpha of all the GUI we need, Lerps the player
    // camera direction towards to the look point position, and Lerps the
    // outline of the target
    private IEnumerator SetupDialogue()
    {
        float t = Time.time;

        // For lerping where the player is looking
        Vector3 directionFromCamera = m_lookPoint.position - m_playerCamera.position;
        Quaternion cameraRotation = Quaternion.LookRotation(directionFromCamera);

        float cameraDot = Vector3.Dot(directionFromCamera.normalized, m_playerCamera.forward);

        // For lerping FOV
        m_FOV = Camera.main.fieldOfView;

        // For lerping the GUI alpha
        float alpha = 0f;

        // For lerping the target outline
        float m_interactObjectOutlineBeforeDialogue = GetObjectMaterialFloat(hitInRange.collider.gameObject, "_OutlineWidth");

        float outln = m_interactObjectOutlineBeforeDialogue;

        yield return null;

        while (cameraDot < 0.9f || alpha < 0.9f || Camera.main.fieldOfView > m_lookTowardsFOV || outln > m_interactObjectOutlineBeforeDialogue * 0.25f)
        {
            // Using the constant 0.015f for lerping replaces the need for Time.deltatime
            // I would prefer to use deltatime however that produces some anomalies
            // in the transition

            // Look rotation
            m_playerCamera.rotation = Quaternion.Slerp(m_playerCamera.rotation, cameraRotation, 0.015f * m_lookTowardsSpeed * 2);            
            cameraDot = Vector3.Dot(directionFromCamera.normalized, m_playerCamera.forward);

            float lerp_t = (Time.time - t) * 0.015f * m_dialogueTransitionSpeed;

            // FOV
            Camera.main.fieldOfView = Mathf.Lerp(m_FOV, m_lookTowardsFOV, lerp_t);

            // GUI alpha
            alpha = Mathf.Lerp(0f, 1f, lerp_t);

            Color lerpColorBG = m_dialogueBackground.color;
            lerpColorBG.a = alpha;
            m_dialogueBackground.color = lerpColorBG;

            Color lerpColorName = m_dialogueName.color;
            lerpColorName.a = alpha;
            // These colours are the same so why not save some processing time and use the same one
            m_dialogueName.color = lerpColorName;
            m_dialogueText.color = lerpColorName;

            // Target outline
            SetObjectMaterialFloat(hitInRange.collider.gameObject, "_OutlineWidth", Mathf.Lerp(outln, m_interactObjectOutlineBeforeDialogue * 0.25f, lerp_t));
            //hitInRange.collider.gameObject.GetComponent<Renderer>().material.SetFloat("_OutlineWidth", Mathf.Lerp(outln, m_interactObjectOutlineBeforeDialogue * 0.25f, lerp_t));

            outln = GetObjectMaterialFloat(hitInRange.collider.gameObject, "_OutlineWidth");
            //outln = hitInRange.collider.gameObject.GetComponent<Renderer>().material.GetFloat("_OutlineWidth");

            yield return null;
        }


        // Set these variables
        m_dialogueEnum = dialogueEnumerator.Talking;
        m_dialogueIEnumRunning = false;
    }

    // Upon ending the dialogue we will fade the GUI out
    // and Lerp the target outline to what it was before
    // the dialogue
    private IEnumerator EndDialogue()
    {
        float t = Time.time;

        // For lerping the GUI alpha
        float alpha = 1f;

        // For lerping the target outline
        float outln = GetObjectMaterialFloat(hitInRange.collider.gameObject, "_OutlineWidth");
        //float outln = hitInRange.collider.gameObject.GetComponent<Renderer>().material.GetFloat("_OutlineWidth");

        // Exiting dialogue
        while (alpha > 0f || Camera.main.fieldOfView < m_FOV || outln < m_interactObjectOutlineBeforeDialogue)
        {
            float lerp_t = (Time.time - t) * 0.015f * m_dialogueTransitionSpeed;

            // FOV
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, m_FOV, lerp_t);

            // GUI alpha
            alpha = Mathf.Lerp(1f, 0f, lerp_t);

            Color lerpColorBG = m_dialogueBackground.color;
            lerpColorBG.a = alpha;
            m_dialogueBackground.color = lerpColorBG;

            Color lerpColorName = m_dialogueName.color;
            lerpColorName.a = alpha;
            // These colours are the same so why not save some processing time and use the same one
            m_dialogueName.color = lerpColorName;
            m_dialogueText.color = lerpColorName;

            // Target outline
            SetObjectMaterialFloat(hitInRange.collider.gameObject, "_OutlineWidth", Mathf.Lerp(outln, m_interactObjectOutlineBeforeDialogue, lerp_t));
            //hitInRange.collider.gameObject.GetComponent<Renderer>().material.SetFloat("_OutlineWidth", Mathf.Lerp(outln, m_interactObjectOutlineBeforeDialogue, lerp_t));

            outln = GetObjectMaterialFloat(hitInRange.collider.gameObject, "_OutlineWidth");
            //outln = hitInRange.collider.gameObject.GetComponent<Renderer>().material.GetFloat("_OutlineWidth");

            yield return null;
        }

        m_crosshair.gameObject.SetActive(true);
        m_hoverImage.gameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;

        // Set the interaction state
        m_interactionState = InteractionState.None;

        m_dialogueIEnumRunning = false;
    }

    // Displaying dialogue text slowly, can be interupted by pressing the interaction key
    private IEnumerator DisplayDialogue()
    {
        UpdateDialogueName();

        // A variable used for tracking time,
        // I could use
        // yield return new WaitForSeconds(time)
        // however I want to check for button presses while waiting
        float t = 0f;

        // Set the sentances to be displayed.
        // This should just be the current dialogue text
        List<string> sentancesToBeDisplayed = m_dialogue.dialogueText;

        // However if the dialogue overwrite is not null then display that instead
        if (m_dialogueTextOverwrite != null)
        {
            sentancesToBeDisplayed = m_dialogueTextOverwrite;

        }

        // For each sentance that we need to display
        foreach (string sentance in sentancesToBeDisplayed)
        {
            // Reset the text
            m_dialogueText.text = "";

            // For each letter in that sentance
            foreach (char letter in sentance)
            {
                // If we have pressed the interaction key this will be true
                if (m_skipSentance)
                {
                    // Load all the text
                    m_dialogueText.text = sentance;
                    // Reset the variable
                    m_skipSentance = false;
                    // Break out of the loop since we don't need to load any more letters
                    break;
                }

                // Add a letter to the dialogue text
                m_dialogueText.text = m_dialogueText.text + letter;

                // Player audio for the letters
                m_aSrc.pitch = UnityEngine.Random.Range(hitInRange.collider.gameObject.GetComponent<DialogueNode>().VoicePitch.x, hitInRange.collider.gameObject.GetComponent<DialogueNode>().VoicePitch.y);
                m_aSrc.PlayOneShot(hitInRange.collider.gameObject.GetComponent<DialogueNode>().VoiceSound);

                // Wait for the amount of time between 
                // For this we could use 
                // yield return new WaitForSeconds(time)
                // but I don't think it really matters,
                // they have the same effect and probably do the same thing
                t = Time.time + m_textDelay;
                while(Time.time < t)
                {
                    yield return null;
                }

            }

            // Waiting for time in between sentances
            // This one does actually need to be like this
            // and not a WaitForSeconds as we need to check
            // if we need to move to the next sentance.
            // The delay on this is pretty small (probably 0.25 seconds)
            // however I wanted to make sure there is enough time
            // for the m_skipSentance variable to be reset so
            // the player doesn't accidentally skip any dialogue
            t = Time.time + m_timeBetweenSentances;
            while (Time.time < t || !m_skipSentance)
            {
                yield return null;
            }

            m_skipSentance = false;
        }

        // Reset the dialogue sentances overwrite
        m_dialogueTextOverwrite = null;

        // Set the variables
        m_dialogueEnum = dialogueEnumerator.Options;
        m_dialogueIEnumRunning = false;
    }

    // List of the buttons that are going to be created for the dialogue options
    private List<GameObject> buttons;
    // Displaying all the dialogue options
    private void DisplayOptions()
    {
        UpdateDialogueName("Violet");

        // Reset the dialogue text
        m_dialogueText.text = "";

        // Removing all content from last list of options
        for (int i = 0; i < m_dialogueButtonsContainer.transform.childCount; ++i)
        {
            Destroy(m_dialogueButtonsContainer.transform.GetChild(i).gameObject);
        }

        // Create a new list for the buttons
        buttons = new List<GameObject>();

        // For all the options create new game objects from a button prefab and add them
        // to the button list
        for (int i = 0; i < m_dialogue.options.Count; ++i)
        {
            GameObject btn = Instantiate(m_dialogueButtonPrefab, m_dialogueButtonsContainer.transform);
            btn.transform.GetChild(0).GetComponent<Text>().text = m_dialogue.options[i].optionText;

            // Depending on if a dialogue options has already been selected we will
            // change the colour
            if (m_dialogue.options[i].hasBeenRead)
            {
                btn.transform.GetChild(0).GetComponent<Text>().color = Color.grey;
            }
            else
            {
                btn.transform.GetChild(0).GetComponent<Text>().color = Color.white;
            }

            // Add the button
            buttons.Add(btn);
        }

        // If this is the first piece of dialogue in an option we will need a back option
        // so only add it if it is not the first dialogue options set
        if (!m_dialogue.FirstDialogue)
        {
            GameObject backBtn = Instantiate(m_dialogueButtonPrefab, m_dialogueButtonsContainer.transform);
            backBtn.transform.GetChild(0).GetComponent<Text>().text = "[BACK]";
            buttons.Add(backBtn);
        }

        // Add an end dialogue button so the player can exit at any time
        GameObject endBtn = Instantiate(m_dialogueButtonPrefab, m_dialogueButtonsContainer.transform);
        endBtn.transform.GetChild(0).GetComponent<Text>().text = "[END DIALOGUE]";
        buttons.Add(endBtn);

        // Set the GUI objects to stop displaying dialogue text and 
        // start displaying dialogue options
        m_dialogueText.gameObject.SetActive(false);
        m_dialogueButtonsContainer.transform.parent.transform.parent.gameObject.SetActive(true);

        // Set the scrollbar position to the top
        m_dialogueScrollbar.value = 1f;
    }

    // This method gets called when a button is pressed through a script on the buttons that
    // sends its sibling index
    public void OptionsButtonClick(int a_buttonIndex)
    {
        // Set the GUI objects to start displaying dialogue text and 
        // stop displaying dialogue options
        m_dialogueText.gameObject.SetActive(true);
        m_dialogueButtonsContainer.transform.parent.transform.parent.gameObject.SetActive(false);

        // If the button that has been pressed is last in the list then it is the exit
        // dialogue button so this just exits the dialogue
        if (a_buttonIndex == m_dialogueButtonsContainer.transform.childCount - 1)
        {
            m_dialogueEnum = dialogueEnumerator.Exit;
            m_dialogueIEnumRunning = false;
            return;
        }

        // We can do the same for the back button here if this is not the
        // first dialogue
        if (!m_dialogue.FirstDialogue && a_buttonIndex == m_dialogueButtonsContainer.transform.childCount - 2)
        {
            m_dialogue = m_dialoguePrev.Pop();
            m_dialogueEnum = dialogueEnumerator.Options;
            m_dialogueIEnumRunning = false;
            return;
        }

        // Set the dialogue options so it has been read/selected
        m_dialogue.options[a_buttonIndex].hasBeenRead = true;

        // Invoke the events that are set to this dialogue option
        m_dialogue.options[a_buttonIndex].eventsOnDialogueFinish.Invoke();

        // If we don't need to return to the dialogue once an option is read then set the next dialogue option
        if (!m_dialogue.options[a_buttonIndex].ReturnOnceRead)
        {
            // We have a stack of dialogues so that we can go back, we add the current one
            // here before it is set to something else
            m_dialoguePrev.Push(m_dialogue);

            // Set the dialogue to the net dialogue
            foreach(DNode dialogue in hitInRange.collider.GetComponent<DialogueNode>().Dialogue)
            {
                if (dialogue._ID == m_dialogue.options[a_buttonIndex].nextID)
                {
                    m_dialogue = dialogue;
                    break;
                }
            }            
        }
        // Else set the dialogue text overwrite
        else
        {
            foreach (DNode dialogue in hitInRange.collider.GetComponent<DialogueNode>().Dialogue)
            {
                if (dialogue._ID == m_dialogue.options[a_buttonIndex].nextID)
                {
                    m_dialogueTextOverwrite = dialogue.dialogueText;
                    break;
                }
            }
        }


        // Set the variables
        m_dialogueEnum = dialogueEnumerator.Talking;
        m_dialogueIEnumRunning = false;
    }

    public void UpdateDialogueName(string a_name = null)
    {
        if (a_name == null)
        {
            // Get the object used for dialogue text
            m_dialogueName.text = hitInRange.collider.gameObject.GetComponent<DialogueNode>().Name;
        }
        else
        {
            m_dialogueName.text = a_name;
        }
    }
    #endregion

    #region Pickup Code
    private Quaternion m_heldObjRot;

    public void StartPickup()
    {
        // Set the interaction state
        m_interactionState = InteractionState.Lifting;

        // Turn off object gravity
        hitInRange.collider.gameObject.GetComponent<Rigidbody>().useGravity = false;

        // Getting object rotation
        m_heldObjRot = hitInRange.collider.transform.rotation;

        // UI instructions
        m_pickupInstructions.SetActive(true);

        // Deactivate the crosshair
        m_crosshair.gameObject.SetActive(false);
        // Deactivate the hover image
        m_hoverImage.gameObject.SetActive(false);
    }

    private void EndPickup()
    {
        // Set the interaction state
        m_interactionState = InteractionState.None;

        // Turn off object gravity
        hitInRange.collider.gameObject.GetComponent<Rigidbody>().useGravity = true;

        // UI instructions
        m_pickupInstructions.SetActive(false);

        // Deactivate the crosshair
        m_crosshair.gameObject.SetActive(true);
        // Deactivate the hover image
        m_hoverImage.gameObject.SetActive(true);
    }
    #endregion

    #region Door Interaction Code
    public void OpenDoor()
    {
        hitInRange.collider.gameObject.GetComponent<DoorBehaviour>().Open();
    }    
    #endregion

    #region Misc
    public void NextLevel()
    {
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().ActivateNextScene();
    }
    #endregion
}
