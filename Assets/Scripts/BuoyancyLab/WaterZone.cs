using UnityEngine;

public sealed class WaterZone : MonoBehaviour
{
    public static WaterZone Instance { get; private set; }

    public float SurfaceY { get; private set; }
    public Bounds Bounds { get; private set; }

    public void Configure(Vector2 center, Vector2 size, float surfaceY)
    {
        Instance = this;
        SurfaceY = surfaceY;
        Bounds = new Bounds(center, size);

        var trigger = gameObject.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true;
        // O objeto visual já está escalado para o tamanho da água.
        // Um collider unitário evita multiplicar o tamanho duas vezes.
        trigger.size = Vector2.one;
    }

    public float GetSubmersion(Collider2D bodyCollider)
    {
        if (bodyCollider == null || bodyCollider.bounds.max.x < Bounds.min.x || bodyCollider.bounds.min.x > Bounds.max.x)
            return 0f;

        Bounds body = bodyCollider.bounds;
        if (body.min.y >= SurfaceY || body.max.y <= Bounds.min.y)
            return 0f;

        return Mathf.Clamp01((SurfaceY - body.min.y) / Mathf.Max(0.05f, body.size.y));
    }
}
