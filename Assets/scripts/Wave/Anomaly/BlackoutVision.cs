using UnityEngine;

public class BlackoutVision : MonoBehaviour
{
    public static BlackoutVision Current { get; private set; }

    private const int MaskTexSize = 128;
    private const float EdgeStart = 0.65f;
    private const float MaskFrac = 0.85f;
    private const float FarExtent = 2000f;
    private const int Segments = 64;
    private const int GradientRings = 10;
    private const int SortingOrder = -1;
    private const float BonusDuration = 1.5f;
    private const float RadiusLerpSpeed = 10f;

    private static Sprite maskSprite;
    private static Material spriteMaterial;

    private Transform player;
    private Transform shade;
    private Transform mask;
    private float baseRadius;
    private float perKill;
    private float maxBonus;
    private float bonus;
    private float shown;

    public float Radius => shown;
    public float SightRadius => shown * MaskFrac;

    public bool InSight(Vector2 pos)
    {
        Vector2 c = transform.position;
        float r = SightRadius;
        return (pos - c).sqrMagnitude <= r * r;
    }

    public void Setup(float radius, float widenPerKill, float darkness)
    {
        Current = this;

        baseRadius = Mathf.Max(0.5f, radius);
        perKill = Mathf.Max(0f, widenPerKill);
        maxBonus = perKill * 3f;
        bonus = 0f;
        shown = baseRadius;

        EnsureSprites();

        shade = CreateShade(Mathf.Clamp01(darkness)).transform;

        GameObject mgo = new("Mask");
        mgo.transform.SetParent(transform, false);
        SpriteMask sm = mgo.AddComponent<SpriteMask>();
        sm.sprite = maskSprite;
        sm.alphaCutoff = 0.5f;
        mask = mgo.transform;

        ResolvePlayer();
        Follow();
        ApplyRadius();
    }

    public void Widen()
    {
        if (perKill <= 0f) return;
        bonus = Mathf.Min(bonus + perKill, maxBonus);
    }

    private void OnDestroy()
    {
        if (ReferenceEquals(Current, this)) Current = null;
    }

    private void LateUpdate()
    {
        float dt = Time.deltaTime;

        if (bonus > 0f && perKill > 0f) bonus = Mathf.MoveTowards(bonus, 0f, perKill / BonusDuration * dt);
        shown = Mathf.Lerp(shown, baseRadius + bonus, 1f - Mathf.Exp(-RadiusLerpSpeed * dt));

        Follow();
        ApplyRadius();
    }

    private void ResolvePlayer()
    {
        if (player != null) return;

        GameObject p = GameObject.FindWithTag("Player");
        if (p != null) player = p.transform;
    }

    private void Follow()
    {
        ResolvePlayer();
        if (player == null) return;

        Vector3 pos = player.position;
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    private void ApplyRadius()
    {
        float r = shown;

        shade.localScale = new Vector3(r, r, 1f);
        mask.localScale = new Vector3(r * 2f * MaskFrac, r * 2f * MaskFrac, 1f);
    }

    // One continuous mesh (clear centre, fading ring, dark square out to FarExtent) so there are no
    // seams between separate sprites for sub-pixel rounding to open into a visible line.
    private GameObject CreateShade(float darkness)
    {
        GameObject go = new("Shade");
        go.transform.SetParent(transform, false);

        int ringCount = GradientRings + 1;
        int vertCount = 1 + (ringCount + 1) * Segments;
        Vector3[] verts = new Vector3[vertCount];
        Color[] cols = new Color[vertCount];

        verts[0] = Vector3.zero;
        cols[0] = new Color(0f, 0f, 0f, 0f);

        for (int j = 0; j < ringCount; j++)
        {
            float t = j / (float)GradientRings;
            float rad = Mathf.Lerp(EdgeStart, 1f, t);
            float a = Mathf.SmoothStep(0f, 1f, t) * darkness;

            for (int i = 0; i < Segments; i++)
            {
                float ang = i * Mathf.PI * 2f / Segments;
                int k = 1 + j * Segments + i;
                verts[k] = new Vector3(Mathf.Cos(ang) * rad, Mathf.Sin(ang) * rad, 0f);
                cols[k] = new Color(0f, 0f, 0f, a);
            }
        }

        for (int i = 0; i < Segments; i++)
        {
            float ang = i * Mathf.PI * 2f / Segments;
            float cs = Mathf.Cos(ang);
            float sn = Mathf.Sin(ang);
            float m = Mathf.Max(Mathf.Abs(cs), Mathf.Abs(sn));
            int k = 1 + ringCount * Segments + i;
            verts[k] = new Vector3(cs / m * FarExtent, sn / m * FarExtent, 0f);
            cols[k] = new Color(0f, 0f, 0f, darkness);
        }

        int[] tris = new int[(Segments + ringCount * Segments * 2) * 3];
        int n = 0;

        for (int i = 0; i < Segments; i++)
        {
            tris[n++] = 0;
            tris[n++] = 1 + i;
            tris[n++] = 1 + (i + 1) % Segments;
        }

        for (int j = 0; j < ringCount; j++)
            for (int i = 0; i < Segments; i++)
            {
                int i2 = (i + 1) % Segments;
                int a0 = 1 + j * Segments + i;
                int a1 = 1 + j * Segments + i2;
                int b0 = 1 + (j + 1) * Segments + i;
                int b1 = 1 + (j + 1) * Segments + i2;

                tris[n++] = a0;
                tris[n++] = b0;
                tris[n++] = a1;
                tris[n++] = a1;
                tris[n++] = b0;
                tris[n++] = b1;
            }

        Mesh mesh = new() { name = "BlackoutShade", hideFlags = HideFlags.HideAndDontSave };
        mesh.vertices = verts;
        mesh.colors = cols;
        mesh.triangles = tris;
        mesh.bounds = new Bounds(Vector3.zero, new Vector3(FarExtent * 2f, FarExtent * 2f, 1f));

        go.AddComponent<MeshFilter>().sharedMesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = GetSpriteMaterial();
        mr.sortingOrder = SortingOrder;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;

        return go;
    }

    private static Material GetSpriteMaterial()
    {
        if (spriteMaterial != null) return spriteMaterial;

        GameObject tmp = new("BlackoutMatProbe");
        spriteMaterial = tmp.AddComponent<SpriteRenderer>().sharedMaterial;
        Destroy(tmp);
        return spriteMaterial;
    }

    private static void EnsureSprites()
    {
        if (maskSprite == null) maskSprite = BuildRadialSprite(MaskTexSize, d => d <= 1f ? 1f : 0f);
    }

    private static Sprite BuildRadialSprite(int size, System.Func<float, float> alphaAt)
    {
        Texture2D t = new(size, size, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        Color[] px = new Color[size * size];
        float half = size * 0.5f;

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - half) / half;
                float dy = (y + 0.5f - half) / half;
                px[y * size + x] = new Color(1f, 1f, 1f, alphaAt(Mathf.Sqrt(dx * dx + dy * dy)));
            }

        t.SetPixels(px);
        t.Apply();

        Sprite s = Sprite.Create(t, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        s.hideFlags = HideFlags.HideAndDontSave;
        return s;
    }
}
