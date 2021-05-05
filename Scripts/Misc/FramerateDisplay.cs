// Code shamelessly ripped from
//http://wiki.unity3d.com/index.php/FramesPerSecond
// Why reinvent the wheel?

using UnityEngine;

public class FramerateDisplay : MonoBehaviour
{
    [SerializeField]
    private Vector4 m_textColor;

    private float deltaTime = 0.0f;

    void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(m_textColor.x, m_textColor.y, m_textColor.z, m_textColor.w);
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}