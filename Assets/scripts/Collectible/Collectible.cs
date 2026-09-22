using CrystalFlux.Core;
using CrystalFlux.WaveSystem;
using TMPro;
using UnityEngine;

namespace CrystalFlux.CollectibleSystem
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Collectible : MonoBehaviour, IPoolable
    {
        [Header("References")]
        public Transform visualRoot;
        public SpriteRenderer sr;
        public SpriteRenderer glow;
        public TextMeshPro descText;
        public CircleCollider2D col;

        [Header("Glow")]
        public Sprite glowSprite;
        public float glowScaleMult = 2.5f;
        [Range(0f, 1f)] public float glowMinAlpha = 0.25f;
        [Range(0f, 1f)] public float glowMaxAlpha = 0.6f;
        public float glowPulseSpeed = 2f;

        [Header("Motion")]
        public float bobAmp = 0.15f;
        public float bobSpeed = 2f;
        public float fadeTime = 0.5f;

        private CollectibleData data;
        private int value;
        private float elapsed;
        private float bobPhase;
        private float glowPhase;
        private bool collected;

        public CollectibleData Data => data;

        public void Setup(CollectibleData d, int v)
        {
            data = d;
            value = v;
            elapsed = 0f;
            collected = false;
            bobPhase = Random.Range(0f, Mathf.PI * 2f);
            glowPhase = Random.Range(0f, Mathf.PI * 2f);

            if (col == null) TryGetComponent(out col);
            if (col != null)
            {
                col.isTrigger = true;
                col.enabled = true;
            }

            if (visualRoot != null) visualRoot.localPosition = Vector3.zero;

            if (sr != null)
            {
                sr.sprite = d != null ? d.sprite : null;
                sr.color = Color.white;
            }

            if (glow != null)
            {
                glow.sprite = glowSprite != null ? glowSprite : (d != null ? d.sprite : null);
                glow.transform.localScale = Vector3.one * glowScaleMult;
                SetGlowAlpha(glowMinAlpha);
            }

            if (descText != null)
            {
                descText.text = d != null ? d.BuildDesc(v) : string.Empty;
                descText.color = d != null ? d.lightColor : Color.white;
            }
        }

        private void Update()
        {
            if (Time.timeScale == 0f) return;

            float dt = Time.deltaTime;

            bobPhase += dt * bobSpeed;
            glowPhase += dt * glowPulseSpeed;

            if (visualRoot != null)
            {
                Vector3 lp = visualRoot.localPosition;
                lp.y = Mathf.Sin(bobPhase) * bobAmp;
                visualRoot.localPosition = lp;
            }

            float pulse = (Mathf.Sin(glowPhase) + 1f) * 0.5f;
            float glowAlpha = Mathf.Lerp(glowMinAlpha, glowMaxAlpha, pulse);

            if (collected || data == null) return;

            if (WaveManager.WaveActive) elapsed += dt;

            float maxTime = data.maxTime;

            if (maxTime > 0f)
            {
                float remaining = maxTime - elapsed;

                if (remaining <= 0f)
                {
                    ReleaseSelf();
                    return;
                }

                if (remaining < fadeTime)
                {
                    float f = remaining / fadeTime;
                    SetAlpha(f);
                    glowAlpha *= f;
                }
            }

            SetGlowAlpha(glowAlpha);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || data == null || other == null) return;

            GameObject p = other.attachedRigidbody != null ? other.attachedRigidbody.gameObject : other.gameObject;
            if (!p.CompareTag("Player")) return;

            collected = true;
            if (col != null) col.enabled = false;

            data.Apply(p, value);
            SpawnIndicator();
            ReleaseSelf();
        }

        private void SpawnIndicator()
        {
            TextIndicatorSpawner tis = TextIndicatorSpawner.Instance;
            if (tis == null) return;

            tis.SpawnTextIndicator(data.BuildDesc(value), transform.position, data.lightColor, 0.8f, 0.9f, 1f);
        }

        private void SetAlpha(float a)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = a;
                sr.color = c;
            }

            if (descText != null)
            {
                Color tc = descText.color;
                tc.a = a;
                descText.color = tc;
            }
        }

        private void SetGlowAlpha(float a)
        {
            if (glow == null || data == null) return;

            Color c = data.lightColor;
            c.a = a;
            glow.color = c;
        }

        private void ReleaseSelf()
        {
            CollectibleSpawner cs = CollectibleSpawner.Active;

            if (cs != null)
            {
                cs.ReleaseCollectible(this);
                return;
            }

            Collectible self = this;
            PrefabPool.Release(ref self);
        }

        public void OnPoolAcquire() { }

        public void OnPoolRelease()
        {
            data = null;
            value = 0;
            collected = false;
            if (col != null) col.enabled = false;
        }
    }
}
