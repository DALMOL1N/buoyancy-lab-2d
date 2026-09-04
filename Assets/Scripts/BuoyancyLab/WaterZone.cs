using UnityEngine;

public sealed class WaterZone : MonoBehaviour
{
    public static WaterZone Instance { get; private set; }

    [SerializeField] float surfaceY;
    [SerializeField] Vector2 waterCenter;
    [SerializeField] Vector2 waterSize;

    public float SurfaceY => surfaceY;
    public Bounds Bounds => new Bounds(waterCenter, waterSize);

    void Awake() => Instance = this;

    public void Configure(Vector2 center, Vector2 size, float surfaceY)
    {
        Instance = this;
        this.surfaceY = surfaceY;
        waterCenter = center;
        waterSize = size;

        var trigger = gameObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        // O objeto visual já está escalado para o tamanho da água.
        // Um collider unitário evita multiplicar o tamanho duas vezes.
        trigger.size = Vector2.one;
    }

    public float GetSubmersion(Collider2D bodyCollider)
    {
        Bounds bounds = Bounds;
        if (bodyCollider == null || bodyCollider.bounds.max.x < bounds.min.x || bodyCollider.bounds.min.x > bounds.max.x)
            return 0f;

        Bounds body = bodyCollider.bounds;
        if (body.min.y >= SurfaceY || body.max.y <= bounds.min.y)
            return 0f;

        return Mathf.Clamp01((SurfaceY - body.min.y) / Mathf.Max(0.05f, body.size.y));
    }
}
