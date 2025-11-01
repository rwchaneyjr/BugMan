using System.Collections;
using UnityEngine;

/// Put this on each character that has an Animator.
/// Sequence: (optional jog/idle) -> Dance (15s) -> Stop (2s) -> Die
[RequireComponent(typeof(Animator))]
public class SimpleDanceThenDie : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;                  // auto-filled in Awake

    [Tooltip("Use Animator parameters (Speed bool/trigger) or direct state names.")]
    public bool useParameters = true;

    [Header("When useParameters = true (recommended)")]
    [Tooltip("Float parameter used by your locomotion blend tree. Set to 0 for idle, >0 for jog.")]
    public string speedParam = "Speed";
    [Tooltip("Bool that turns on your dance state via transitions.")]
    public string danceBoolParam = "IsDancing";
    [Tooltip("Trigger to play your Die animation.")]
    public string dieTriggerParam = "Die";

    [Header("When useParameters = false (direct state names)")]
    [Tooltip("Exact state name in Base Layer that idles/jogs (e.g., 'Jogging' or 'Idle').")]
    public string jogStateName = "Jogging";
    [Tooltip("Exact state name that dances (e.g., 'Hip Hop Dancing').")]
    public string danceStateName = "Hip Hop Dancing";
    [Tooltip("Exact state name that plays the death animation (e.g., 'Die').")]
    public string dieStateName = "Die";

    [Header("Sequence Settings")]
    [Tooltip("Start with jogging/idle phase before dance.")]
    public bool startWithJog = true;
    [Tooltip("Speed value to set during the initial phase (0 = idle, 1 = jog).")]
    [Range(0f, 5f)] public float startSpeed = 0f;
    [Tooltip("How long to stay in start phase before dancing.")]
    public float startPhaseSeconds = 1.0f;

    [Tooltip("How long to dance before stopping.")]
    public float danceSeconds = 15.0f;

    [Tooltip("How long to wait after stopping before playing Die.")]
    public float waitBeforeDieSeconds = 2.0f;

    [Header("Crossfade / Layer")]
    [Tooltip("Layer index for direct state plays (usually 0).")]
    public int layerIndex = 0;
    [Tooltip("Crossfade time when using direct state plays.")]
    public float crossfadeSeconds = 0.15f;

    [Header("Stagger Options (optional)")]
    [Tooltip("Randomly delay the whole sequence per character to avoid perfect sync.")]
    public float randomStartDelayMax = 0.0f;

    [Header("Debug")]
    public bool verboseLogs = true;

    void Awake()
    {
        if (!animator) animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        // Optional per-character random offset so the 3 animals don’t start together.
        float offset = (randomStartDelayMax > 0f) ? Random.Range(0f, randomStartDelayMax) : 0f;
        if (offset > 0f) yield return new WaitForSeconds(offset);

        // --- Phase 1: Start (idle/jog) ---
        if (startWithJog)
        {
            if (useParameters)
            {
                SafeSetFloat(speedParam, startSpeed);
                SafeSetBool(danceBoolParam, false); // ensure not dancing
                Log($"Start phase: set {speedParam}={startSpeed}, dancing=false for {startPhaseSeconds:0.00}s.");
            }
            else
            {
                if (!string.IsNullOrEmpty(jogStateName))
                {
                    animator.CrossFadeInFixedTime(jogStateName, crossfadeSeconds, layerIndex);
                    Log($"Start phase: CrossFade to '{jogStateName}' for {startPhaseSeconds:0.00}s.");
                }
            }
            yield return new WaitForSeconds(startPhaseSeconds);
        }

        // --- Phase 2: Dance (for danceSeconds) ---
        if (useParameters)
        {
            SafeSetFloat(speedParam, 0f);            // stop moving while dancing
            SafeSetBool(danceBoolParam, true);       // transition to dance
            Log($"Dance ON ({danceSeconds:0.00}s).");
        }
        else
        {
            if (!string.IsNullOrEmpty(danceStateName))
            {
                animator.CrossFadeInFixedTime(danceStateName, crossfadeSeconds, layerIndex);
                Log($"CrossFade to dance '{danceStateName}' for {danceSeconds:0.00}s.");
            }
        }

        yield return new WaitForSeconds(danceSeconds);

        // --- Phase 3: Stop dance ---
        if (useParameters)
        {
            SafeSetBool(danceBoolParam, false);
            SafeSetFloat(speedParam, 0f); // ensure idle
            Log($"Dance OFF. Waiting {waitBeforeDieSeconds:0.00}s before Die.");
        }
        else
        {
            // Go back to idle/jog state briefly before die (optional)
            if (!string.IsNullOrEmpty(jogStateName))
            {
                animator.CrossFadeInFixedTime(jogStateName, crossfadeSeconds, layerIndex);
                Log($"CrossFade back to '{jogStateName}'. Waiting {waitBeforeDieSeconds:0.00}s before Die.");
            }
        }

        yield return new WaitForSeconds(waitBeforeDieSeconds);

        // --- Phase 4: Die ---
        if (useParameters)
        {
            SafeSetTrigger(dieTriggerParam);
            Log("Triggered Die.");
        }
        else
        {
            if (!string.IsNullOrEmpty(dieStateName))
            {
                animator.CrossFadeInFixedTime(dieStateName, crossfadeSeconds, layerIndex);
                Log($"CrossFade to Die '{dieStateName}'.");
            }
        }
    }

    // ---- Safe parameter helpers (won’t spam warnings if param is missing) ----
    void SafeSetFloat(string param, float value)
    {
        if (string.IsNullOrEmpty(param)) return;
        if (HasParam(param, AnimatorControllerParameterType.Float))
            animator.SetFloat(param, value);
        else
            LogWarn($"Animator missing float '{param}'.");
    }

    void SafeSetBool(string param, bool value)
    {
        if (string.IsNullOrEmpty(param)) return;
        if (HasParam(param, AnimatorControllerParameterType.Bool))
            animator.SetBool(param, value);
        else
            LogWarn($"Animator missing bool '{param}'.");
    }

    void SafeSetTrigger(string param)
    {
        if (string.IsNullOrEmpty(param)) return;
        if (HasParam(param, AnimatorControllerParameterType.Trigger))
            animator.SetTrigger(param);
        else
            LogWarn($"Animator missing trigger '{param}'.");
    }

    bool HasParam(string name, AnimatorControllerParameterType type)
    {
        foreach (var p in animator.parameters)
            if (p.type == type && p.name == name) return true;
        return false;
    }

    void Log(string msg)
    {
        if (verboseLogs) Debug.Log($"[{name}] {msg}");
    }
    void LogWarn(string msg)
    {
        if (verboseLogs) Debug.LogWarning($"[{name}] {msg}");
    }
}
