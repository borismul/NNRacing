using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class ChallengeUIController : MonoBehaviour {

    public Button leaveButton;
	// Use this for initialization
	void Start () {
        leaveButton.onClick.AddListener(Leave);
	}
	
    void Leave()
    {
        GA_Parameters.carUpdateRate = 0;
        SceneManager.LoadScene("MainScene");
    }

}
