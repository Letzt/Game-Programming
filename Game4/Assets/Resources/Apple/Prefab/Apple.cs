using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Apple : MonoBehaviour
{
    public Vector3 direction;
    public float velocity;
    public float birth_time;
    public GameObject birth_turret;
    public Claire c;

    // Start is called before the first frame update
    void Start()
    {
        c = GameObject.Find("Claire").GetComponent<Claire>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - birth_time > 10.0f)  // apples live for 10 sec
        {
            Destroy(transform.gameObject);
        }
        transform.position = transform.position + velocity * direction * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        ////////////////////////////////////////////////
        // WRITE CODE HERE:
        // (a) if the object collides with Claire, subtract one life from her, and destroy the apple
        // (b) if the object collides with another apple, or its own turret that launched it (birth_turret), don't do anything
        // (c) if the object collides with anything else (e.g., terrain, a different turret), destroy the apple
        ////////////////////////////////////////////////

        // (a)
        if(other.gameObject.name == "Claire")
        {
            if(c.num_lives!=0)
                c.num_lives--;
            Destroy(this.gameObject);
        }

        // (b)
        else if(other.gameObject.name == "Apple(clone)" || other.gameObject == this.birth_turret)
            if(c.num_lives>=10)
                c.num_lives--; // Don't do anything

        // (c)
        else
        {
            Destroy(this.gameObject);
            // Debug.Log("destroyedddddddd!!!!!!");
        }

    }
}
