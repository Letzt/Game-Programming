using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HOFScript : MonoBehaviour
{

	public Text winnersList;
    // Start is called before the first frame update
    void Start()
    {
        winnersList = GameObject.Find("Winners").GetComponent<Text>();
				winnersList.text = GameManager.final_list;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
