using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody2D rb;
   

    private bool _isFacingRight = true;

    [Header("Movement Settings")]
    [SerializeField]
    private float horizontalSpeed;
    [SerializeField]
    private float jumpForce;
    private bool _isGrounded;
    
    [Space(10), Header("Ground Check Settings")]
    [SerializeField]
    LayerMask groundLayer;
    [SerializeField]
    float groundCheckDistance;


    private float horizontalInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (horizontalInput > 0) _isFacingRight = true;
        else if (horizontalInput < 0) _isFacingRight = false;

        if (_isFacingRight)
            transform.eulerAngles = new Vector2(0, 0);
        else
            transform.eulerAngles = new Vector2(0, 180);

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);


     
    }

    private void FixedUpdate()
    {
        float x = ProcessHorizontalVelocity();

        rb.linearVelocity = new Vector2(x, rb.linearVelocity.y);

        IsGrounded();
    }


    float ProcessHorizontalVelocity()
    {
        return horizontalInput * horizontalSpeed;
    }

    void IsGrounded()
    {
        _isGrounded = Physics2D.Raycast(transform.position, 
            Vector2.down, groundCheckDistance, 
            groundLayer);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector2 to = (Vector2) transform.position + Vector2.down * groundCheckDistance;
        Gizmos.DrawLine(transform.position, to);
    }
}
