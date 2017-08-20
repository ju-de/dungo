using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	void Start () {
		
	}
	
	void Update () {
		
	}

	public void StartGame() {
		SceneManager.LoadScene("Level");
	}

	public void ShowCredits() {
		// SceneManager.LoadScene("Credits");
	}

	public void QuitGame() {
		Application.Quit();
	}
}
