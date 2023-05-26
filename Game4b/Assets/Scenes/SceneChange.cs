using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChange : MonoBehaviour
{
	public static string nickname = "";
	public static bool start_menu_loaded;
	// Switch to the desired scene as per the passed parameter.
    public void LoadScene(string sceneName){
		SceneManager.LoadScene(sceneName);
	}
	
	public void ReadInput(string s){
		nickname = s;
		Debug.Log(nickname);
	}
}
