using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SimpleHose : MonoBehaviour
{
    [Header("Anchors")]
    public Transform startPoint;      // Josh's wand tip
    public Transform endPoint;        // target point (end of hose / spray)

    [Header("Visuals")]
    public int segments = 20;         // smoothness
    public float sagAmount = 8f;    // how much the hose droops
    public float wiggleSpeed = 2f;    // small motion
    public float wiggleAmplitude = 7f;

    private LineRenderer lr;
    private Vector3[] points;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        points = new Vector3[segments];
    }

    void LateUpdate()
    {
        if (!startPoint || !endPoint) return;

        Vector3 p0 = startPoint.position;
        Vector3 p1 = endPoint.position;
        Vector3 mid = (p0 + p1) * 0.5f;

        // sag downward in world space
        mid.y -= sagAmount;

        // simple sine wiggle
        float wiggle = Mathf.Sin(Time.time * wiggleSpeed) * wiggleAmplitude;
        mid += transform.right * wiggle;

        // draw a curve from start → mid → end
        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            // quadratic bezier curve
            Vector3 a = Vector3.Lerp(p0, mid, t);
            Vector3 b = Vector3.Lerp(mid, p1, t);
            points[i] = Vector3.Lerp(a, b, t);
        }

        lr.positionCount = segments;
        lr.SetPositions(points);
    }
}
