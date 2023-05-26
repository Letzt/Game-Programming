using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    private Animator anim;
    private CharacterController controller;

    public float speed = 3000.0f; //speed of astro
    public float turnSpeed = 400.0f;
    private Vector3 moveDirection = Vector3.zero;
    public float gravity = 20.0f;
    bool isGrounded = true;
    float x_terrain;
    float z_terrain;
    // TerrainData terrain = Terrain.activeTerrain.TerrainData;

    void Start()
    {
        x_terrain = Terrain.activeTerrain.terrainData.size.x;
        z_terrain = Terrain.activeTerrain.terrainData.size.z;
        controller = GetComponent<CharacterController>();
        anim = gameObject.GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (controller.transform.position.x < 100){
          controller.Move(-moveDirection * Time.deltaTime);
          // transform.position = new Vector3(500, 0, 500);
          // transform.position = new Vector3(100, transform.position.y, transform.position.z);
           // gameObjectToMove.transform.position = new Vector3(x, y, z);
        }
        if (controller.transform.position.x > (x_terrain - 10)){
          controller.Move(-moveDirection * Time.deltaTime);
        }
        if (controller.transform.position.z < 10){
          controller.Move(-moveDirection * Time.deltaTime);
        }
        if (controller.transform.position.z > (z_terrain - 10)){
          controller.Move(-moveDirection * Time.deltaTime);
        }


        if (Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow))
        {
            anim.SetInteger("AnimationPar", 1);
        }
        else
        {
            anim.SetInteger("AnimationPar", 0);
        }

        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
          // Debug.Log("Space!");
          moveDirection.y += 30.0f;
          isGrounded = false;
        }

        if (controller.isGrounded)
        {
          isGrounded = true;
          moveDirection = transform.forward * Input.GetAxis("Vertical") * speed;
        }

		    moveDirection.y -= gravity * Time.deltaTime;

        float turn = Input.GetAxis("Horizontal");
        transform.Rotate(0, turn * turnSpeed * Time.deltaTime, 0);
        controller.Move(moveDirection * Time.deltaTime);
    }
}
