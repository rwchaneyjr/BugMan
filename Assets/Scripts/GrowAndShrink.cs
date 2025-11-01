// Example snippet
public class GrowAndShrink : MonoBehaviour
{
    public float growTime = 3f;
    public float shrinkTime = 2f;
    public float targetScale = 10f;
    public bool growFirst = true;

    IEnumerator Start()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 bigScale = originalScale * targetScale;

        if (growFirst)
            yield return ScaleOverTime(originalScale, bigScale, growTime);

        yield return new WaitForSeconds(1f); // pause

        yield return ScaleOverTime(transform.localScale, originalScale, shrinkTime);
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
