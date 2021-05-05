using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FullscreenShader : MonoBehaviour {

    [SerializeField]
    private Material m_mat;

	void Awake () {
        Camera cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.DepthNormals;
    }
	
	void OnRenderImage(RenderTexture a_src, RenderTexture a_dst)
    {
        if (m_mat != null)
            Graphics.Blit(a_src, a_dst, m_mat);
    }
}
