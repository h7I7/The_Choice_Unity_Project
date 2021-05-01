using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePos : MonoBehaviour {

    public Transform track;

	// Update is called once per frame
	void FixedUpdate () {

        GetComponent<Renderer>().material.SetVector("_PlayerPos", track.position);
	}
}
