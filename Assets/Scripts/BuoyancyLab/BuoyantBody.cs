using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class BuoyantBody : MonoBehaviour
{
    [Tooltip("Densidade relativa à água. Menor que 1 boia; maior que 1 afunda.")]
    public float density = 1f;
    public float waterDrag = 3.5f;
    public float waterAngularDrag = 2f;

    public float Submersion { get; private set; }

    Rigidbody2D body;
    Collider2D bodyCollider;
    float airDrag;
    float airAngularDrag;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        airDrag = body.linearDamping;
        airAngularDrag = body.angularDamping;
    }

    void FixedUpdate()
    {
        WaterZone water = WaterZone.Instance;
        Submersion = water == null ? 0f : water.GetSubmersion(bodyCollider);

        if (Submersion <= 0f)
        {
            body.linearDamping = airDrag;
            body.angularDamping = airAngularDrag;
            return;
        }

        // Arquimedes: o volume deslocado gera uma força proporcional à fração submersa.
        // A densidade relativa define se o peso vence (baú) ou se o empuxo vence (garrafa).
        Vector2 buoyancy = -Physics2D.gravity * body.mass * (Submersion / Mathf.Max(0.08f, density));
        body.AddForce(buoyancy, ForceMode2D.Force);
        body.linearDamping = Mathf.Lerp(airDrag, waterDrag, Submersion);
        body.angularDamping = Mathf.Lerp(airAngularDrag, waterAngularDrag, Submersion);
    }
}
