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

    private float _t;
    private bool _firedPlayerZone;
    private bool gotHit;

    private Rigidbody _rb;

    private void Awake()
    {
        // Ensure Rigidbody exists on the root (runtime only)
        _rb = GetComponent<Rigidbody>();
        if (_rb == null) _rb = gameObject.AddComponent<Rigidbody>();

        _rb.isKinematic = true;
        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
    }

    public void Initialize(Transform start, Transform player, Transform end,
                           float duration, AnimationCurve curve, float evaluateWindowSeconds)
    {
        _start = start;
        _player = player;
        _end = end;

        _duration = Mathf.Max(0.01f, duration);
        _curve = curve ?? AnimationCurve.Linear(0, 0, 1, 1);

        _t = 0f;
        _firedPlayerZone = false;
        gotHit = false;

        _rb.position = _start.position;
        _rb.rotation = _start.rotation;
    }

    private void FixedUpdate()
    {
        if (_start == null || _end == null) return;

        _t += Time.fixedDeltaTime;
        float u = Mathf.Clamp01(_t / _duration);
        float curved = Mathf.Clamp01(_curve.Evaluate(u));

        Vector3 newPos = Vector3.Lerp(_start.position, _end.position, curved);
        Quaternion newRot = Quaternion.Slerp(_start.rotation, _end.rotation, curved);

        _rb.MovePosition(newPos);
        _rb.MoveRotation(newRot);

        if (!_firedPlayerZone && _player != null)
        {
            float dist = Vector3.Distance(newPos, _player.position);
            if (dist <= 0.15f)
            {
                _firedPlayerZone = true;
                ReachedPlayerZone?.Invoke(this);
            }
        }

        if (u >= 1f)
        {
            Finished?.Invoke(this);

            if (!gotHit)
            {
                Debug.Log("Point Scored!");
                MusicController.Instance.PlayCheer();
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the thing entering the area is the Player
        if (other.CompareTag("Player"))
        {
            if (gotHit) return;
            gotHit = true;
            Debug.Log("Player entered the boundary!");
            MusicController.Instance.PlayBoo();
        }
    }
}
