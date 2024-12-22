using UnityEngine;

public class CandyController : MonoBehaviour
{
    private GridManager gridManager; // Reference to the GridManager

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>(); // Find the GridManager in the scene
    }

    // Handle mouse down to select the first candy
    private void OnMouseDown()
    {
        // Notify GridManager that this candy is selected
        gridManager.SelectFirstCandy(this.gameObject);
    }

    // Handle mouse up to select the target candy
    private void OnMouseUp()
    {
        // Notify GridManager that the mouse button is released and select the target candy
        gridManager.SelectTargetCandy(this.gameObject);
    }
}
