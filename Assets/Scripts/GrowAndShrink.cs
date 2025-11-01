using System.Collections;
using UnityEngine;

public class GrowAndShrink : MonoBehaviour
{
    [Header("Scaling Settings")]
    public float growDuration = 15f;
    public float shrinkDuration = 5f;
    public float targetScale = 10f;

    [Header("Sprayer Reference")]
    public JoshFanSprayer sprayer;      // drag Josh's sprayer here
    public float sprayDuration = 5f;

    [Header("Disappearance")]
    public bool destroyAfter = true;    // true = destroy, false = just hide

    IEnumerator Start()
    {
        Vector3 original = transform.localScale;
        Vector3 large = original * targetScale;

        // 1️⃣ Grow up
        yield return ScaleOverTime(original, large, growDuration);

        // 2️⃣ Start spraying + shrink together
        if (sprayer) StartCoroutine(sprayer.FireForSeconds(sprayDuration));
        yield return ScaleOverTime(large, original, shrinkDuration);

        // 3️⃣ Remove object
        if (destroyAfter) Destroy(gameObject);
        else gameObject.SetActive(false);
    }

    IEnumerator ScaleOverTime(Vector3 from, Vector3 to, float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(from, to, t / duration);
            yield return null;
        }
        transform.localScale = to;
    }
}
