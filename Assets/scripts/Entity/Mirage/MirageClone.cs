using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using CrystalFlux.StatusEffectSystem;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    [RequireComponent(typeof(MirageStatManager))]
    public class MirageClone : MonoBehaviour, IOwnerProxy, IKnockbackable
    {
        private static readonly List<MirageClone> active = new();
        public static IReadOnlyList<MirageClone> Active => active;

        public GameObject Base { get; private set; }
        public GameObject ProxyOwner => Base;
        public int TeamID => msm != null ? msm.TeamID : 0;
        public bool IsAlive => Base != null && (eh == null || eh.IsAlive);

        private MirageStatManager msm;
        private EntityHealth eh;
        private Rigidbody2D rb;
        private SpriteRenderer sr;
        private SpriteRenderer bsr;
        private Transform bt;
        private Vector2 offset;
        private Vector2 anchor;
        private float opacity = 1f;
        private bool followBase;
        private bool copyAttacks;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => active.Clear();

        private void Awake()
        {
            msm = GetComponent<MirageStatManager>();
            eh = GetComponent<EntityHealth>();
            rb = GetComponent<Rigidbody2D>();
            sr = GetComponentInChildren<SpriteRenderer>(true);
            if (!TryGetComponent<Targetable>(out _)) gameObject.AddComponent<Targetable>();
        }

        private void OnEnable() => active.Add(this);
        private void OnDisable() => active.Remove(this);

        public void Setup(GameObject baseObj, Vector2 off, float share, List<StatShare> overrides, float opa, bool follow, bool atk)
        {
            Base = baseObj;
            bt = baseObj != null ? baseObj.transform : null;
            bsr = baseObj != null ? baseObj.GetComponentInChildren<SpriteRenderer>(true) : null;
            offset = off;
            anchor = (Vector2)transform.position;
            opacity = Mathf.Clamp01(opa);
            followBase = follow;
            copyAttacks = atk;

            if (baseObj != null) gameObject.layer = baseObj.layer;

            if (msm != null) msm.Setup(baseObj, share, overrides);

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
                rb.gravityScale = 0f;
                rb.linearVelocity = Vector2.zero;
            }

            SyncVisuals();
        }

        public void ApplyKnockback(Vector2 direction, float force, float duration) {}

        private void LateUpdate()
        {
            if (Time.timeScale == 0f || bt == null) return;

            Vector2 pos = followBase ? (Vector2)bt.position + offset : anchor;
            transform.position = pos;

            if (rb != null)
            {
                rb.position = pos;
                rb.linearVelocity = Vector2.zero;
            }

            SyncVisuals();
        }

        private void SyncVisuals()
        {
            if (sr == null || bsr == null) return;

            sr.sprite = bsr.sprite;
            sr.flipX = bsr.flipX;
            sr.flipY = bsr.flipY;
            sr.sortingLayerID = bsr.sortingLayerID;
            sr.sortingOrder = bsr.sortingOrder;

            Color c = bsr.color;
            c.a *= opacity;
            sr.color = c;

            if (followBase && bt != null) transform.localScale = bt.localScale;
        }

        public static void NotifyCast(GameObject caster, AttackData ad, Vector2 center)
        {
            if (caster == null || ad == null || active.Count == 0) return;

            ProjectileSpawner.ResolveAim(ad, caster, center, null, null, out var dir, out var dist);
            NotifyCast(caster, ad, center, dir, dist);
        }

        public static void NotifyCast(GameObject caster, AttackData ad, Vector2 center, Vector2 dir, float dist)
        {
            if (caster == null || ad == null) return;

            ProjectileSpawner ps = ProjectileSpawner.Instance;
            if (ps == null) return;

            Vector2 rel = center - (Vector2)caster.transform.position;

            for (int i = active.Count - 1; i >= 0; i--)
            {
                MirageClone mc = active[i];
                if (!mc.CanRepeat(caster)) continue;

                Vector2 c = (Vector2)mc.transform.position + rel;
                mc.StartCoroutine(ps.SpawnFromPattern(ad, mc.gameObject, c, dir, dist, null, true));
            }
        }

        public static void NotifyCast(GameObject caster, GameObject prefab, Vector2 center, Vector2 dir, float dist)
        {
            if (caster == null || prefab == null) return;

            ProjectileSpawner ps = ProjectileSpawner.Instance;
            if (ps == null) return;

            Vector2 rel = center - (Vector2)caster.transform.position;

            for (int i = active.Count - 1; i >= 0; i--)
            {
                MirageClone mc = active[i];
                if (!mc.CanRepeat(caster)) continue;

                Vector2 c = (Vector2)mc.transform.position + rel;
                mc.StartCoroutine(ps.SpawnFromPattern(prefab, mc.gameObject, c, dir, dist, null, true));
            }
        }

        private bool CanRepeat(GameObject caster)
            => this != null && copyAttacks && Base == caster && IsAlive && isActiveAndEnabled;

        public static GameObject CreateDefault(GameObject baseObj, Vector2 pos)
        {
            GameObject go = new("Mirage Clone");
            go.transform.SetPositionAndRotation(pos, Quaternion.identity);

            if (baseObj != null)
            {
                go.layer = baseObj.layer;
                go.transform.localScale = baseObj.transform.localScale;
            }

            go.AddComponent<SpriteRenderer>();

            Rigidbody2D r = go.AddComponent<Rigidbody2D>();
            r.bodyType = RigidbodyType2D.Dynamic;
            r.constraints = RigidbodyConstraints2D.FreezeAll;
            r.gravityScale = 0f;

            CopyHurtbox(baseObj, go);

            go.AddComponent<MirageStatManager>();
            go.AddComponent<StatusEffectManager>();
            go.AddComponent<EntityHealth>();
            go.AddComponent<MirageClone>();

            return go;
        }

        private static void CopyHurtbox(GameObject baseObj, GameObject go)
        {
            Collider2D src = baseObj != null ? baseObj.GetComponent<Collider2D>() : null;
            Collider2D dst;

            switch (src)
            {
                case BoxCollider2D b:
                    var nb = go.AddComponent<BoxCollider2D>();
                    nb.size = b.size;
                    dst = nb;
                    break;
                case CapsuleCollider2D cp:
                    var nc = go.AddComponent<CapsuleCollider2D>();
                    nc.size = cp.size;
                    nc.direction = cp.direction;
                    dst = nc;
                    break;
                case CircleCollider2D ci:
                    var nci = go.AddComponent<CircleCollider2D>();
                    nci.radius = ci.radius;
                    dst = nci;
                    break;
                default:
                    dst = go.AddComponent<CircleCollider2D>();
                    break;
            }

            if (src != null) dst.offset = src.offset;
            dst.isTrigger = true;
        }
    }
}
