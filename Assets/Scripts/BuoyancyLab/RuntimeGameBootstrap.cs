using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class RuntimeGameBootstrap : MonoBehaviour
{
    public const int GroundLayer = 8;
    public static int GroundMask => 1 << GroundLayer;

    static Sprite whiteSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartGame()
    {
        if (Object.FindFirstObjectByType<RuntimeGameBootstrap>() != null)
            return;

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            root.SetActive(false);

        new GameObject("Buoyancy Lab - Runtime").AddComponent<RuntimeGameBootstrap>().Build();
    }

    void Build()
    {
        Physics2D.gravity = new Vector2(0f, -12f);
        whiteSprite = MakeWhiteSprite();

        Camera cam = CreateCamera();
        CreateBackground(cam);
        CreateWorld();
        CreateWater();

        Texture2D explorerAtlas = Resources.Load<Texture2D>("Art/ExplorerSheet");
        Texture2D propAtlas = Resources.Load<Texture2D>("Art/DensityProps");
        explorerAtlas.filterMode = FilterMode.Point;
        explorerAtlas.wrapMode = TextureWrapMode.Clamp;
        propAtlas.filterMode = FilterMode.Point;
        propAtlas.wrapMode = TextureWrapMode.Clamp;
        Sprite[] explorerFrames = SliceHorizontal(explorerAtlas, 8, 260f);
        Sprite[] propSprites = SliceHorizontal(propAtlas, 3, 250f);

        GameHUD hud = gameObject.AddComponent<GameHUD>();
        GameProgress progress = gameObject.AddComponent<GameProgress>();
        progress.Configure(hud, 3);
        CreatePlayer(explorerFrames);
        CreateProp("Baú Pesado", new Vector2(-3.1f, 1.15f), new Vector2(1.45f, 1.25f), propSprites[0], 5.2f, 2.35f, 0.85f, hud, progress,
            "BAÚ", "densidade 2,35 • afunda", new Color(0.82f, 0.43f, 0.12f));
        CreateProp("Barril Equilibrado", new Vector2(-4.8f, 1.1f), new Vector2(1.15f, 1.35f), propSprites[1], 2.2f, 0.92f, 0.72f, hud, progress,
            "BARRIL", "densidade 0,92 • quase neutro", new Color(0.2f, 0.68f, 0.74f));
        CreateProp("Garrafa Leve", new Vector2(-7.0f, 0.9f), new Vector2(0.65f, 1.05f), propSprites[2], 0.45f, 0.22f, 0.42f, hud, progress,
            "GARRAFA", "densidade 0,22 • boia", new Color(0.22f, 0.88f, 1f));

        CreateFireflies();
    }

    Camera CreateCamera()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        Camera cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 7.5f;
        cam.backgroundColor = new Color(0.005f, 0.015f, 0.055f);
        cam.transform.position = new Vector3(0f, 0.4f, -10f);
        go.AddComponent<AudioListener>();
        return cam;
    }

    void CreateBackground(Camera cam)
    {
        Texture2D texture = Resources.Load<Texture2D>("Art/MysticLake");
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
        GameObject go = new GameObject("Mystic Lake Backdrop");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -30;
        go.transform.position = new Vector3(0f, 0.5f, 8f);
        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * (16f / 9f);
        float scale = Mathf.Max(worldWidth / sprite.bounds.size.x, worldHeight / sprite.bounds.size.y);
        go.transform.localScale = Vector3.one * scale;
    }

    void CreateWorld()
    {
        CreateSolid("Margem esquerda", new Vector2(-8f, -1.55f), new Vector2(10f, 3f), new Color(0.035f, 0.09f, 0.15f), 0);
        CreateSolid("Fundo do lago", new Vector2(3f, -5.45f), new Vector2(12f, 1f), new Color(0.025f, 0.055f, 0.11f), 0);
        CreateSolid("Margem direita", new Vector2(11.5f, -1.7f), new Vector2(5f, 3.4f), new Color(0.035f, 0.09f, 0.15f), 0);
        CreateSolid("Pedra submersa", new Vector2(7.8f, -3.65f), new Vector2(2.6f, 0.55f), new Color(0.05f, 0.14f, 0.2f), -8f);

        // Escadaria submersa que permite sair do lago pelo lado esquerdo.
        CreateSolid("Degrau da margem 1", new Vector2(-2.65f, -0.32f), new Vector2(0.9f, 0.48f), new Color(0.04f, 0.16f, 0.21f), 0f);
        CreateSolid("Degrau da margem 2", new Vector2(-2.05f, -0.82f), new Vector2(0.9f, 0.48f), new Color(0.04f, 0.15f, 0.21f), 0f);
        CreateSolid("Degrau da margem 3", new Vector2(-1.45f, -1.32f), new Vector2(0.9f, 0.48f), new Color(0.035f, 0.14f, 0.2f), 0f);
        CreateSolid("Degrau da margem 4", new Vector2(-0.85f, -1.82f), new Vector2(0.9f, 0.48f), new Color(0.03f, 0.13f, 0.19f), 0f);

        for (int i = 0; i < 15; i++)
        {
            float x = -12f + i * 1.25f;
            CreateDecor("Musgo", new Vector2(x, 0.02f + Mathf.Sin(i) * 0.04f), new Vector2(1.3f, 0.11f), new Color(0.05f, 0.7f, 0.64f, 0.8f), -4);
        }
    }

    void CreateWater()
    {
        const float surfaceY = -0.15f;
        Vector2 size = new Vector2(13.5f, 5.2f);
        Vector2 center = new Vector2(4.75f, surfaceY - size.y * 0.5f);
        GameObject water = new GameObject("Água");
        water.transform.position = center;
        SpriteRenderer sr = water.AddComponent<SpriteRenderer>();
        sr.sprite = whiteSprite;
        sr.color = new Color(0.02f, 0.58f, 0.78f, 0.27f);
        sr.sortingOrder = -2;
        water.transform.localScale = size;
        water.AddComponent<WaterZone>().Configure(center, size, surfaceY);

        CreateDecor("Linha luminosa da água", new Vector2(center.x, surfaceY), new Vector2(size.x, 0.075f), new Color(0.15f, 0.96f, 1f, 0.9f), 4);
        CreateDecor("Brilho da superfície", new Vector2(center.x, surfaceY - 0.1f), new Vector2(size.x, 0.18f), new Color(0.08f, 0.76f, 1f, 0.18f), 3);
    }

    void CreatePlayer(Sprite[] frames)
    {
        GameObject player = new GameObject("Exploradora");
        player.transform.position = new Vector2(-10.4f, 0.85f);
        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = frames[0];
        sr.sortingOrder = 12;
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.mass = 1.1f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        CapsuleCollider2D col = player.AddComponent<CapsuleCollider2D>();
        col.size = new Vector2(0.72f, 1.65f);
        col.offset = new Vector2(0f, -0.03f);
        player.AddComponent<BuoyantBody>().density = 0.78f;
        player.AddComponent<ExplorerController>().Configure(frames);
    }

    void CreateProp(string objectName, Vector2 position, Vector2 colliderSize, Sprite sprite, float mass, float density, float scale, GameHUD hud, GameProgress progress, string title, string subtitle, Color labelColor)
    {
        GameObject prop = new GameObject(objectName);
        prop.transform.position = position;
        prop.transform.localScale = Vector3.one * scale;
        SpriteRenderer sr = prop.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 10;
        Rigidbody2D rb = prop.AddComponent<Rigidbody2D>();
        rb.mass = mass;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        BoxCollider2D col = prop.AddComponent<BoxCollider2D>();
        col.size = colliderSize / scale;
        PhysicsMaterial2D material = new PhysicsMaterial2D(objectName + " Material") { friction = 0.42f, bounciness = 0.04f };
        col.sharedMaterial = material;
        BuoyantBody buoyancy = prop.AddComponent<BuoyantBody>();
        buoyancy.density = density;
        buoyancy.waterDrag = density < 0.5f ? 5f : 2.4f;
        buoyancy.waterAngularDrag = density < 0.5f ? 4f : 1.4f;
        prop.AddComponent<DensityItem>().Configure(title, progress);
        hud.AddItem(prop.transform, title, subtitle, labelColor);
    }

    void CreateSolid(string objectName, Vector2 position, Vector2 size, Color color, float angle)
    {
        GameObject go = CreateDecor(objectName, position, size, color, 1);
        go.layer = GroundLayer;
        go.transform.rotation = Quaternion.Euler(0f, 0f, angle);
        go.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    GameObject CreateDecor(string objectName, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        GameObject go = new GameObject(objectName);
        go.transform.position = position;
        go.transform.localScale = size;
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = whiteSprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
        return go;
    }

    void CreateFireflies()
    {
        Random.InitState(1927);
        for (int i = 0; i < 26; i++)
        {
            float x = Random.Range(-12f, 13f);
            float y = Random.Range(0.7f, 5.8f);
            float s = Random.Range(0.025f, 0.07f);
            GameObject firefly = CreateDecor("Vagalume", new Vector2(x, y), new Vector2(s, s), new Color(1f, 0.7f, 0.12f, 0.9f), 2);
            firefly.AddComponent<FireflyMotion>().phase = Random.Range(0f, 6f);
        }
    }

    static Sprite MakeWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.name = "Runtime White Pixel";
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    static Sprite[] SliceHorizontal(Texture2D texture, int count, float pixelsPerUnit)
    {
        Sprite[] result = new Sprite[count];
        for (int i = 0; i < count; i++)
        {
            int x0 = Mathf.RoundToInt(i * texture.width / (float)count);
            int x1 = Mathf.RoundToInt((i + 1) * texture.width / (float)count);
            result[i] = Sprite.Create(texture, new Rect(x0, 0, x1 - x0, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
        return result;
    }
}

public sealed class FireflyMotion : MonoBehaviour
{
    public float phase;
    Vector3 origin;

    void Start() => origin = transform.position;

    void Update()
    {
        float t = Time.time + phase;
        transform.position = origin + new Vector3(Mathf.Sin(t * 0.8f) * 0.16f, Mathf.Sin(t * 1.4f) * 0.1f, 0f);
        transform.localScale = Vector3.one * (0.04f + Mathf.Sin(t * 2f) * 0.012f);
    }
}
