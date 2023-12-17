using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager0 : MonoBehaviour
{
    public int size = 10;
    private bool startAssigned = false;
    public Cube cubePrefab; // Assign a prefab with the Cube script
    private Cube[,,] grid; // 3D grid of cubes

    // basical block
    [SerializeField] GameObject prefabRed;
    [SerializeField] GameObject prefabGreen;
    [SerializeField] GameObject prefabBlue;
    [SerializeField] GameObject prefabBlack;
    [SerializeField] GameObject prefabEmpty;

    private List<Rule> rules = new List<Rule>
    {
        new Rule(new CubeState[] { CubeState.Red, CubeState.Black, CubeState.Black }, new CubeState[] { CubeState.Green, CubeState.Green, CubeState.Red }),
        new Rule(new CubeState[] { CubeState.Red, CubeState.Green, CubeState.Green }, new CubeState[] { CubeState.White, CubeState.White, CubeState.Red }),
        // Add more rules here
    };
    IEnumerator ApplyRulesCoroutine()
    {
        bool transformationApplied;
        do
        {
            transformationApplied = false;

            // First, try to apply the first rule
            if (ApplyRule(rules[0]))
            {
                transformationApplied = true;
                Debug.Log("First rule applied, waiting for next frame to update the grid.");
                yield return new WaitForSeconds(0.1f); // Adjust the time as needed
            }
            else
            {
                // If the first rule wasn't applied, try the second rule
                if (ApplyRule(rules[1]))
                {
                    transformationApplied = true;
                    Debug.Log("Second rule applied, waiting for next frame to update the grid.");
                    yield return new WaitForSeconds(0.1f); // Adjust the time as needed
                }
            }

            // Optionally, add an additional wait time here to see the state of the grid
            // after all rules have been applied for this iteration
            yield return new WaitForSeconds(0.5f); // Adjust the time as needed
        } while (transformationApplied);

        Debug.Log("No more rules can be applied.");
    }


    void Start()
    {
        InitializeGrid();
        StartCoroutine(ApplyRulesCoroutine());
    }

    void InitializeGrid()
    {
        grid = new Cube[size, size, size];
        // Instantiate your cubes and place them into the grid
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {

                    // Instantiate a new cube at the current position
                    // Quaternion.identity : This quaternion corresponds to "no rotation" - the object is perfectly aligned with the world or parent axes.
                    Cube newCube = Instantiate(prefabEmpty, new Vector3(x, y, z), Quaternion.identity).GetComponent<Cube>();

                    // Set the start point
                    float seed = Random.Range(-10.0f, 10.0f);
                    if (!startAssigned && seed < -9.0f)
                    {
                        // Set the cube's state to red
                        newCube.state = CubeState.Red;
                        startAssigned = true;
                    }
                    else
                    {
                        // Set the cube's state to black/empty
                        newCube.state = CubeState.Black;
                    }


                    // Optionally, update the material to reflect the state if required
                    newCube.UpdateMaterial();
                    // Store the cube in the grid array
                    grid[x, y, z] = newCube;
                }
            }
        }
    }

    bool ApplyRule(Rule rule)
    {
        bool ruleApplied = false;

        // Define all 6 possible directions in a 3D grid
        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(1, 0, 0), // right
        new Vector3Int(-1, 0, 0), // left
        new Vector3Int(0, 1, 0), // up
        new Vector3Int(0, -1, 0), // down
        new Vector3Int(0, 0, 1), // forward
        new Vector3Int(0, 0, -1), // backward
        };

        //get a randomly chosen direction order
        directions = ShuffleArray(directions);

        // Loop through the entire grid
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    foreach (var direction in directions)
                    {
                        bool matchFound = true;
                        Vector3Int currentPos = new Vector3Int(x, y, z);

                        // Check if the subsequent cubes match the rest of the pattern in the current direction
                        matchFound = CheckRule(rule, grid, currentPos, direction);

                        // If a match is found, apply the replacement pattern
                        if (matchFound)
                        {
                            // update grid state
                            UpdateGrid(rule, grid, currentPos, direction);
                            ruleApplied = true;
                            // If you want to apply only one rule at a time, uncomment the following line:
                            // return true;
                        }
                    }
                }
            }
        }

        return ruleApplied;
    }

    bool CheckRule(Rule rule, Cube[,,] grid, Vector3Int currentPos, Vector3Int direction)
    {
        bool allFound = true;
        for (int i = 0; i < rule.pattern.Length && allFound; i++)
        {
            Vector3Int nextPos = currentPos + direction * i;
            if (!IsPositionValid(nextPos) || grid[nextPos.x, nextPos.y, nextPos.z].state != rule.pattern[i])
            {
                allFound = false;
            }
        }

        return allFound;
    }

    void UpdateGrid(Rule rule, Cube[,,] grid, Vector3Int currentPos, Vector3Int direction)
    {
        for (int i = 0; i < rule.replacement.Length; i++)
        {
            Vector3Int nextPos = currentPos + direction * i;
            grid[nextPos.x, nextPos.y, nextPos.z].state = rule.replacement[i];
            grid[nextPos.x, nextPos.y, nextPos.z].UpdateMaterial();
        }
    }

    // Helper method to check if a position is within the bounds of the grid
    bool IsPositionValid(Vector3Int position)
    {
        return position.x >= 0 && position.x < size &&
               position.y >= 0 && position.y < size &&
               position.z >= 0 && position.z < size;
    }

    Vector3Int[] ShuffleArray(Vector3Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            Vector3Int temp = array[i];
            int randomIndex = Random.Range(i, array.Length);
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
        return array;
    }


}