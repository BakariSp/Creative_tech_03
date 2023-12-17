using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Assets.Scripts
{

    public class GridManager : MonoBehaviour
    {
        public int size = 10;
        public int height = 20;
        private int lockSeed;
        private bool startAssigned = false;
        private bool isGridProcessingComplete = false;

        private float columnGrowthProbability = 0.8f;
        private float verticalGrowthProbability = 0.2f;



        // Grid to store states
        private CubeState[,,] grid;
        private GameObject[,,] currentGrid;

        // Prefabs
        [SerializeField] GameObject prefabRed;
        [SerializeField] GameObject prefabGreen;
        [SerializeField] GameObject prefabBlue;
        [SerializeField] GameObject prefabBlack;
        [SerializeField] GameObject prefabWhite;
        [SerializeField] GameObject prefabEmpty;


        private List<Rule> oldRules = new List<Rule>
    {
        // Define your rules here
        new Rule(new CubeState[] { CubeState.Red, CubeState.Empty }, new CubeState[] { CubeState.Green, CubeState.Red }),
        new Rule(new CubeState[] { CubeState.Red, CubeState.Green }, new CubeState[] { CubeState.White, CubeState.Red }),
        new Rule(new CubeState[] { CubeState.Red, CubeState.Empty, CubeState.Empty }, new CubeState[] { CubeState.Green, CubeState.Green, CubeState.Red }),
        new Rule(new CubeState[] { CubeState.Red, CubeState.Green, CubeState.Green }, new CubeState[] { CubeState.Black, CubeState.Black, CubeState.Red }),
    };

        private List<Rule> rules = new List<Rule>
    {
        // Spread green prefabs on the ground
        new Rule(new CubeState[] { CubeState.Empty }, new CubeState[] { CubeState.Green }),
    
        // Grow white columns from green prefabs
        new Rule(new CubeState[] { CubeState.Green, CubeState.Empty }, new CubeState[] { CubeState.Green, CubeState.White }),
        new Rule(new CubeState[] { CubeState.Red, CubeState.Empty, CubeState.Empty }, new CubeState[] { CubeState.Green, CubeState.Green, CubeState.Red }),
        new Rule(new CubeState[] { CubeState.Red, CubeState.Green, CubeState.Green }, new CubeState[] { CubeState.Black, CubeState.Black, CubeState.Red }),
    
        // Transform columns into black prefabs
        new Rule(new CubeState[] { CubeState.White }, new CubeState[] { CubeState.Black }),
      };

        void Start()
        {
            Debug.Log("simulation begin");
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

        IEnumerator ApplyRulesCoroutine()
        {
            // Apply the ground spreading rule only once
            ApplyGroundSpreadRule();

            bool transformationApplied;
            do
            {
                transformationApplied = false;

                // Apply rules for white columns growing from green blocks
                transformationApplied |= ApplyColumnGrowthRule();

                // Apply rules for black blocks growing vertically with randomness
                transformationApplied |= ApplyVerticalGrowthRule();

                yield return new WaitForSeconds(0.1f); // Timing for visual effect
            } while (transformationApplied);

            Debug.Log("Grid processing complete.");
        }

        bool ApplyColumnGrowthRule()
        {
            bool ruleApplied = false;
            // Apply the white growth rule with some probability to create columns
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    if (Random.value < columnGrowthProbability) // Adjust columnGrowthProbability as needed
                    {
                        ruleApplied |= ApplyRule(rules[1], 0); // Apply white column growth rule
                    }
                }
            }
            return ruleApplied;
        }

        bool ApplyVerticalGrowthRule()
        {
            bool ruleApplied = false;
            // Apply the black growth rule with some probability to create vertical growth
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    for (int y = 1; y < height; y++)
                    {
                        if (Random.value < verticalGrowthProbability) // Adjust verticalGrowthProbability as needed
                        {
                            ruleApplied |= ApplyRule(rules[2], 0, y); // Apply black vertical growth rule with y as height parameter
                        }
                        else
                        {
                            ruleApplied |= ApplyRule(rules[3], 0, y);
                        }
                    }
                }
            }
            return ruleApplied;
        }



        bool ApplyRule(Rule rule, int lockSeed, float probabilityOfGrowth = 1.0f)
        {
            bool ruleApplied = false;
            // Loop through the entire grid
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        Vector3Int[] directions = GetDirections(lockSeed);

                        foreach (var direction in directions)
                        {
                            Vector3Int currentPos = new Vector3Int(x, y, z);
                            if (CheckRule(rule, grid, currentPos, direction))
                            {
                                // Apply a probability check before growing the block
                                if (Random.value < probabilityOfGrowth)
                                {
                                    UpdateGrid(rule, grid, currentPos, direction);
                                    ruleApplied = true;
                                }
                            }
                        }
                    }
                }
            }

            return ruleApplied;
        }


        void ApplyGroundSpreadRule()
        {
            // Apply the green ground spreading rule across the entire bottom layer (y = 0)
            for (int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    UpdateGrid(rules[0], grid, new Vector3Int(x, 0, z), Vector3Int.up);
                }
            }
        }

        bool ApplyRule(Rule rule, int lockSeed)
        {
            bool ruleApplied = false;
            // Loop through the entire grid
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < size; z++)
                    {

                        //lockSeed = (y < 3) ? 1 : 0;
                        lockSeed = 0;
                        Vector3Int[] directions = GetDirections(lockSeed);

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

                        if (x == size - 1 && y == height - 1 && z == size - 1)
                        {
                            isGridProcessingComplete = true;
                        }
                    }
                }
            }

            return ruleApplied;
        }

        bool ApplyRule(int y)
        {
            bool ruleApplied = false;

            // Choose rules and lockSeed based on the y coordinate
            List<Rule> selectedRules;
            if (y < 3)
            {
                selectedRules = new List<Rule> { rules[0], rules[1] };
                lockSeed = 1; // Horizontal
            }
            else if (y < 7)
            {
                selectedRules = new List<Rule> { rules[2], rules[3] };
                lockSeed = 0; // No lock
            }
            else
            {
                selectedRules = new List<Rule> { rules[2], rules[3] }; // Assuming you want to apply the same rules, adjust if needed
                lockSeed = 2; // Vertical
            }

            Vector3Int[] directions = GetDirections(lockSeed);

            // Apply the selected rules
            foreach (Rule rule in selectedRules)
            {
                // Apply each rule to the entire grid
                for (int x = 0; x < size; x++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        foreach (var direction in directions)
                        {
                            Vector3Int currentPos = new Vector3Int(x, y, z);
                            if (CheckRule(rule, grid, currentPos, direction))
                            {
                                UpdateGrid(rule, grid, currentPos, direction);
                                ruleApplied = true;
                            }
                        }
                    }
                }
            }

            return ruleApplied;
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

        Vector3Int[] GetDirections(int lockSeed = 0)
        {
            Vector3Int[] array = new Vector3Int[]
            {
        new Vector3Int(1, 0, 0), // right
        new Vector3Int(-1, 0, 0), // left
        new Vector3Int(0, 1, 0), // up
        new Vector3Int(0, -1, 0), // down
        new Vector3Int(0, 0, 1), // forward
        new Vector3Int(0, 0, -1), // backward
            };

            switch (lockSeed)
            {
                case 1: // Horizontal directions
                    array = new Vector3Int[]
                    {
                new Vector3Int(1, 0, 0), // right
                new Vector3Int(-1, 0, 0), // left
                new Vector3Int(0, 0, 1), // forward
                new Vector3Int(0, 0, -1), // backward
                    };
                    break;

                case 2: // Vertical directions
                    array = new Vector3Int[]
                    {
                new Vector3Int(0, 1, 0), // up
                new Vector3Int(0, -1, 0), // down
                    };
                    break;

                default: // Default directions 
                    break;
            }

            // Shuffle the array
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
}