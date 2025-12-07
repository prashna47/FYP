using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player3DMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float gravity = -20f;          // Stronger than -9.81 to feel snappy
    private float _verticalVelocity;      // Our Y speed

    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int SpeedParam = Animator.StringToHash("Speed");      // FLOAT
    private static readonly int WalkingParam = Animator.StringToHash("Walk");  // BOOL

    private CharacterController _controller;
    private Animator _animator;

    // Remember last direction (X = left/right, Y = up/down in 2D sense)
    private Vector2 _lastMoveDir = new Vector2(0f, -1f);

    void Start()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();

        if (_animator == null)
        {
            Debug.LogError("Player3DMovement: No Animator found on this GameObject or its children.");
        }
    }

    void Update()
    {
        // --- INPUT ---
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector2 inputDir2D = new Vector2(inputX, inputZ);
        if (inputDir2D.sqrMagnitude > 0.001f)
        {
            _lastMoveDir = inputDir2D.normalized;
        }

        // --- HORIZONTAL MOVE (XZ plane) ---
        Vector3 moveDir = new Vector3(inputX, 0f, inputZ);
        if (moveDir.magnitude > 1f)
            moveDir.Normalize();

        // --- GRAVITY ---
        // CharacterController tells us if we’re on the ground
        if (_controller.isGrounded)
        {
            // Small downward force to keep us snapped to ground
            if (_verticalVelocity < 0f)
                _verticalVelocity = -2f;
        }
        else
        {
            // Apply gravity over time when in the air
            _verticalVelocity += gravity * Time.deltaTime;
        }

        // Combine horizontal and vertical
        Vector3 motion = new Vector3(
            moveDir.x * moveSpeed,
            _verticalVelocity,
            moveDir.z * moveSpeed
        );

        _controller.Move(motion * Time.deltaTime);

        // --- ANIMATOR PARAMS ---
        _animator.SetFloat(Horizontal, _lastMoveDir.x);
        _animator.SetFloat(Vertical, _lastMoveDir.y);

        float speed = moveDir.magnitude;
        _animator.SetFloat(SpeedParam, speed);
        _animator.SetBool(WalkingParam, speed > 0.1f);
    }
}
