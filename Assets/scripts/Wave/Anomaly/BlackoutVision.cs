using UnityEngine;

public class BlackoutVision : MonoBehaviour
{
    public static BlackoutVision Current { get; private set; }

    private const int DiscTexSize = 256;
    private const int MaskTexSize = 128;
    private const float EdgeStart = 0.65f;
    private const float MaskFrac = 0.85f;
    private const float SideSize = 400f;
    private const int SortingOrder = -1;
    private const float BonusDuration = 1.5f;
    private const float RadiusLerpSpeed = 10f;

    private static Sprite discSprite;
    private static Sprite solidSprite;
    private static Sprite maskSprite;

    private Transform player;
    private Transform disc;
    private Transform mask;
    private readonly Transform[] sides = new Transform[4];
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

        Color c = new(0f, 0f, 0f, Mathf.Clamp01(darkness));

        disc = CreatePart("Disc", discSprite, c).transform;
        for (int i = 0; i < sides.Length; i++) sides[i] = CreatePart($"Side{i}", solidSprite, c).transform;

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
        float d = r * 2f;

        disc.localScale = new Vector3(d, d, 1f);
        mask.localScale = new Vector3(d * MaskFrac, d * MaskFrac, 1f);

        float off = r + SideSize * 0.5f;
        float wide = d + SideSize * 2f;

        sides[0].localPosition = new Vector3(0f, off, 0f);
        sides[0].localScale = new Vector3(wide, SideSize, 1f);
        sides[1].localPosition = new Vector3(0f, -off, 0f);
        sides[1].localScale = new Vector3(wide, SideSize, 1f);
        sides[2].localPosition = new Vector3(-off, 0f, 0f);
        sides[2].localScale = new Vector3(SideSize, d, 1f);
        sides[3].localPosition = new Vector3(off, 0f, 0f);
        sides[3].localScale = new Vector3(SideSize, d, 1f);
    }

    private GameObject CreatePart(string n, Sprite s, Color c)
    {
        GameObject go = new(n);
        go.transform.SetParent(transform, false);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.color = c;
        sr.sortingOrder = SortingOrder;

        return go;
    }

    private static void EnsureSprites()
    {
        if (discSprite == null) discSprite = BuildRadialSprite(DiscTexSize, d => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(EdgeStart, 1f, d)));
        if (maskSprite == null) maskSprite = BuildRadialSprite(MaskTexSize, d => d <= 1f ? 1f : 0f);

        if (solidSprite == null)
        {
            Texture2D t = new(4, 4, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            Color[] px = new Color[16];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            t.SetPixels(px);
            t.Apply();
            solidSprite = Sprite.Create(t, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
            solidSprite.hideFlags = HideFlags.HideAndDontSave;
        }
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
