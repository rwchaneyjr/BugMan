using UnityEngine;

[RequireComponent(typeof(Animator))]
public class JoshIK_MouseBeam : MonoBehaviour
{
    [Header("IK Targets")]
    public Transform handTarget;   // drag the cube here
    public Transform sprayOrigin;  // tip of the gun
    public LineRenderer beam;      // beam visual

    [Header("Beam Settings")]
    public float beamDuration = 0.1f;
    public float beamRange = 20f;
    public float beamWidth = 0.03f;
    public LayerMask hitMask = Physics.DefaultRaycastLayers;

    private Animator anim;
    private float beamTimer;

    void Start()
    {
        anim = GetComponent<Animator>();
        if (beam)
        {
            beam.positionCount = 2;
            beam.useWorldSpace = true;
            beam.startWidth = beamWidth;
            beam.endWidth = beamWidth;
            beam.enabled = false;
        }
    }

    void Update()
    {
        // Move handTarget to mouse aim point
        if (Camera.main)
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(r, out RaycastHit hit, 100f, hitMask))
            {
                handTarget.position = hit.point;
            }
            else
            {
                handTarget.position = r.origin + r.direction * 5f;
            }
        }

        // Left-click to fire
        if (Input.GetMouseButtonDown(0))
        {
            FireBeam();
        }

        // Hide beam after timer
        if (beam && beam.enabled)
        {
            beamTimer -= Time.deltaTime;
            if (beamTimer <= 0f) beam.enabled = false;
        }
    }

    void FireBeam()
    {
        if (!sprayOrigin || !beam) return;

        Vector3 start = sprayOrigin.position;
        Vector3 dir = sprayOrigin.forward;
        Vector3 end = start + dir * beamRange;

        // Raycast for collision
        if (Physics.Raycast(start, dir, out RaycastHit hit, beamRange, hitMask))
        {
            end = hit.point;
            // Optional: if target has a BugHit script
            if (hit.collider.TryGetComponent(out BugHit bug))
                bug.OnSprayed();
        }

        // Draw beam
        beam.SetPosition(0, start);
        beam.SetPosition(1, end);
        beam.enabled = true;
        beamTimer = beamDuration;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!anim || !handTarget) return;

        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        anim.SetIKPosition(AvatarIKGoal.RightHand, handTarget.position);

        // make hand face forward toward target
        Vector3 fwd = (handTarget.position - transform.position).normalized;
        anim.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(fwd, transform.up));
    }
}
