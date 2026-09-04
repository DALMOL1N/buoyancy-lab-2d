using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class ExplorerController : MonoBehaviour
{
    public float moveSpeed = 6.2f;
    public float acceleration = 36f;
    public float jumpImpulse = 9.2f;
    public float swimSpeed = 4.8f;
    public float swimAcceleration = 28f;

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
        float nextX = Mathf.MoveTowards(body.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);

        if (Mathf.Abs(input) > 0.01f)
            spriteRenderer.flipX = input < 0f;

        float submersion = WaterZone.Instance == null ? 0f : WaterZone.Instance.GetSubmersion(bodyCollider);
        // Mesmo um contato pequeno com a água mantém o controle de natação ativo.
        // Isso dá força suficiente para vencer o último degrau da margem.
        bool inWater = submersion > 0.02f;
        if (inWater)
        {
            float verticalInput = Input.GetAxisRaw("Vertical");
            if (Input.GetKey(KeyCode.Space)) verticalInput = 1f;

            float targetY = verticalInput * swimSpeed;
            float nextY = Mathf.MoveTowards(body.linearVelocity.y, targetY, swimAcceleration * Time.fixedDeltaTime);
            body.linearVelocity = new Vector2(nextX, nextY);
            return;
        }

        body.linearVelocity = new Vector2(nextX, body.linearVelocity.y);
        bool grounded = Physics2D.Raycast(transform.position, Vector2.down, 1.05f, RuntimeGameBootstrap.GroundMask);
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
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
