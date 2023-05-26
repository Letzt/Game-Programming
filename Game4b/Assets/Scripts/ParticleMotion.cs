using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

// change this class as you may see fit
public class ParticleMotion : MonoBehaviour
{
    float[] particle_state;
    Vector3 lcenter, rcenter;

    // Particle (ball instance initialization)
    void Start()
    {
        particle_state = new float[6]; // you may keep the velocity/position in the same array (it can also be separated into two Vector3's)
        lcenter = new Vector3(-5.0f, 1.0f, 0.0f);  // left center of gravity
        rcenter = new Vector3(5.0f, 1.0f, 0.0f);  // right center of gravity

        // initialize particle state (position from the initial instance's position, and velocity according to the cannon orientation)
        for (int d = 0; d < 3; d++)
            particle_state[d] = transform.position[d];

        float angle_radians = transform.rotation.eulerAngles.x * (Mathf.PI / 180.0f);
        particle_state[3] = 2.0f * Mathf.Cos(-angle_radians);
        particle_state[4] = 2.0f * Mathf.Sin(-angle_radians);
        particle_state[5] = 0.0f;
    }

    void FixedUpdate()
    {
        // your function to implement!
        UpdateState(lcenter, rcenter, Time.deltaTime, ref particle_state);

        // update position for particle
        transform.position = new Vector3(particle_state[0], particle_state[1], particle_state[2]);
    }



    // you need to change this function for numerical integration to calculate the particle state
    // to support
    // (a) two attraction forces
    // (b) a simple linear drag function (for good balls)
    // (c) update velocity from forces, and
    // (d) update position from velocity
    void UpdateState(Vector3 lcenter, Vector3 rcenter, float dt, ref float[] particle_state)
    {
    		// delete these lines and update the particle state based on the Newton's law of motion and  forces described above
    		Vector3 now_pos = new Vector3(particle_state[0], particle_state[1], particle_state[2]);
    		Vector3 now_vel = new Vector3(particle_state[3], particle_state[4], particle_state[5]);

        Vector3 b1_pos = lcenter - now_pos;
        Vector3 b2_pos = rcenter - now_pos;

    		Vector3 force1 = b1_pos/Mathf.Sqrt(Mathf.Pow(b1_pos[0], 2) + Mathf.Pow(b1_pos[1], 2) + Mathf.Pow(b1_pos[2], 2));
    		Vector3 force2 = b2_pos/Mathf.Sqrt(Mathf.Pow(b2_pos[0], 2) + Mathf.Pow(b2_pos[1], 2) + Mathf.Pow(b2_pos[2], 2));

    		Vector3 new_pos = now_pos + (float)0.02 * now_vel;
    		Vector3 new_vel = now_vel + ((float)0.02 * ((force1 + force2 + (-1 * (float)0.012 * now_vel))/(float)0.18));

    		particle_state[0] = new_pos[0];
    		particle_state[1] = new_pos[1];
    		particle_state[2] = new_pos[2];
    		particle_state[3] = new_vel[0];
    		particle_state[4] = new_vel[1];
    		particle_state[5] = new_vel[1];
    }
}
