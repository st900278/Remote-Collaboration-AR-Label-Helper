using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARMarkerList : MonoBehaviour {

    public List<MarkerToGameObject> markers;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

[System.Serializable]
public struct MarkerToGameObject
{
    public int id;
    public GameObject item;
    public string text;
}