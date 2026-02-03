using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DummyPlayerMove : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float xMin = -3f;
    [SerializeField] private float xMax = 3f;

    private Rigidbody rb;
    private float inputX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Recommended settings for a "player you push around"
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        // Keep it from tipping over (optional)
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        // Read input in Update (recommended), apply movement in FixedUpdate
        inputX = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) inputX = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) inputX = 1f;
    }

    private void FixedUpdate()
    {
        Vector3 pos = rb.position;
        pos.x = Mathf.Clamp(pos.x + inputX * speed * Time.fixedDeltaTime, xMin, xMax);

        rb.MovePosition(pos);
    }
}
