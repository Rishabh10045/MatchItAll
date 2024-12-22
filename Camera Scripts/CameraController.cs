using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GridManager gridManager;

    void Start()
    {
        // Find the GridManager in the scene
        gridManager = FindObjectOfType<GridManager>();

        if (gridManager != null)
        {
            AdjustCamera();
        }
        else
        {
            Debug.LogError("GridManager not found in the scene!");
        }
    }

    void AdjustCamera()
    {
        // Fetch grid properties from the GridManager
        int rows = gridManager.rows;
        int columns = gridManager.columns;
        float cellSize = gridManager.cellSize;

        // Calculate the center position of the grid
        float gridWidth = columns * cellSize;
        float gridHeight = rows * cellSize;

        float centerX = (gridWidth / 2f) - (cellSize / 2f); // Account for cell size
        float centerY = -(gridHeight / 2f) + (cellSize / 2f);

        // Set the camera position to center on the grid
        transform.position = new Vector3(centerX, centerY, -10f);

        // Adjust the camera size to fit the grid
        float verticalSize = gridHeight / 2f + 1; // Extra padding for vertical alignment
        float horizontalSize = (gridWidth / 2f) * (Screen.height / (float)Screen.width);

        Camera.main.orthographicSize = Mathf.Max(verticalSize, horizontalSize);
    }
}
