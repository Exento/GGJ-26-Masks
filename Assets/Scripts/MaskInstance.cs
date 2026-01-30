using System;
using UnityEngine;

public class MaskInstance : MonoBehaviour
{
    public event Action<MaskInstance> ReachedPlayerZone;
    public event Action<MaskInstance> Finished;

    private Transform _start;
    private Transform _player;
    private Transform _end;

    private float _duration;
    private AnimationCurve _curve;
    private float _evalWindow;

    private float _t;
    private bool _firedPlayerZone;

    // Called right after spawn
    public void Initialize(Transform start, Transform player, Transform end,
                           float duration, AnimationCurve curve, float evaluateWindowSeconds)
    {
        _start = start;
        _player = player;
        _end = end;
        _duration = Mathf.Max(0.01f, duration);
        _curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);
        _evalWindow = Mathf.Max(0.01f, evaluateWindowSeconds);

        transform.position = _start.position;
        transform.rotation = _start.rotation;

        _t = 0f;
        _firedPlayerZone = false;
    }

    private void Update()
    {
        _t += Time.deltaTime;
        float u = Mathf.Clamp01(_t / _duration);
        float curved = Mathf.Clamp01(_curve.Evaluate(u));

        // Position and rotation interpolation
        transform.position = Vector3.Lerp(_start.position, _end.position, curved);
        transform.rotation = Quaternion.Slerp(_start.rotation, _end.rotation, curved);

        // Detect when we're "at player point"
        // We do it by proximity; you can swap to plane-crossing if you want stricter logic.
        if (!_firedPlayerZone)
        {
            float dist = Vector3.Distance(transform.position, _player.position);

            // Evaluate window expressed as distance-ish: we convert time window into normalized u window
            // Simple approach: fire once when u crosses the player’s projected u (approx by distances)
            // For now: just use a threshold radius around player point:
            if (dist <= 0.15f) // tweak or expose on definition/prefab
            {
                _firedPlayerZone = true;
                ReachedPlayerZone?.Invoke(this);
            }
        }

        if (u >= 1f)
        {
            Finished?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
