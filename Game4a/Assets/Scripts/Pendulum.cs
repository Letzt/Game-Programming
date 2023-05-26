using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Pendulum : MonoBehaviour {
    // parameters of ths simulation (are specified through the Unity's interface)
    public float gravity_acceleration = 9.81f;
    public float mass = 1.0f;
    public float friction_coeficient = 0.0f;
    public float initial_angular_velocity = 0.0f;
    public float time_step_h = 0.1f;
    public string ode_method = "euler";

    // parameters that will be populated automatically from the geometry/components of the scene
    private float rod_length = 0.0f;
    private float c = 0.0f;
    private float omega_sqr = 0.0f;
    private GameObject pendulum = null;

    // the state vector stores two entries:
    // state_vector[0] is the angle of pendulum (\theta) in radians
    // state_vector[1] is the angular velocity of pendulum
    private Vector2 state_vector;

    public List<float> kinetic_energy_list;
    public List<float> potential_energy_list;
    public List<float> total_energy_list;
    public List<float> time_list;
    public bool output_flag;

    // Use this for initialization
    void Start ()
    {
        Time.fixedDeltaTime = time_step_h;      // set the simulation step - FixedUpdate is called every 'time_step_h' seconds
        state_vector = new Vector2(0.0f, 0.0f); // initialization of the state vector
        pendulum = GameObject.Find("Pendulum");
        if (pendulum == null)
        {
            Debug.LogError("Sphere not found! Did you delete it from the starter scene?");
        }
        GameObject rod = GameObject.Find("Quad");
        if (rod == null)
        {
            Debug.LogError("Rod not found! Did you delete it from the starter scene?");
        }
        rod_length = rod.transform.localScale.y; // finds rod length (based on quad's scale)

        state_vector[0] = pendulum.transform.eulerAngles.z * Mathf.Deg2Rad; // initial angle is set from the starter scene
        state_vector[1] = initial_angular_velocity;

        c = friction_coeficient / mass;        // following the ODE specification
        omega_sqr = gravity_acceleration / rod_length;
        output_flag = true;
    }

    // Update is called once per Time.fixedDeltaTime  sec
    void FixedUpdate ()
    {
        // complete this function (measure kinetic, potential, total energy)
        float kinetic_energy = (float)0.5 * mass * Mathf.Pow(rod_length, 2) * Mathf.Pow(state_vector[1], 2); // change here
        float potential_energy = mass * gravity_acceleration * rod_length * (1 - Mathf.Cos(state_vector[0])); // change here
        // total_energy = kinetic_energy + potential_energy

    		if(Time.time < 1.0f)
        {
            total_energy_list.Add(kinetic_energy + potential_energy);
        }

        if(Time.time >= 10.0f)
        {
            Debug.Log("10 seconds");
            return;
        }

    		if(output_flag && Time.time > 20.0f)
        {
            float total_energy = kinetic_energy + potential_energy;
    		    total_energy_list.Add(total_energy);
            Debug.Log(total_energy_list.Last() - total_energy_list[0]);

            string te = "TE : [";
            string ke = "KE : [";
            string pe = "PE : [";
            string sec = "Time : [";
            // for(int i=0;i<total_energy_list.Count;i++)
            // {
            //   te += total_energy_list[i].ToString();
            //   te += ", ";
            // }

      			for(int i=0;i<time_list.Count;i++)
      			{
      				ke += kinetic_energy_list[i].ToString();
      				ke += ", ";
      				pe += potential_energy_list[i].ToString();
      				pe += ", ";
      				te += total_energy_list[i].ToString();
      				te += ", ";
      				sec += time_list[i].ToString();
      				sec += ", ";
      			}
      			Debug.Log(ke);
      			Debug.Log(pe);
      			Debug.Log(te);
      			Debug.Log(sec);

            output_flag = false; //output done
    		}

    		kinetic_energy_list.Add(kinetic_energy);
    		potential_energy_list.Add(potential_energy);
    		total_energy_list.Add(kinetic_energy + potential_energy);
    		time_list.Add(Time.time);

        OdeStep();
        pendulum.transform.eulerAngles = new Vector3(0.0f, 0.0f, state_vector[0] * Mathf.Rad2Deg);
    }

    // complete this function!
    // it should return the right side of the two ODEs (in other words, the derivative of the pendulum's angle and angular velocity)
    // and you should use it in OdeStep!
    Vector2 PendulumDynamics(Vector2 current_state_vector)
    {
        float deri = (-c * current_state_vector[1]) - (omega_sqr * Mathf.Sin(current_state_vector[0])) ;
        return new Vector2(current_state_vector[1], deri); // change here
    }

    void OdeStep()
    {
        // delete the next line, and complete this function
        // update the state_vector (both entries) properly depending on the specified ode_method

        if (ode_method == "euler")
        {
            Vector2 pend_dyn = PendulumDynamics(state_vector);

            state_vector = new Vector2((state_vector[0] + (time_step_h * pend_dyn[0])), (state_vector[1] + (time_step_h * pend_dyn[1])));
        }
        else if (ode_method == "trapezoidal")
        {
            Vector2 start_vec = state_vector;

            Vector2 pend_dyn_1 = PendulumDynamics(start_vec);
            Vector2 second_vec = new Vector2((state_vector[0] + (time_step_h * pend_dyn_1[0])), (state_vector[1] + (time_step_h * pend_dyn_1[1])));

            Vector2 pend_dyn_2 = PendulumDynamics(second_vec);

            state_vector = new Vector2((start_vec[0] + ((float)0.5 * time_step_h * (pend_dyn_1[0] + pend_dyn_2[0]))), (start_vec[1] + (float)0.5 * time_step_h * (pend_dyn_1[1] + pend_dyn_2[1])));
        }
        else if (ode_method == "rk")
        {
            Vector2 k1 = time_step_h * PendulumDynamics(state_vector);
            Vector2 k2 = 2 * time_step_h * PendulumDynamics(state_vector + ((float)0.5 * k1));
            Vector2 k3 = 2 * time_step_h * PendulumDynamics(state_vector + ((float)0.5 * k2));
            Vector2 k4 = time_step_h * PendulumDynamics(state_vector + k3);

            state_vector += ((k1 + k2 + k3 + k4)/6);
        }
        else if (ode_method == "semi-implicit")
        {
            Vector2 v1 = state_vector + (time_step_h * PendulumDynamics(state_vector));
            Vector2 v2 = state_vector + (time_step_h * PendulumDynamics(new Vector2(state_vector[0], v1[1])));

            state_vector = new Vector2(v2[0], v1[1]);
        }
        else
        {
            Debug.LogError("ODE method should be one of the: euler, trapezoidal, rk, semi-implicit");
        }
    }
}
