using System.Collections;
using UnityEngine;

public class CandyEffect : MonoBehaviour
{
    public ParticleSystem popEffect; // Reference to the particle system
    public float shrinkDuration = 0.2f;
    public float minScale = 0.1f;

    // Method to trigger the effect
    public void TriggerEffect()
    {
        StartCoroutine(ShrinkAndPop());
    }

    private IEnumerator ShrinkAndPop()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 targetScale = new Vector3(minScale, minScale, minScale);

        float elapsedTime = 0f;

        // Gradually shrink the object
        while (elapsedTime < shrinkDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / shrinkDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the final scale is the target scale
        transform.localScale = targetScale;

        // Trigger particle effect
        if (popEffect != null)
        {
            popEffect.transform.parent = null; // Detach the particle system
            popEffect.Play();
            Destroy(popEffect.gameObject, popEffect.main.duration); // Clean up particles after they finish
        }

        // Destroy the candy object
        Destroy(gameObject);
    }
}
