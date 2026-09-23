using CrystalFlux.Core;
using CrystalFlux.ProjectileSystem;
using UnityEngine;

namespace CrystalFlux.EntitySystem
{
    public class RushState
    {
        private readonly GameObject go;
        private readonly IStatProvider esm;
        private readonly System.Action onStart;
        private readonly System.Action onEnd;
        private float progress;
        private bool immuneHeld;

        public AttackData Ad { get; private set; }
        public Vector2 Dir { get; private set; }
        public bool Active => Ad != null;
        public bool BlocksAttacks => Ad != null && Ad.DisableAttacksWhileRushing;
        public bool Unstoppable => Ad != null && Resist >= 2f;
        public float Speed => Ad == null ? 0f : GetSpeed(Ad, esm);
        private float Resist => esm == null ? 0f : esm.GetStat(StatType.interruptResist);

        public RushState(GameObject go, IStatProvider esm, System.Action onStart = null, System.Action onEnd = null)
        {
            this.go = go;
            this.esm = esm;
            this.onStart = onStart;
            this.onEnd = onEnd;
        }

        public bool Begin(AttackData ad, Vector2 origin, Vector2 aim, Vector2 fallback)
        {
            if (ad == null || !ad.Rushes || esm == null) return false;

            End();

            Ad = ad;
            progress = 0f;
            Dir = GetDir(ad, origin, aim, fallback);

            if (ad.RushType == RushType.Distance && Speed <= 0f)
            {
                Ad = null;
                return false;
            }

            if (ad.ImmuneWhileRushing)
            {
                immuneHeld = true;
                esm.AddStat(new StatBuff(StatType.isImmune, 1f));
            }

            onStart?.Invoke();
            return true;
        }

        public Vector2 GetVelocity(Vector2 steer, float dt)
        {
            if (Ad == null) return Vector2.zero;

            if (Ad.OmnidirectionalRush && steer.sqrMagnitude > 0.0001f)
            {
                Vector2 tgt = steer.normalized;
                if (Ad.RushTurnRate <= 0f) Dir = tgt;
                else
                {
                    float cur = Mathf.Atan2(Dir.y, Dir.x) * Mathf.Rad2Deg;
                    float to = Mathf.Atan2(tgt.y, tgt.x) * Mathf.Rad2Deg;
                    float rad = Mathf.MoveTowardsAngle(cur, to, Ad.RushTurnRate * dt) * Mathf.Deg2Rad;
                    Dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                }
            }

            return Dir * Speed;
        }

        public void Tick(float dt, bool moving)
        {
            if (Ad == null) return;

            if (Ad.RushType == RushType.Duration) progress += dt;
            else if (moving)
            {
                float spd = Speed;
                if (spd <= 0f)
                {
                    End();
                    return;
                }
                progress += spd * dt;
            }

            if (progress >= Ad.RushTypeVal) End();
        }

        public void OnImmobilized()
        {
            if (Ad != null && Resist < 1f) End();
        }

        public bool OnKnockback()
        {
            if (Ad == null) return false;

            float ir = Resist;
            if (ir >= 2f) return true;
            if (ir < 1f) End();
            return false;
        }

        public void OnCollision(Collision2D c)
        {
            if (Ad == null || c == null) return;

            if (Ad.EndRushOnCollision)
            {
                End();
                return;
            }

            if (Ad.RushKnockback && Ad.RushKnockbackForce > 0f) TryKnockback(c);

            if (Ad != null && Ad.BounceOnCollision && c.contactCount > 0)
            {
                Vector2 n = c.GetContact(0).normal;
                if (Vector2.Dot(Dir, n) < 0f) Dir = Vector2.Reflect(Dir, n).normalized;
            }
        }

        private void TryKnockback(Collision2D c)
        {
            GameObject other = c.rigidbody != null ? c.rigidbody.gameObject : c.gameObject;
            if (other == null || other == go || !other.TryGetComponent<IKnockbackable>(out var kb)) return;

            if (go.TryGetComponent<ITeamMember>(out var own) && other.TryGetComponent<ITeamMember>(out var otm) && own.TeamID == otm.TeamID) return;

            Vector2 d = other.transform.position - go.transform.position;
            if (d.sqrMagnitude < 0.0001f) d = Dir;

            kb.ApplyKnockback(d.normalized, Ad.RushKnockbackForce, Ad.RushKnockbackTime);
        }

        public void End()
        {
            if (Ad == null) return;

            Ad = null;
            progress = 0f;

            if (immuneHeld)
            {
                immuneHeld = false;
                esm?.AddStat(new StatBuff(StatType.isImmune, -1f));
            }

            onEnd?.Invoke();
        }

        public static float GetSpeed(AttackData ad, IStatProvider esm)
            => ad == null || esm == null ? 0f : esm.GetStat(StatType.EffSpd) * ad.RushSpeedMult;

        public static Vector2 GetDir(AttackData ad, Vector2 origin, Vector2 aim, Vector2 fallback)
        {
            Vector2 toAim = aim - origin;
            Vector2 aimDir = toAim.sqrMagnitude > 0.0001f ? toAim.normalized : fallback.normalized;

            switch (ad.RushDirection)
            {
                case RushDirection.WorldAngle:
                    float rad = ad.RushAngle * Mathf.Deg2Rad;
                    return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                case RushDirection.CursorRelativeAngle:
                    return Quaternion.Euler(0f, 0f, ad.RushAngle) * aimDir;
                default:
                    return aimDir;
            }
        }
    }
}
