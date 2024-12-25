using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GridManager : MonoBehaviour
{
    public int rows = 8;
    public int columns = 8;
    public float cellSize = 1.0f;
    public GameObject[] candyPrefabs;
    public GameObject borderPrefab;
    public GameObject[,] grid;

    private GameObject selectedCandy = null; // Store the first selected candy
    private Vector3 mouseDownPosition;      // Position where the mouse button was pressed
    private Vector3 mouseUpPosition;        // Position where the mouse button was released

    private GameObject candy1 = null;       // First candy selected
    private GameObject candy2 = null;       // Second candy selected

    void Start()
    {
        grid = new GameObject[columns, rows]; // Initialize the grid
        GenerateGridWithoutMatches(); // Generate the initial grid with candies
    }
    // Generate the initial board without matches
    void GenerateGridWithoutMatches()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                List<string> possibleCandyTags = GetPossibleCandyTags(col, row);
                if (possibleCandyTags.Count > 0)
                {
                    string selectedCandyTag = possibleCandyTags[Random.Range(0, possibleCandyTags.Count)];

                    Vector3 spawnPosition = new Vector3(col * cellSize, (rows + 1) * cellSize, 0); // Start above grid
                    Vector3 targetPosition = new Vector3(col * cellSize, row * -cellSize, 0); // Final position in grid

                    // Spawn border (if applicable)
                    if (borderPrefab != null)
                    {
                        GameObject border = Instantiate(borderPrefab, spawnPosition, Quaternion.identity);
                        border.name = $"Border_{row}_{col}";
                        border.transform.parent = transform;
                    }

                    // Find prefab matching the selected tag
                    GameObject candyPrefab = candyPrefabs.First(prefab => prefab.CompareTag(selectedCandyTag));
                    GameObject candy = Instantiate(candyPrefab, spawnPosition, Quaternion.identity);

                    candy.name = $"Candy_{row}_{col}";
                    candy.transform.parent = transform;

                    grid[col, row] = candy; // Assign to grid

                    // Animate candy falling into place
                    StartCoroutine(FallCandy(candy, targetPosition));
                }
                else
                {
                    Debug.LogError($"No possible candies for position ({col}, {row}). Check candy prefabs and matching logic.");
                }
            }
        }
    }
    List<string> GetPossibleCandyTags(int col, int row)
    {
        List<string> possibleCandyTags = new List<string>();
        foreach (GameObject prefab in candyPrefabs)
        {
            if (!HasMatchAt(col, row, prefab.tag))
            {
                possibleCandyTags.Add(prefab.tag);
            }
        }
        return possibleCandyTags;
    }
    // Clear the grid
    void ClearGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                if (grid[col, row] != null)
                {
                    Destroy(grid[col, row]);
                    grid[col, row] = null;
                }
            }
        }
    }
    bool HasMatchAt(int col, int row, string candyTag)
    {
        // Horizontal match check
        if (col >= 2 &&
            grid[col - 1, row] != null && grid[col - 2, row] != null &&
            grid[col - 1, row].tag == candyTag &&
            grid[col - 2, row].tag == candyTag)
        {
            return true;
        }

        // Vertical match check
        if (row >= 2 &&
            grid[col, row - 1] != null && grid[col, row - 2] != null &&
            grid[col, row - 1].tag == candyTag &&
            grid[col, row - 2].tag == candyTag)
        {
            return true;
        }

        return false; // No match found
    }
    // Coroutine to animate the candy falling into position
    IEnumerator FallCandy(GameObject candy, Vector3 targetPosition)
    {
        if (candy == null) yield break; // Check if the candy is already destroyed

        float fallDuration = 0.5f; // Duration for the candy to fall
        float elapsedTime = 0f;

        Vector3 startPosition = candy.transform.position;

        // Animate the candy falling by interpolating its position
        while (elapsedTime < fallDuration)
        {
            if (candy == null) yield break; // Check if the candy is destroyed during the fall

            candy.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / fallDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (candy != null)
        {
            candy.transform.position = targetPosition; // Ensure it reaches the target position exactly
        }

        // Call CheckForMatches after the fall animation completes
        yield return new WaitForSeconds(0.2f); // Wait before checking for matches
        CheckForMatches(false); // Perform a match check
    }

    // Method to check for matching candies horizontally and vertically
    bool CheckForMatches(bool onlyDetect = false)
    {
        bool hasMatches = false;
        List<GameObject> matchedCandies = new List<GameObject>();

        // Check horizontally for matches
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns - 2; col++) // Stop at columns - 2 to prevent overflow
            {
                if (grid[col, row] != null && grid[col + 1, row] != null && grid[col + 2, row] != null)
                {
                    string candyTag = grid[col, row].tag;
                    if (grid[col + 1, row].tag == candyTag && grid[col + 2, row].tag == candyTag)
                    {
                        matchedCandies.Add(grid[col, row]);
                        matchedCandies.Add(grid[col + 1, row]);
                        matchedCandies.Add(grid[col + 2, row]);
                        hasMatches = true;
                    }
                }
            }
        }

        // Check vertically for matches
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rows - 2; row++) // Stop at rows - 2 to prevent overflow
            {
                if (grid[col, row] != null && grid[col, row + 1] != null && grid[col, row + 2] != null)
                {
                    string candyTag = grid[col, row].tag;
                    if (grid[col, row + 1].tag == candyTag && grid[col, row + 2].tag == candyTag)
                    {
                        matchedCandies.Add(grid[col, row]);
                        matchedCandies.Add(grid[col, row + 1]);
                        matchedCandies.Add(grid[col, row + 2]);
                        hasMatches = true;
                    }
                }
            }
        }

        if (!onlyDetect)
        {
            // Trigger removal effects for matched candies
            foreach (GameObject candy in matchedCandies)
            {
                if (candy != null)
                {
                    CandyEffect effect = candy.GetComponent<CandyEffect>();
                    if (effect != null)
                    {
                        effect.TriggerEffect(); // Trigger the shrinking and particle effect
                    }

                    // Remove candy from grid after effect duration
                    int col = Mathf.RoundToInt(candy.transform.position.x / cellSize);
                    int row = Mathf.RoundToInt(-candy.transform.position.y / cellSize);

                    if (col >= 0 && col < columns && row >= 0 && row < rows) // Boundary check
                    {
                        grid[col, row] = null;
                    }
                }
            }

            // Collapse columns after match 
            // Call collapse and refill after match removal
            StartCoroutine(DelayedCollapseAndRefill());

        }

        return hasMatches;
    }
    IEnumerator DelayedRefillGrid()
    {
        yield return new WaitForSeconds(0.5f); // Delay to allow effects to finish
        CollapseGrid(); // Collapse columns to shift candies down
        yield return new WaitForSeconds(0.6f); // Adjust based on shrinkDuration and popEffect duration
    }
    // Select the first candy (called from CandyController)
    public void SelectFirstCandy(GameObject candy)
    {
        if (selectedCandy == null)
        {
            selectedCandy = candy; // Store the first selected candy
        }
    }

    // Select the target candy and swap if valid (called from CandyController)
    // Select the target candy and swap if valid (only if adjacent)
    public void SelectTargetCandy(GameObject candy)
    {
        if (selectedCandy != null && selectedCandy != candy)
        {
            // Get the grid positions of the selected and target candies
            int selectedCandyCol = Mathf.RoundToInt(selectedCandy.transform.position.x / cellSize);
            int selectedCandyRow = Mathf.RoundToInt(-selectedCandy.transform.position.y / cellSize);
            int targetCandyCol = Mathf.RoundToInt(candy.transform.position.x / cellSize);
            int targetCandyRow = Mathf.RoundToInt(-candy.transform.position.y / cellSize);

            // Check if the candies are adjacent (horizontally or vertically)
            bool areAdjacent = (Mathf.Abs(selectedCandyCol - targetCandyCol) == 1 && selectedCandyRow == targetCandyRow) ||
                               (Mathf.Abs(selectedCandyRow - targetCandyRow) == 1 && selectedCandyCol == targetCandyCol);

            // Only swap if the candies are adjacent
            if (areAdjacent)
            {
                candy1 = selectedCandy;
                candy2 = candy;

                // Store original positions of candies before swapping
                Vector3 candy1OriginalPos = candy1.transform.position;
                Vector3 candy2OriginalPos = candy2.transform.position;

                // Animate the swapping
                StartCoroutine(SwapCandies(candy1, candy2, candy1OriginalPos, candy2OriginalPos));

                // Reset selection
                selectedCandy = null;
            }
            else
            {
                // Optionally, you could provide feedback to the player if the candies are not adjacent
                // Reset selection
                selectedCandy = null;
                Debug.Log("Selected candy is not adjacent to the target candy.");
            }
        }
    }

    IEnumerator DelayedCollapseAndRefill()
    {
        yield return new WaitForSeconds(0.5f); // Delay to allow effects to finish
        CollapseGrid(); // Collapse columns to shift candies down
    }
    void CollapseColumn(int col)
    {
        bool columnChanged;

        do
        {
            columnChanged = false; // Reset flag for each cycle

            // Loop through the rows from bottom to top (starting from row 1 to prevent overwriting candies)
            for (int row = rows - 1; row > 0; row--)
            {
                // If the current row is empty and there's a candy in the row above it
                if (grid[col, row] == null && grid[col, row - 1] != null)
                {
                    // Move the candy from above to the current empty spot
                    grid[col, row] = grid[col, row - 1];
                    grid[col, row].transform.position = new Vector3(col * cellSize, row * -cellSize, 0);
                    grid[col, row - 1] = null; // Clear the old position

                    columnChanged = true; // Mark that a change was made
                }
            }

        } while (columnChanged); // Keep checking until no further shifts are needed

        // Now, after all candies have fallen, spawn new candies at the top for any remaining empty spots
        for (int row = 0; row < rows; row++)
        {
            if (grid[col, row] == null)
            {
                // Spawn a new candy just above the grid (row 0)
                GameObject newCandy = Instantiate(
                    candyPrefabs[Random.Range(0, candyPrefabs.Length)],
                    new Vector3(col * cellSize, (rows+1) * cellSize, 0), // Spawn above the grid
                    Quaternion.identity
                );
                newCandy.transform.parent = transform; // Make sure it is a child of the Grid Manager

                // Move the new candy to the current empty position (row 0, 1, etc.)
                grid[col, row] = newCandy;

                // Start the falling animation for the new candy
                StartCoroutine(FallCandy(newCandy, new Vector3(col * cellSize, row * -cellSize, 0)));
            }
        }
    }

    // Call this collapse method after checking and removing matches
    void CollapseGrid()
    {
        for (int col = 0; col < columns; col++)
        {
            CollapseColumn(col); // Collapse each column
        }
    }

    // Coroutine to handle candy swapping animation and logic
    private IEnumerator SwapCandies(GameObject candy1, GameObject candy2, Vector3 originalPos1, Vector3 originalPos2)
    {
        float swapDuration = 0.3f; // Duration for the swap animation
        float elapsedTime = 0f;

        Vector3 startPos1 = candy1.transform.position;
        Vector3 startPos2 = candy2.transform.position;

        // Animate the swapping of candies
        while (elapsedTime < swapDuration)
        {
            candy1.transform.position = Vector3.Lerp(startPos1, originalPos2, elapsedTime / swapDuration);
            candy2.transform.position = Vector3.Lerp(startPos2, originalPos1, elapsedTime / swapDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure candies reach the target positions
        candy1.transform.position = originalPos2;
        candy2.transform.position = originalPos1;

        // Update the grid
        int candy1Col = Mathf.RoundToInt(originalPos2.x / cellSize);
        int candy1Row = Mathf.RoundToInt(-originalPos2.y / cellSize);

        int candy2Col = Mathf.RoundToInt(originalPos1.x / cellSize);
        int candy2Row = Mathf.RoundToInt(-originalPos1.y / cellSize);

        grid[candy1Col, candy1Row] = candy1;
        grid[candy2Col, candy2Row] = candy2;

        // Check for matches after swapping
        if (!CheckForMatches()) // If no matches found, swap back
        {
            elapsedTime = 0f;

            // Animate the candies swapping back
            while (elapsedTime < swapDuration)
            {
                candy1.transform.position = Vector3.Lerp(originalPos2, startPos1, elapsedTime / swapDuration);
                candy2.transform.position = Vector3.Lerp(originalPos1, startPos2, elapsedTime / swapDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Ensure candies return to their original positions
            candy1.transform.position = startPos1;
            candy2.transform.position = startPos2;

            // Restore the grid to its original state
            grid[candy1Col, candy1Row] = candy1;
            grid[candy2Col, candy2Row] = candy2;
        }
    }

}




