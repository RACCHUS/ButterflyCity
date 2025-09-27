using UnityEngine;
using UnityEngine.InputSystem; // Required for new Input System

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3f; // Units per second
    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // This method will be called by the Player Input component
    public void OnMove(InputValue value)
    {
        movement = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}
