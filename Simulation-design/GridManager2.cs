using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GridManager2 : MonoBehaviour
{
    public int size = 10;
    public int height = 50;
    private bool startAssigned = false;
    public Cube cubePrefab; // Assign a prefab with the Cube script

    private CubeState[,,] grid; // 3D grid of cubes
    private GameObject[,,] currentGrid;

    //private int[,,] grid;
    

    // basical block
    // Prefabs
    [SerializeField] GameObject prefabRed;
    [SerializeField] GameObject prefabGreen;
    [SerializeField] GameObject prefabBlue;
    [SerializeField] GameObject prefabBlack;
    [SerializeField] GameObject prefabWhite;
    [SerializeField] GameObject prefabEmpty;

    private List<Rule> rules = new List<Rule>
    {
        //new Rule(new CubeState[] { CubeState.Red, CubeState.Empty, CubeState.Empty, CubeState.Empty }, new CubeState[] { CubeState.Green, CubeState.Green, CubeState.Green, CubeState.Red }),
        //new Rule(new CubeState[] { CubeState.Red, CubeState.Green, CubeState.Green }, new CubeState[] { CubeState.White, CubeState.White, CubeState.Red }),
        new Rule(new CubeState[] { CubeState.Red, CubeState.Empty, CubeState.Empty }, new CubeState[] { CubeState.Green, CubeState.Green, CubeState.Red }),
        new Rule(new CubeState[] { CubeState.Red, CubeState.Green, CubeState.Green }, new CubeState[] { CubeState.White, CubeState.White, CubeState.Red }),
        new Rule(new CubeState[] { CubeState.White, CubeState.White, CubeState.White }, new CubeState[] { CubeState.Black, CubeState.White, CubeState.Blue}),
        new Rule(new CubeState[] { CubeState.Blue, CubeState.Black, CubeState.White }, new CubeState[] { CubeState.Black, CubeState.Black, CubeState.Red }),
        //new Rule(new CubeState[] { CubeState.Black, CubeState.White }, new CubeState[] { CubeState.Black, CubeState.Black}),
        // Add more rules here
    };
    IEnumerator ApplyRulesCoroutine()
    {
        bool transformationApplied;
        do
        {
            transformationApplied = false;

            // Weighted random rule application
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        Rule selectedRule = ChooseRuleByWeight();
                        if (ApplyRule(selectedRule))
                        {
                            transformationApplied = true;
                            // Log which rule was applied if needed
                            // Debug.Log($"Applied rule with pattern: {selectedRule.pattern}");
                            yield return new WaitForSeconds(0.1f); // Adjust the time as needed
                        }
                    }
                }
            }
            yield return new WaitForSeconds(0.5f); // Adjust the time as needed
        } while (transformationApplied);

        Debug.Log("No more rules can be applied.");
    }

    Rule ChooseRuleByWeight()
    {
        // Example weights: rule one has a higher chance of being selected
        float[] weights = { 0.7f, 0.15f, 0.15f }; // Adjust weights as needed
        float totalWeight = 0f;
        foreach (var weight in weights)
        {
            totalWeight += weight;
        }

        float randomPoint = Random.value * totalWeight;
        for (int i = 0; i < weights.Length; i++)
        {
            if (randomPoint < weights[i])
            {
                return rules[i];
            }
            else
            {
                randomPoint -= weights[i];
            }
        }
        return rules[0]; // Default return the most common rule if needed
    }


    void Start()
    {
        InitializeGrid();
        StartCoroutine(ApplyRulesCoroutine());
    }

    void InitializeGrid()
    {
        Debug.Log("Initializing Grid");
        grid = new CubeState[size, height, size];
        currentGrid = new GameObject[size, height, size];

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    CubeState state = CubeState.Empty; // Default to black
                    float seed = Random.Range(-10.0f, 10.0f);
                    if (!startAssigned && seed < -9.0f)
                    {
                        state = CubeState.Red;
                        startAssigned = true;
                    }

                    grid[x, y, z] = state;

                    Debug.Log("Instantiating Initial Grid");
                    InstantiatePrefabAtPosition(state, new Vector3Int(x, y, z));
                }
            }
        }

        Debug.Log("Grid initialized successful");
    }

    void InstantiatePrefabAtPosition(CubeState state, Vector3Int position)
    {
        if (currentGrid[position.x, position.y, position.z] != null)
        {
            Destroy(currentGrid[position.x, position.y, position.z]);
        }

        GameObject prefabToUse = GetPrefabByState(state);
        // Convert position to Vector3
        if (prefabToUse != null)
        {
            currentGrid[position.x, position.y, position.z] = Instantiate(prefabToUse, position, Quaternion.identity);
        }

    }

    GameObject GetPrefabByState(CubeState state)
    {
        switch (state)
        {
            case CubeState.Red:
                return prefabRed;
            case CubeState.Green:
                return prefabGreen;
            case CubeState.Blue:
                return prefabBlue;
            case CubeState.Black:
                return prefabBlack;
            case CubeState.White:
                return prefabWhite;
            // Add other cases here
            default:
                return null;
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
            for (int y = 0; y < height; y++)
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

    bool CheckRule(Rule rule, CubeState[,,] grid, Vector3Int currentPos, Vector3Int direction)
    {
        bool allFound = true;
        for (int i = 0; i < rule.pattern.Length && allFound; i++)
        {
            Vector3Int nextPos = currentPos + direction * i;
            if (!IsPositionValid(nextPos) || grid[nextPos.x, nextPos.y, nextPos.z] != rule.pattern[i])
            {
                allFound = false;
            }
        }

        return allFound;
    }

    void UpdateGrid(Rule rule, CubeState[,,] grid, Vector3Int currentPos, Vector3Int direction)
    {
        for (int i = 0; i < rule.replacement.Length; i++)
        {
            Vector3Int nextPos = currentPos + direction * i;
            grid[nextPos.x, nextPos.y, nextPos.z] = rule.replacement[i];
            InstantiatePrefabAtPosition(grid[nextPos.x, nextPos.y, nextPos.z], nextPos);
        }
    }

    // Helper method to check if a position is within the bounds of the grid
    bool IsPositionValid(Vector3Int position)
    {
        return position.x >= 0 && position.x < size &&
               position.y >= 0 && position.y < height &&
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