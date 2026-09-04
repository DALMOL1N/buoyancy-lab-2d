using Unity.VisualScripting;
using UnityEngine;

public class Patroll : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField]
    float speed;

    [Space(10), Header("Movement Settings")]
    [SerializeField] bool movingRight = false;
    [SerializeField] Vector2 offset;
    [SerializeField, Range(0, 3), Tooltip("Distância para o chão")] 
    float groundCheckDistance = -1;
    [SerializeField] LayerMask groudLayer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        float direction; // = (movingRight) ? 1 : -1;

        if (movingRight)
            direction = 1;
        else
            direction = -1;

        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);

        if (!HasGroundAhead() || HasWallAhead())
        {
            movingRight = !movingRight;
            offset.x *= -1;
        }
    }

    bool HasGroundAhead()
    {
        Vector2 origin = (Vector2)transform.position + offset;

        return Physics2D.Raycast(
            origin, // começa aqui
            Vector2.down, // vai para esta direção
            1, // até está distância
            groudLayer); // enquanto estiver tocando nesta camada
    }

    bool HasWallAhead()
    {
        return Physics2D.Raycast(
            transform.position, // começa aqui
            new Vector2(offset.x, 0), // vai para esta direção
            1, // até está distância
            groudLayer); // enquanto estiver tocando nesta camada
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        // origem , destino
        Gizmos.DrawLine((Vector2) transform.position + offset, 
            (Vector2) transform.position + new Vector2(offset.x, -groundCheckDistance));

        Gizmos.color = Color.black;
        Gizmos.DrawLine(transform.position, 
            (Vector2) transform.position + new Vector2(offset.x, 0));
    }
}
