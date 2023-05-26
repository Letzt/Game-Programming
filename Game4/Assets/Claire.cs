using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Claire : MonoBehaviour {

    private Animator animation_controller;
    private CharacterController character_controller;
    public Vector3 movement_direction;
    public float walking_velocity;
    public Text text;
    public Text WOL;
    public float velocity, velocity_claire, forwardvelocity, backvelocity, crouchforwardvelocity, crouchbackvelocity, runvelocity, jumpvelocity;
    public int num_lives;
    public bool is_crouching;
    public static bool has_won;
    public GameObject playAgain;
    bool isGrounded = true;

	  // Use this for initialization
	  void Start ()
    {
        animation_controller = GetComponent<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        walking_velocity = 1.5f;
        velocity = 0.0f;
        forwardvelocity = 0.0f;
        backvelocity = 0.0f;
        crouchforwardvelocity = 0.0f;
        crouchbackvelocity = 0.0f;
        runvelocity = 0.0f;
        jumpvelocity = 0.0f;
        num_lives = 5;
        has_won = false;

        playAgain = GameObject.Find("Restart");
        WOL = GameObject.Find("WOL").GetComponent<Text>();

        ////////////////////////////////
        double min, max, range, sample, scaled;
        float f;

        min = 0;
        max = 90;
        range = max - min;
        List<float> degreesPerSecondArr = new List<float>(18);
        for (i = 0; i < 18; i++)
        {
            sample = rand.NextDouble();
            scaled = (sample * range) + min;
            degreesPerSecondArr.Add((float)scaled);
        }

        min = 1;
        max = 10;
        range = max - min;
        List<float> amplitudeArr = new List<float>(18);
        for (i = 0; i < 18; i++)
        {
            sample = rand.NextDouble();
            scaled = (sample * range) + min;
            amplitudeArr.Add((float)scaled);
        }

        min = 1;
        max = 7;
        range = max - min;
        List<float> frequencyArr = new List<float>(18);
        for (i = 0; i < 18; i++)
        {
            sample = rand.NextDouble();
            scaled = (sample * range) + min;
            frequencyArr.Add((float)scaled);
        }
        /////////////////////////////////////

    }

    IEnumerator freeze()
    {
        // Debug.Log("enter freeze");
        yield return new WaitForSeconds(3.6f);
        animation_controller.speed = 0;
        // Debug.Log("After 3 seconds" + Time.time);
    }

    IEnumerator jumpover()
    {
        yield return new WaitForSeconds(2.2f);
        isGrounded = true;
        jumpvelocity = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(has_won)
        {
            animation_controller.SetBool("death", false);
            animation_controller.SetBool("is_crouching_up", false);
            animation_controller.SetBool("is_crouching_down", false);
            animation_controller.SetBool("WalkForward", false);
            animation_controller.SetBool("WalkBack", false);
            animation_controller.SetBool("run_forward", false);
            animation_controller.SetBool("jump", false);
            return;
        }

        text.text = "Lives left: " + num_lives;

        bool check_is_dead = animation_controller.GetCurrentAnimatorStateInfo(0).IsName("Death");

        //Increase forward velocity slowly
        if(forwardvelocity < walking_velocity)
            forwardvelocity += 0.005f;

        //Increase backward velocity slowly
        if(backvelocity < walking_velocity/1.5)
            backvelocity += 0.003f; // walks slowly backwards

        if(crouchforwardvelocity < walking_velocity/2.0)
            crouchforwardvelocity += 0.005f;

        if(crouchbackvelocity < walking_velocity/2.0)
            crouchbackvelocity += 0.003f;

        if(runvelocity < walking_velocity*2.0)
            runvelocity +=0.05f;

        if(jumpvelocity < walking_velocity*3.0)
            jumpvelocity +=0.5f;

        ////////////////////////////////////////////////
        // WRITE CODE HERE:
        // (a) control the animation controller (animator) based on the keyboard input. Adjust also its velocity and moving direction.
        // (b) orient (i.e., rotate) your character with left/right arrow [do not change the character's orientation while jumping]
        // (c) check if the character is out of lives, call the "death" state, let the animation play, and restart the game
        // (d) check if the character reached the target (display the message "you won", freeze the character (idle state), provide an option to restart the game
        // feel free to add more fields in the class
        ////////////////////////////////////////////////

        // you don't need to change the code below (yet, it's better if you understand it). Name your FSM states according to the names below (or change both).
        // do not delete this. It's useful to shift the capsule (used for collision detection) downwards.
        // The capsule is also used from turrets to observe, aim and shoot (see Turret.cs)
        // If the character is crouching, then she evades detection.
        bool cmd_press = (Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand) || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));

        bool is_crouching_up = (Input.GetKey("up") && cmd_press);
        animation_controller.SetBool("is_crouching_up", is_crouching_up);

        bool is_crouching_down = (Input.GetKey("down") && cmd_press);
        animation_controller.SetBool("is_crouching_down", is_crouching_down);

        if(is_crouching_up || is_crouching_down)
            is_crouching = true;
        else
            is_crouching = false;

        if (is_crouching)
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, 0.0f, GetComponent<CapsuleCollider>().center.z);
        else
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, 0.9f, GetComponent<CapsuleCollider>().center.z);

        // you will use the movement direction and velocity in Turret.cs for deflection shooting
        float xdirection = Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
        float zdirection = Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
        movement_direction = new Vector3(xdirection, 0.0f, zdirection);

        //Walk Forward Code
        bool walk_forward = Input.GetKey("up");
        animation_controller.SetBool("WalkForward", walk_forward);
        if(walk_forward && !is_crouching_up && !check_is_dead)
            character_controller.Move(transform.forward * forwardvelocity * Time.deltaTime);
        else
            forwardvelocity = 0.0f;

        //Crouch Forward Code
        if(is_crouching_up && !check_is_dead)
            character_controller.Move(transform.forward * crouchforwardvelocity * Time.deltaTime);
        else
            crouchforwardvelocity = 0.0f;

        //Walk Backward Code
        bool walk_backward = Input.GetKey("down");
        animation_controller.SetBool("WalkBack", walk_backward);
        if(walk_backward && !is_crouching_down && !check_is_dead)
            character_controller.Move(-transform.forward * backvelocity * Time.deltaTime);
        else
            backvelocity = 0.0f;

        //Crouch Backward Code
        if(is_crouching_down && !check_is_dead)
            character_controller.Move(-transform.forward * crouchbackvelocity * Time.deltaTime);
        else
            crouchbackvelocity = 0.0f;

        //Run forward code
        bool run_forward =  (Input.GetKey("up") && (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift)));
        animation_controller.SetBool("run_forward", run_forward);
        if(run_forward && !check_is_dead)
            character_controller.Move(transform.forward * runvelocity * Time.deltaTime);
        else
            runvelocity = 0.0f;

        // Jump CODE
        bool jump = Input.GetKey("space");
        animation_controller.SetBool("jump", jump);
        if(jump && isGrounded && !check_is_dead)
        {
            character_controller.Move(transform.up * jumpvelocity * Time.deltaTime); //check line
            isGrounded = false;
            StartCoroutine(jumpover());
        }

        //Right Side Turn Code
        // bool turn_right = Input.GetKey("Right");
        if(Input.GetKey("right") && isGrounded && !check_is_dead)
        {
            transform.Rotate(Vector3.up * 30.0f * Time.deltaTime);
        }

        //Left Side Turn Code
        if(Input.GetKey("left") && isGrounded && !check_is_dead)
        {
            transform.Rotate(-Vector3.up * 30.0f * Time.deltaTime);
        }

        int time_of_death = System.DateTime.Now.Second;

        // //Fix velocity
        if(walk_forward)
            velocity_claire = forwardvelocity;
        if(walk_backward)
            velocity_claire = backvelocity;
        if(is_crouching_up)
            velocity_claire = crouchforwardvelocity;
        if(is_crouching_down)
            velocity_claire = crouchbackvelocity;
        if(run_forward)
            velocity_claire = runvelocity;
        if(jump)
            velocity_claire = jumpvelocity;

        //dead state
        if(num_lives <= 0)
        {
            animation_controller.SetBool("death", true);
            playAgain.gameObject.SetActive(true);
            WOL.gameObject.SetActive(true);
            WOL.text = "You are Dead!!";
            StartCoroutine(freeze());
            return;
        }

        // character controller's move function is useful to prevent the character passing through the terrain
        // (changing transform's position does not make these checks)
        if (transform.position.y > 0.0f) // if the character starts "climbing" the terrain, drop her down
        {
            Vector3 lower_character = movement_direction * velocity * Time.deltaTime;
            lower_character.y = -100f; // hack to force her down
            character_controller.Move(lower_character);
        }
        else
        {
            character_controller.Move(movement_direction * velocity * Time.deltaTime);
        }
    }
}
