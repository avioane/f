using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateVersionLabels : MonoBehaviour {
        
    private string vers ="991.0.222";
        
    void Start () {
        PlayerPrefs.SetString("version", vers);

        GameObject.Find("LoginVersionLbl").GetComponent<Text>().text = vers;       

        //GameObject.Find("LoginVersionLbl").GetComponent<Text>().text = PlayerPrefs.GetString("version");
    }
	
	// Update is called once per frame
	void Update () {
		//Canvas.
	}
}
