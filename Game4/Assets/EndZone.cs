using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndZone : MonoBehaviour
{
    public GameObject endline;
    public GameObject playAgain;
    public Text WOL;

    // public Texture2D texture;
    // Start is called before the first frame update
    void Start()
    {
        endline = GameObject.Find("EndZone");
        playAgain = GameObject.Find("Restart");
        playAgain.gameObject.SetActive(false);

        WOL = GameObject.Find("WOL").GetComponent<Text>();
        WOL.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        ////////////////////////////////////////////////
        // WRITE CODE HERE:
        // if Claire reaches this platform, make it green, make "has_won" true in Claire.cs / see Claire.cs for what to do here
        ////////////////////////////////////////////////
        if(other.gameObject.name == "Claire")
        {
            var endrenderer = endline.GetComponent<Renderer>();
            endrenderer.material.SetColor("_Color", Color.green);

            playAgain.gameObject.SetActive(true);

            Claire.has_won = true;
            WOL.gameObject.SetActive(true);
            Debug.Log("Game Condition Reached!");
        }
    }
}
