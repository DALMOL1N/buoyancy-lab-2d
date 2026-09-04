using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BuoyantBody), typeof(Rigidbody2D), typeof(Collider2D))]
public sealed class DensityItem : MonoBehaviour
{
    [SerializeField] string displayName;
    [SerializeField] GameProgress progress;

    public string DisplayName => displayName;
    public bool HasEnteredWater { get; private set; }
    public bool IsCollected { get; private set; }
    public bool CanCollect => HasEnteredWater && !IsCollected;

    BuoyantBody buoyancy;
    Rigidbody2D body;
    Collider2D bodyCollider;

    public void Configure(string displayName, GameProgress gameProgress)
    {
        this.displayName = displayName;
        progress = gameProgress;
    }

    void Awake()
    {
        buoyancy = GetComponent<BuoyantBody>();
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (!HasEnteredWater && buoyancy.Submersion > 0.12f)
            HasEnteredWater = true;
    }

    public void Collect(Transform collector)
    {
        if (!CanCollect) return;
        IsCollected = true;
        body.simulated = false;
        bodyCollider.enabled = false;
        progress.RegisterCollection(DisplayName);
        StartCoroutine(FlyToCollector(collector));
    }

    IEnumerator FlyToCollector(Transform collector)
    {
        Vector3 start = transform.position;
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < 0.38f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / 0.38f);
            Vector3 target = collector == null ? start + Vector3.up : collector.position + Vector3.up * 0.35f;
            transform.position = Vector3.Lerp(start, target, t);
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }
        Destroy(gameObject);
    }
}
