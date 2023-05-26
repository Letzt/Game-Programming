using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

enum TileType
{
    WALL = 0,
    FLOOR = 1,
    WATER = 2,
    DRUG = 3,
    VIRUS = 4,
}

public class Level : MonoBehaviour
{
    // fields/variables you may adjust from Unity's interface
    public int width = 16;   // size of level (default 16 x 16 blocks)
    public int length = 16;
    public float storey_height = 2.5f;   // height of walls
    public float virus_speed = 3.0f;     // virus velocity
    public GameObject fps_prefab;        // these should be set to prefabs as provided in the starter scene
    public GameObject virus_prefab;
    public GameObject water_prefab;
    public GameObject house_prefab;
    public GameObject text_box;
    public GameObject scroll_bar;

    // fields/variables accessible from other scripts
    internal GameObject fps_player_obj;   // instance of FPS template
    internal float player_health = 1.0f;  // player health in range [0.0, 1.0]
    internal int num_virus_hit_concurrently = 0;            // how many viruses hit the player before washing them off
    internal bool virus_landed_on_player_recently = false;  // has virus hit the player? if yes, a timer of 5sec starts before infection
    internal float timestamp_virus_landed = float.MaxValue; // timestamp to check how many sec passed since the virus landed on player
    internal bool drug_landed_on_player_recently = false;   // has drug collided with player?
    internal bool player_is_on_water = false;               // is player on water block
    internal bool player_entered_house = false;             // has player arrived in house?

    // fields/variables needed only from this script
    private Bounds bounds;                   // size of ground plane in world space coordinates
    private float timestamp_last_msg = 0.0f; // timestamp used to record when last message on GUI happened (after 7 sec, default msg appears)
    private int function_calls = 0;          // number of function calls during backtracking for solving the CSP
    private int num_viruses = 0;             // number of viruses in the level
    private List<int[]> pos_viruses;         // stores their location in the grid

    // feel free to put more fields here, if you need them e.g, add AudioClips that you can also reference them from other scripts
    // for sound, make also sure that you have ONE audio listener active (either the listener in the FPS or the main camera, switch accordingly)

    public GameObject tryAgain;

    public AudioSource drug;
    public AudioSource virus;
    public bool virus_flag;
    public AudioSource health;
    public AudioSource water;
    public bool water_flag;
    public AudioSource end;
    public bool end_flag;

    // a helper function that randomly shuffles the elements of a list (useful to randomize the solution to the CSP)
    private void Shuffle<T>(ref List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        // initialize internal/private variables
        bounds = GetComponent<Collider>().bounds;
        timestamp_last_msg = 0.0f;
        function_calls = 0;
        num_viruses = 0;
        player_health = 1.0f;
        num_virus_hit_concurrently = 0;
        virus_landed_on_player_recently = false;
        timestamp_virus_landed = float.MaxValue;
        drug_landed_on_player_recently = false;
        player_is_on_water = false;
        player_entered_house = false;

        //button for failure
        tryAgain = GameObject.Find("TryAgain");
        tryAgain.gameObject.SetActive(false);

        //Audio
        water_flag = true;
        end_flag = true;
        virus_flag = true;

        // initialize 2D grid
        List<TileType>[,] grid = new List<TileType>[width, length];
        // useful to keep variables that are unassigned so far
        List<int[]> unassigned = new List<int[]>();

        // will place x viruses in the beginning (at least 1). x depends on the sise of the grid (the bigger, the more viruses)
        num_viruses = width * length / 25 + 1; // at least one virus will be added
        pos_viruses = new List<int[]>();
        // create the wall perimeter of the level, and let the interior as unassigned
        // then try to assign variables to satisfy all constraints
        // *rarely* it might be impossible to satisfy all constraints due to initialization
        // in this case of no success, we'll restart the random initialization and try to re-solve the CSP
        bool success = false;
        while (!success)
        {
            for (int v = 0; v < num_viruses; v++)
            {
                while (true) // try until virus placement is successful (unlikely that there will no places)
                {
                    // try a random location in the grid
                    int wr = Random.Range(1, width - 1);
                    int lr = Random.Range(1, length - 1);

                    // if grid location is empty/free, place it there
                    if (grid[wr, lr] == null)
                    {
                        grid[wr, lr] = new List<TileType> { TileType.VIRUS };
                        pos_viruses.Add(new int[2] { wr, lr });
                        break;
                    }
                }
            }

            for (int w = 0; w < width; w++)
                for (int l = 0; l < length; l++)
                    if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                        grid[w, l] = new List<TileType> { TileType.WALL };
                    else
                    {
                        if (grid[w, l] == null) // does not have virus already or some other assignment from previous run
                        {
                            // CSP will involve assigning variables to one of the following four values (VIRUS is predefined for some tiles)
                            List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.WATER, TileType.DRUG };
                            Shuffle<TileType>(ref candidate_assignments);

                            grid[w, l] = candidate_assignments;
                            unassigned.Add(new int[] { w, l });
                        }
                    }

            // YOU MUST IMPLEMENT this function!!!
            success = BackTrackingSearch(grid, unassigned);
            if (!success)
            {
                Debug.Log("Could not find valid solution - will try again");
                unassigned.Clear();
                grid = new List<TileType>[width, length];
                function_calls = 0;
            }
        }

        DrawDungeon(grid);
    }

    // one type of constraint already implemented for you
    bool DoWeHaveTooManyInteriorWallsORWaterORDrug(List<TileType>[,] grid)
    {
        int[] number_of_assigned_elements = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                if (grid[w, l].Count == 1)
                    number_of_assigned_elements[(int)grid[w, l][0]]++;
            }

        if ((number_of_assigned_elements[(int)TileType.WALL] > num_viruses * 10) ||
             (number_of_assigned_elements[(int)TileType.WATER] > (width + length) / 4) ||
             (number_of_assigned_elements[(int)TileType.DRUG] >= num_viruses / 2))
            return true;
        else
            return false;
    }

    // another type of constraint already implemented for you
    bool DoWeHaveTooFewWallsORWaterORDrug(List<TileType>[,] grid)
    {
        int[] number_of_potential_assignments = new int[] { 0, 0, 0, 0, 0 };
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
            {
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                for (int i = 0; i < grid[w, l].Count; i++)
                    number_of_potential_assignments[(int)grid[w, l][i]]++;
            }

        if ((number_of_potential_assignments[(int)TileType.WALL] < (width * length) / 4) ||
             (number_of_potential_assignments[(int)TileType.WATER] < num_viruses / 4) ||
             (number_of_potential_assignments[(int)TileType.DRUG] < num_viruses / 4))
            return true;
        else
            return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there are three (or more) interior consecutive wall blocks either horizontally or vertically
    // by interior, we mean walls that do not belong to the perimeter of the grid
    // e.g., a grid configuration: "FLOOR - WALL - WALL - WALL - FLOOR" is not valid
    bool TooLongWall(List<TileType>[,] grid)
    {
        // return false;
        /*** implement the rest ! */
        int w, l;

        //pick each cell and compare the 2 adjacent ones
        //row-wise
        for (w = 1; w < width-1; w++)
        {
            for (l = 1; l < length-3; l++)
            {
                if(grid[w,l][0] == TileType.WALL && grid[w,l+1][0] == TileType.WALL && grid[w,l+2][0] == TileType.WALL &&
                  grid[w,l].Count == 1 && grid[w,l+1].Count == 1 && grid[w,l+2].Count == 1)
                    return true;
            }
        }

        //column-wise
        for (l = 1; l < length-1; l++)
        {
            for (w = 1; w < width-3; w++)
            {
                if(grid[w,l][0] == TileType.WALL && grid[w+1,l][0] == TileType.WALL && grid[w+2,l][0] == TileType.WALL &&
                  grid[w,l].Count == 1 && grid[w+1,l].Count == 1 && grid[w+2,l].Count == 1)
                    return true;
            }
        }
        return false;
    }

    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // must return true if there is no WALL adjacent to a virus
    // adjacency means left, right, top, bottom, and *diagonal* blocks
    bool NoWallsCloseToVirus(List<TileType>[,] grid)
    {
        /*** implement the rest ! */
        for (int w = 0; w < width; w++)
            for (int l = 0; l < length; l++)
                if (w == 0 || l == 0 || w == width - 1 || l == length - 1)
                    continue;
                else
                {
                    if (grid[w, l][0] == TileType.VIRUS)
                    {
                        if(grid[w-1, l][0] == TileType.WALL || grid[w+1, l][0] == TileType.WALL || grid[w, l-1][0] == TileType.WALL ||
                         grid[w, l+1][0] == TileType.WALL || grid[w-1, l-1][0] == TileType.WALL || grid[w-1, l+1][0] == TileType.WALL ||
                         grid[w+1, l-1][0] == TileType.WALL || grid[w+1, l+1][0] == TileType.WALL)
                            return false;
                    }
                }
        return true;
    }


    // check if attempted assignment is consistent with the constraints or not
    bool CheckConsistency(List<TileType>[,] grid, int[] cell_pos, TileType t)
    {
        int w = cell_pos[0];
        int l = cell_pos[1];

        List<TileType> old_assignment = new List<TileType>();
        old_assignment.AddRange(grid[w, l]);
        grid[w, l] = new List<TileType> { t };

		// note that we negate the functions here i.e., check if we are consistent with the constraints we want
        bool areWeConsistent = !DoWeHaveTooFewWallsORWaterORDrug(grid) && !DoWeHaveTooManyInteriorWallsORWaterORDrug(grid)
                            && !TooLongWall(grid) && !NoWallsCloseToVirus(grid);

        grid[w, l] = new List<TileType>();
        grid[w, l].AddRange(old_assignment);
        return areWeConsistent;
    }


    // *** YOU NEED TO COMPLETE THIS FUNCTION  ***
    // implement backtracking
    bool BackTrackingSearch(List<TileType>[,] grid, List<int[]> unassigned)
    {
        // if there are too many recursive function evaluations, then backtracking has become too slow (or constraints cannot be satisfied)
        // to provide a reasonable amount of time to start the level, we put a limit on the total number of recursive calls
        // if the number of calls exceed the limit, then it's better to try a different initialization
        if (function_calls++ > 100000)
            return false;

        // we are done!
        if (unassigned.Count == 0)
            return true;

        /*** implement the rest ! */

        //randomly selecting an unassigned spot
        int index = Random.Range(0, unassigned.Count);
        int w = unassigned[index][0];
        int l = unassigned[index][1];

        // List<TileType> candidate_assignments = grid[w, l];
        List<TileType> candidate_assignments = new List<TileType> { TileType.WALL, TileType.FLOOR, TileType.WATER, TileType.DRUG };
        foreach (var tiletype in candidate_assignments) {
            if (CheckConsistency(grid, unassigned[index], tiletype))
            {
                grid[w, l] = new List<TileType> { tiletype };
                unassigned.RemoveAt(index);
                if (BackTrackingSearch(grid, unassigned))
                  return true;
                unassigned.Add(new int[] { w, l });
            }
        }
        return false;
    }

    void check(int moveX, int moveY, ref bool[,] visited, ref Queue<int[]> new_track)
    {
        if (moveX >= 0 && moveX < width && moveY >=0 && moveY < length && !visited[moveX, moveY])
        {
           new_track.Enqueue(new int[] { moveX, moveY });
           visited[moveX, moveY] = true;
        }
        return;
    }

    void dfs(List<TileType>[,] solution, ref bool[,] tracker, ref bool path, int iniX, int iniY, int finalX, int finalY)
    {
        if (iniX < 0 || iniX >= width || iniY < 0 || iniY >= length || path || tracker[iniX, iniY])
            return;
        if (iniX == finalX && iniX == finalY)
        {
            path = true;
            return;
        }
        if (solution[iniX, iniY][0] == TileType.WALL)
            return;

        tracker[iniX, iniY] = true;
        dfs(solution, ref tracker, ref path, iniX+1, iniY, finalX, finalY);
        dfs(solution, ref tracker, ref path, iniX, iniY+1, finalX, finalY);
        dfs(solution, ref tracker, ref path, iniX-1, iniY, finalX, finalY);
        dfs(solution, ref tracker, ref path, iniX, iniY-1, finalX, finalY);
    }

    // places the primitives/objects according to the grid assignents
    // you will need to edit this function (see below)
    void DrawDungeon(List<TileType>[,] solution)
    {
        GetComponent<Renderer>().material.color = Color.grey; // ground plane will be grey

        // place character at random position (wr, lr) in terms of grid coordinates (integers)
        // make sure that this random position is a FLOOR tile (not wall, drug, or virus)
        int wr = 0;
        int lr = 0;
        while (true) // try until a valid position is sampled
        {
            wr = Random.Range(1, width - 1);
            lr = Random.Range(1, length - 1);

            if (solution[wr, lr][0] == TileType.FLOOR)
            {
                float x = bounds.min[0] + (float)wr * (bounds.size[0] / (float)width);
                float z = bounds.min[2] + (float)lr * (bounds.size[2] / (float)length);
                fps_player_obj = Instantiate(fps_prefab);
                fps_player_obj.name = "PLAYER";
                // character is placed above the level so that in the beginning, he appears to fall down onto the maze
                fps_player_obj.transform.position = new Vector3(x + 0.5f, 2.0f * storey_height, z + 0.5f);
                break;
            }
        }

        // place an exit from the maze at location (wee, lee) in terms of grid coordinates (integers)
        // destroy the wall segment there - the grid will be used to place a house
        // the exist will be placed as far as away from the character (yet, with some randomness, so that it's not always located at the corners)
        int max_dist = -1;
        int wee = -1;
        int lee = -1;
        while (true) // try until a valid position is sampled
        {
            if (wee != -1)
                break;
            for (int we = 0; we < width; we++)
            {
                for (int le = 0; le < length; le++)
                {
                    // skip corners
                    if (we == 0 && le == 0)
                        continue;
                    if (we == 0 && le == length - 1)
                        continue;
                    if (we == width - 1 && le == 0)
                        continue;
                    if (we == width - 1 && le == length - 1)
                        continue;

                    if (we == 0 || le == 0 || wee == length - 1 || lee == length - 1)
                    {
                        // randomize selection
                        if (Random.Range(0.0f, 1.0f) < 0.1f)
                        {
                            int dist = System.Math.Abs(wr - we) + System.Math.Abs(lr - le);
                            if (dist > max_dist) // must be placed far away from the player
                            {
                                wee = we;
                                lee = le;
                                max_dist = dist;
                            }
                        }
                    }
                }
            }
        }


        // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION  ***
        // implement an algorithm that checks whether
        // all paths between the player at (wr,lr) and the exit (wee, lee)
        // are blocked by walls. i.e., there's no way to get to the exit!
        // if this is the case, you must guarantee that there is at least
        // one accessible path (any path) from the initial player position to the exit
        // by removing a few wall blocks (removing all of them is not acceptable!)
        // this is done as a post-processing step after the CSP solution.
        // It might be case that some constraints might be violated by this
        // post-processing step - this is OK.

        /*** implement what is described above ! */
        int iniX = wr;
        int iniY = lr;
        int finalX = wee;
        int finalY = lee;

        int[,] vals = new int[width, length];
        int m, n, wid, len, i, j;

        for(m = 0; m < width; m++)
        {
            for(n = 0; n < length; n++)
            {
                if(solution[m,n][0] == TileType.WALL)
                    vals[m,n] = 1;
                else
                    vals[m,n] = 0;
            }
        }

        vals[iniX, iniY] = 2;
        vals[finalX, finalY] = 3;

        bool path = false;
        bool[,] tracker = new bool[width, length];
        dfs(solution, ref tracker, ref path, iniX, iniY, finalX, finalY);

        if(!path)
        {
          bool[,] visited = new bool[width, length];
          Queue<int[]> q_track = new Queue<int[]>();
          List<int>[,] head = new List<int>[width, length];

          //Single check
          visited[iniX, iniY] = true;
          q_track.Enqueue(new int[] { iniX, iniY });
          head[iniX, iniY] = new List<int> {-1, -1};

          while(q_track.Count > 0)
          {
              int[] present = q_track.Dequeue();
              if (present[0] == finalX && present[1] == finalY)
                break;

              List<int[]> next = new List<int[]>{};
              Queue<int[]> new_track = new Queue<int[]>();
              visited[present[0], present[1]] = true;

              int moveX = present[0] + 1;
              int moveY = present[1];
              check(moveX, moveY, ref visited, ref new_track);

              moveX = present[0];
              moveY = present[1] + 1;
              check(moveX, moveY, ref visited, ref new_track);

              moveX = present[0] - 1;
              moveY = present[1];
              check(moveX, moveY, ref visited, ref new_track);

              moveX = present[0];
              moveY = present[1] - 1;
              check(moveX, moveY, ref visited, ref new_track);

              while(new_track.Count != 0)
              {
                int[] spot = new_track.Dequeue();

                if (vals[spot[0],spot[1]] == 1 || vals[spot[0],spot[1]] == 3 || vals[spot[0],spot[1]] == 2)
                {
                  next.Add(spot);
                  continue;
                }

                moveX = spot[0] + 1;
                moveY = spot[1];
                check(moveX, moveY, ref visited, ref new_track);

                moveX = spot[0];
                moveY = spot[1] + 1;
                check(moveX, moveY, ref visited, ref new_track);

                moveX = spot[0] - 1;
                moveY = spot[1];
                check(moveX, moveY, ref visited, ref new_track);

                moveX = spot[0];
                moveY = spot[1] - 1;
                check(moveX, moveY, ref visited, ref new_track);
            }

            for(i = 0; i < next.Count; i++)
            {
              wid = next[i][0];
              len = next[i][1];

              if ((wid == 0 || len == 0 || wid == width - 1 || len == length - 1) && (!(next[i][0] == finalX) || !(next[i][1] == finalY)))
                continue;

              head[next[i][0], next[i][1]] = new List<int> {present[0], present[1]};
              q_track.Enqueue(next[i]);
            }
          }

          List<int[]> walls = new List<int[]>();
          List<int> node = head[finalX, finalY];
          int nodeX = node[0];
          int nodeY = node[1];

          while(!(nodeX == iniX) || !(nodeY == iniY))
          {
            node = head[nodeX, nodeY];
            walls.Add(new int[]{nodeX, nodeY});
            nodeX = node[0];
            nodeY = node[1];
          }

          for(i = 0; i < walls.Count; i++)
          {
            m = walls[i][0];
            n = walls[i][1];
            vals[m, n] = 0;
            solution[m, n][0] = TileType.FLOOR;
          }
        }

        for(i = 0; i < width; i++)
        {
          for(j = 0; j < length; j++)
          {
            if (solution[i, j][0] == TileType.WALL)
              vals[i, j] = 1;
            else
              vals[i, j] = 0;
          }
        }

        vals[iniX, iniY] = 2;
        vals[finalX, finalY] = 3;

        // the rest of the code creates the scenery based on the grid state
        // you don't need to modify this code (unless you want to replace the virus
        // or other prefabs with something else you like)
        int w = 0;
        for (float x = bounds.min[0]; x < bounds.max[0]; x += bounds.size[0] / (float)width - 1e-6f, w++)
        {
            int l = 0;
            for (float z = bounds.min[2]; z < bounds.max[2]; z += bounds.size[2] / (float)length - 1e-6f, l++)
            {
                if ((w >= width) || (l >= width))
                    continue;

                float y = bounds.min[1];
                //Debug.Log(w + " " + l + " " + h);
                if ((w == wee) && (l == lee)) // this is the exit
                {
                    GameObject house = Instantiate(house_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    house.name = "HOUSE";
                    house.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    if (l == 0)
                        house.transform.Rotate(0.0f, 270.0f, 0.0f);
                    else if (w == 0)
                        house.transform.Rotate(0.0f, 0.0f, 0.0f);
                    else if (l == length - 1)
                        house.transform.Rotate(0.0f, 90.0f, 0.0f);
                    else if (w == width - 1)
                        house.transform.Rotate(0.0f, 180.0f, 0.0f);

                    house.AddComponent<BoxCollider>();
                    house.GetComponent<BoxCollider>().isTrigger = true;
                    house.GetComponent<BoxCollider>().size = new Vector3(3.0f, 3.0f, 3.0f);
                    house.AddComponent<House>();
                }
                else if (solution[w, l][0] == TileType.WALL)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WALL";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y + storey_height / 2.0f, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = new Color(0.6f, 0.8f, 0.8f);
                }
                else if (solution[w, l][0] == TileType.VIRUS)
                {
                    GameObject virus = Instantiate(virus_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    virus.name = "COVID";
                    virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);

                    //GameObject virus = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //virus.GetComponent<Renderer>().material.color = new Color(0.5f, 0.0f, 0.0f);
                    //virus.name = "ENEMY";
                    //virus.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    //virus.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    //virus.AddComponent<BoxCollider>();
                    //virus.GetComponent<BoxCollider>().size = new Vector3(1.2f, 1.2f, 1.2f);
                    //virus.AddComponent<Rigidbody>();
                    //virus.GetComponent<Rigidbody>().useGravity = false;

                    virus.AddComponent<Virus>();
                    virus.GetComponent<Rigidbody>().mass = 10000;
                }
                else if (solution[w, l][0] == TileType.DRUG)
                {
                    GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                    capsule.name = "DRUG";
                    capsule.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                    capsule.transform.position = new Vector3(x + 0.5f, y + Random.Range(1.0f, storey_height / 2.0f), z + 0.5f);
                    capsule.GetComponent<Renderer>().material.color = Color.green;
                    capsule.AddComponent<Drug>();
                }
                else if (solution[w, l][0] == TileType.WATER)
                {
                    GameObject water = Instantiate(water_prefab, new Vector3(0, 0, 0), Quaternion.identity);
                    water.name = "WATER";
                    water.transform.localScale = new Vector3(0.5f * bounds.size[0] / (float)width, 1.0f, 0.5f * bounds.size[2] / (float)length);
                    water.transform.position = new Vector3(x + 0.5f, y + 0.1f, z + 0.5f);

                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "WATER_BOX";
                    cube.transform.localScale = new Vector3(bounds.size[0] / (float)width, storey_height / 20.0f, bounds.size[2] / (float)length);
                    cube.transform.position = new Vector3(x + 0.5f, y, z + 0.5f);
                    cube.GetComponent<Renderer>().material.color = Color.grey;
                    cube.GetComponent<BoxCollider>().size = new Vector3(1.1f, 20.0f * storey_height, 1.1f);
                    cube.GetComponent<BoxCollider>().isTrigger = true;
                    cube.AddComponent<Water>();
                }
            }
        }
    }

    IEnumerator destroyscene()
    {
        yield return new WaitForSeconds(2f);
        Object.Destroy(fps_player_obj);
    }

    // *** YOU NEED TO COMPLETE THIS PART OF THE FUNCTION JUST TO ADD SOUNDS ***
    // YOU MAY CHOOSE ANY SHORT SOUNDS (<2 sec) YOU WANT FOR A VIRUS HIT, A VIRUS INFECTION,
    // GETTING INTO THE WATER, AND REACHING THE EXIT
    // note: you may also change other scripts/functions to add sound functionality,
    // along with the functionality for the starting the level, or repeating it
    void Update()
    {
        if (player_health < 0.001f) // the player dies here
        {
            text_box.GetComponent<Text>().text = "Failed!";

            if (fps_player_obj != null)
            {
                GameObject grave = GameObject.CreatePrimitive(PrimitiveType.Cube);
                grave.name = "GRAVE";
                grave.transform.localScale = new Vector3(bounds.size[0] / (float)width, 2.0f * storey_height, bounds.size[2] / (float)length);
                grave.transform.position = fps_player_obj.transform.position;
                grave.GetComponent<Renderer>().material.color = Color.black;

                tryAgain.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                Object.Destroy(fps_player_obj);
            }
            return;
        }
        if (player_entered_house) // the player suceeds here, variable manipulated by House.cs
        {
            if (end_flag)
            {
                end.Play();
                end_flag = false;
            }

            if (virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Washed it off at home! Success!!!";
            else
                text_box.GetComponent<Text>().text = "Success!!!";

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            StartCoroutine(destroyscene());
            return;
        }


        if (Time.time - timestamp_last_msg > 7.0f) // renew the msg by restating the initial goal
        {
            text_box.GetComponent<Text>().text = "Find your home!";
        }

        // virus hits the players (boolean variable is manipulated by Virus.cs)
        if (virus_landed_on_player_recently)
        {
            // virus.Play();
            if (virus_flag)
            {
                virus.Play();
                virus_flag = false;
            }

            float time_since_virus_landed = Time.time - timestamp_virus_landed;

            // if (time_since_virus_landed == 5.0f)
            // {
            //     Debug.Log("power down");
            //     health.Play();
            // }

            if (time_since_virus_landed > 5.0f)
            {
                // if (health_flag)
                // {
                //   health.Play();
                //   health_flag = false;
                // }
                player_health -= Random.Range(0.25f, 0.5f) * (float)num_virus_hit_concurrently;
                player_health = Mathf.Max(player_health, 0.0f);
                if (num_virus_hit_concurrently > 1)
                {
                    health.Play();
                    text_box.GetComponent<Text>().text = "Ouch! Infected by " + num_virus_hit_concurrently + " viruses";
                }
                else
                {
                    health.Play();
                    text_box.GetComponent<Text>().text = "Ouch! Infected by a virus";
                }

                timestamp_last_msg = Time.time;
                timestamp_virus_landed = float.MaxValue;
                virus_landed_on_player_recently = false;
                num_virus_hit_concurrently = 0;
            }
            else
            {
                if (num_virus_hit_concurrently == 1)
                {
                    text_box.GetComponent<Text>().text = "A virus landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
                }
                else
                {
                    text_box.GetComponent<Text>().text = num_virus_hit_concurrently + " viruses landed on you. Infection in " + (5.0f - time_since_virus_landed).ToString("0.0") + " seconds. Find water or drug!";
                }
            }
        }
        else
        {
            virus_flag = true;
        }

        // drug picked by the player  (boolean variable is manipulated by Drug.cs)
        if (drug_landed_on_player_recently)
        {
            drug.Play();
            if (player_health < 0.999f || virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Phew! New drug helped!";
            else
                text_box.GetComponent<Text>().text = "No drug was needed!";
            timestamp_last_msg = Time.time;
            player_health += Random.Range(0.25f, 0.75f);
            player_health = Mathf.Min(player_health, 1.0f);
            drug_landed_on_player_recently = false;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }

        // splashed on water  (boolean variable is manipulated by Water.cs)
        if (player_is_on_water)
        {
            if(water_flag)
            {
                water.Play();
                water_flag = false;
            }

            if (virus_landed_on_player_recently)
                text_box.GetComponent<Text>().text = "Phew! Washed it off!";
            timestamp_last_msg = Time.time;
            timestamp_virus_landed = float.MaxValue;
            virus_landed_on_player_recently = false;
            num_virus_hit_concurrently = 0;
        }
        else
          water_flag = true;

        // update scroll bar (not a very conventional manner to create a health bar, but whatever)
        scroll_bar.GetComponent<Scrollbar>().size = player_health;
        if (player_health < 0.5f)
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(1.0f, 0.0f, 0.0f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }
        else
        {
            ColorBlock cb = scroll_bar.GetComponent<Scrollbar>().colors;
            cb.disabledColor = new Color(0.0f, 1.0f, 0.25f);
            scroll_bar.GetComponent<Scrollbar>().colors = cb;
        }

        /*** implement the rest ! */
    }
}
