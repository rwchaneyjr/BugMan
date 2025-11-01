using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class JoshFanSprayer : MonoBehaviour
{
    [Header("Hooks")]
    public Transform nozzle;
    public LineRenderer linePrefab;
    public LayerMask hitMask;

    [Header("Fan Settings")]
    [Range(2, 64)] public int lineCount = 12;
    [Range(0f, 180f)] public float fanAngle = 80f;
    public float maxLength = 8f;

    [Header("Timing")]
    public float growTime = 0.12f;
    public float holdTime = 0.06f;
    public float fadeTime = 0.18f;

    [Header("Style")]
    public int jaggedSegments = 5;
    public float jaggedAmplitude = 0.08f;
    public float noiseSeedSpeed = 8f;

    [Header("Pooling")]
    public int poolSize = 24;

    private readonly List<LineRenderer> pool = new();
    private int poolIndex = 0;
    private float noiseSeed = 0f;
    private bool initialized = false;

    void Awake()
    {
        if (!nozzle) nozzle = transform;
        TryInitializePool();
    }

    void Update()
    {
        if (!initialized) return;
        noiseSeed += Time.deltaTime * noiseSeedSpeed;

        // Manual test
        if (Input.GetMouseButtonDown(0))
            FireFan();
    }

    // ======================================================
    // 🔹 Main continuous firing coroutine
    public IEnumerator FireForSeconds(float seconds)
    {
        float timer = 0f;
        while (timer < seconds)
        {
            FireFan();
            yield return new WaitForSeconds(0.1f); // fire burst every 0.1s
            timer += 0.1f;
        }
    }

    // ======================================================
    public void FireFan()
    {
        if (!initialized) return;

        float startAngle = -fanAngle * 0.5f;
        for (int i = 0; i < lineCount; i++)
        {
            float t = (lineCount == 1) ? 0.5f : (float)i / (lineCount - 1);
            float angle = startAngle + fanAngle * t;
            Vector3 dir = Quaternion.AngleAxis(angle, nozzle.up) * nozzle.forward;

            float length = maxLength;
            if (Physics.Raycast(nozzle.position, dir, out RaycastHit hit, maxLength, hitMask))
                length = hit.distance;

            LineRenderer lr = GetLine();
            if (lr == null) continue;
            StartCoroutine(AnimateLineBurst(lr, nozzle.position, dir, length));
        }
    }

    private IEnumerator AnimateLineBurst(LineRenderer lr, Vector3 origin, Vector3 dir, float length)
    {
        int segments = Mathf.Max(1, jaggedSegments);
        lr.positionCount = segments + 1;

        float t = 0f;
        while (t < growTime)
        {
            t += Time.deltaTime;
            SetJaggedPositions(lr, origin, dir, length * (t / growTime));
            yield return null;
        }

        float h = 0f;
        while (h < holdTime)
        {
            h += Time.deltaTime;
            SetJaggedPositions(lr, origin, dir, length);
            yield return null;
        }

        float f = 0f;
        while (f < fadeTime)
        {
            f += Time.deltaTime;
            SetJaggedPositions(lr, origin, dir, length * (1f - f / fadeTime));
            yield return null;
        }

        lr.positionCount = 0;
        lr.gameObject.SetActive(false);
    }

    private void SetJaggedPositions(LineRenderer lr, Vector3 start, Vector3 dir, float length)
    {
        int segments = lr.positionCount - 1;
        if (segments <= 0) return;

        Vector3 right = Vector3.Cross(dir, Vector3.up);
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(dir, Vector3.right);
        right.Normalize();

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;
            Vector3 p = start + dir * (length * t);
            float wobble = (segments > 1)
                ? (Mathf.PerlinNoise(noiseSeed, t * 7.13f) - 0.5f) * 2f
                : 0f;
            p += right * wobble * jaggedAmplitude;
            lr.SetPosition(i, p);
        }
    }

    // ======================================================
    private void TryInitializePool()
    {
        if (!linePrefab)
        {
            Debug.LogError("❌ JoshFanSprayer: linePrefab not assigned. Disabled.");
            enabled = false;
            return;
        }

        int want = Mathf.Max(poolSize, lineCount);
        for (int i = 0; i < want; i++)
        {
            LineRenderer lr = Instantiate(linePrefab, transform);
            lr.positionCount = 0;
            lr.useWorldSpace = true;
            lr.gameObject.SetActive(false);
            pool.Add(lr);
        }
        initialized = true;
    }

    private LineRenderer GetLine()
    {
        if (pool.Count == 0) return null;
        poolIndex = (poolIndex + 1) % pool.Count;
        LineRenderer lr = pool[poolIndex];
        lr.gameObject.SetActive(true);
        return lr;
    }
}
