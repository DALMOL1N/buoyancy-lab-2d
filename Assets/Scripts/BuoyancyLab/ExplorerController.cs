using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class ExplorerController : MonoBehaviour
{
    public float moveSpeed = 6.2f;
    public float acceleration = 36f;
    public float jumpImpulse = 9.2f;
    public float swimImpulse = 4.5f;

    Rigidbody2D body;
    Collider2D bodyCollider;
    SpriteRenderer spriteRenderer;
    Sprite[] frames;
    Vector3 spawnPoint;
    float animationClock;
    DensityItem nearbyItem;

    public void Configure(Sprite[] animationFrames)
    {
        frames = animationFrames;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (frames != null && frames.Length > 0)
            spriteRenderer.sprite = frames[0];
    }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPoint = transform.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (transform.position.y < -7f)
            Respawn();

        UpdateInteraction();
        Animate();
    }

    void FixedUpdate()
    {
        float input = Input.GetAxisRaw("Horizontal");
        float targetSpeed = input * moveSpeed;
        body.linearVelocity = new Vector2(
            Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime),
            body.linearVelocity.y);

        if (Mathf.Abs(input) > 0.01f)
            spriteRenderer.flipX = input < 0f;

        // Permite continuar nadando quando só os pés/corpo inferior estão na água,
        // condição comum ao tentar subir de volta para a margem.
        bool inWater = WaterZone.Instance != null && WaterZone.Instance.GetSubmersion(bodyCollider) > 0.08f;
        bool grounded = Physics2D.Raycast(transform.position, Vector2.down, 1.05f, RuntimeGameBootstrap.GroundMask);
        if (Input.GetKey(KeyCode.Space) && inWater && body.linearVelocity.y < 4.8f)
            body.AddForce(Vector2.up * swimImpulse * 1.65f, ForceMode2D.Force);
        else if (Input.GetKeyDown(KeyCode.Space) && grounded)
            body.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);
    }

    void UpdateInteraction()
    {
        nearbyItem = null;
        float nearestDistance = float.MaxValue;
        foreach (Collider2D hit in Physics2D.OverlapCircleAll(transform.position, 1.75f))
        {
            DensityItem item = hit.GetComponent<DensityItem>();
            if (item == null || item.IsCollected) continue;
            float distance = (item.transform.position - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearbyItem = item;
            }
        }

        if (GameHUD.Instance != null)
        {
            if (nearbyItem == null)
                GameHUD.Instance.SetInteraction(string.Empty);
            else if (nearbyItem.CanCollect)
                GameHUD.Instance.SetInteraction($"PRESSIONE E PARA PEGAR: {nearbyItem.DisplayName.ToUpperInvariant()}");
            else
                GameHUD.Instance.SetInteraction("PRIMEIRO EMPURRE ESTE OBJETO PARA A ÁGUA");
        }

        if (nearbyItem != null && nearbyItem.CanCollect && Input.GetKeyDown(KeyCode.E))
            nearbyItem.Collect(transform);
    }

    void Animate()
    {
        if (frames == null || frames.Length < 8)
            return;

        animationClock += Time.deltaTime;
        bool airborne = Mathf.Abs(body.linearVelocity.y) > 0.7f;
        if (airborne)
            spriteRenderer.sprite = frames[body.linearVelocity.y > 0f ? 6 : 7];
        else if (Mathf.Abs(body.linearVelocity.x) > 0.35f)
            spriteRenderer.sprite = frames[2 + Mathf.FloorToInt(animationClock * 10f) % 4];
        else
            spriteRenderer.sprite = frames[Mathf.FloorToInt(animationClock * 2f) % 2];
    }

    void Respawn()
    {
        transform.SetPositionAndRotation(spawnPoint, Quaternion.identity);
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0f;
    }
}
