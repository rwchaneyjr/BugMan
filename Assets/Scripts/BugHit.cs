using UnityEngine;

public class BugHit : MonoBehaviour
{
    [Header("What happens when sprayed")]
    public bool triggerAnimatorDie = true;
    public string dieTriggerName = "Die";
    public float destroyDelay = 1.0f;   // seconds after spray to remove
    public bool shrinkOnSpray = true;
    public float shrinkTo = 0.15f;
    public float shrinkDuration = 0.25f;

    public void OnSprayed()
    {
        // 1) Try to play a Die animation
        var anim = GetComponent<Animator>();
        if (triggerAnimatorDie && anim && !string.IsNullOrEmpty(dieTriggerName))
            anim.SetTrigger(dieTriggerName);

        // 2) Optional quick shrink visual
        if (shrinkOnSpray)
            StartCoroutine(ShrinkThenDestroy());

        // 3) Or just destroy after delay if you don’t want shrink
        if (!shrinkOnSpray)
            Destroy(gameObject, destroyDelay);
    }

    System.Collections.IEnumerator ShrinkThenDestroy()
    {
        Vector3 start = transform.localScale;
        Vector3 end = start * shrinkTo;
        float t = 0f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, end, t / shrinkDuration);
            yield return null;
        }
        Destroy(gameObject, destroyDelay);
    }
}
