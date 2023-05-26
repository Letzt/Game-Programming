using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Ballclick : MonoBehaviour
{
	public GameObject cam1, cam2;

	void Start(){
		cam1.SetActive(true);
		cam2.SetActive(false);
	}

	void OnMouseDown(){
		if(this.gameObject.name == "Sphere")
		{
			SceneManager.LoadScene("SampleScene");
		}
	}

	void Update(){
		if(Input.GetKeyDown(KeyCode.Space))
		{
			if(cam1.activeSelf)
			{
				cam1.SetActive(false);
				cam2.SetActive(true);
			}
			else
			{
				cam1.SetActive(true);
				cam2.SetActive(false);
			}
		}
	}
}
