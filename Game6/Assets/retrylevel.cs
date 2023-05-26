using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class retrylevel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RetryLevel() {
        // Debug.Log("works!!!!!!!");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //picks the same build
    }
}
