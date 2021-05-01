using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsButtonController : MonoBehaviour {

    private Button btn;

    private void Start()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(ButtonClicked);
    }

    public void ButtonClicked()
    {
        InteractionController.instance.OptionsButtonClick(transform.GetSiblingIndex());
    }
}
