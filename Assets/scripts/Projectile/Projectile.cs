using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using UnityEngine;

namespace CrystalFlux.ProjectileSystem
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(RectTransform))]
    public class Projectile : MonoBehaviour, IPoolable
    {
        public ProjectileData pd;

        [HideInInspector] public GameObject ownerObj;
        public static bool ApplyingProjectileHit { get; private set; }
        [HideInInspector] public Vector2 dir;
        [HideInInspector] public int pierced;
        private float effSpd;
        private float lifeRemaining;
        private List<GameObject> hit;
        private ProjectileDamageSnapshot damageSnapshot;
        private Transform followTarget;
        private Transform orbitTarget;
        private float orbitDirectionSign;
        private float effectiveOrbitRadius;
        private float orbitAngleOffset;
        private bool orbitInitialized;
        private Rigidbody2D rb;
        private bool boomerangActive;
        private bool boomerangReturning;
        private float boomerangDecel;
        private float boomerangSpeed;
        private bool orbitCancelled;
        private bool canTriggerAdd;
        private Rigidbody2D sourceRb;
        private Vector2 patternOrigin;
        private float patternTime;
        private float patternBaseAngle;
        private float spiralTheta;
        private bool patternSuspended;
        private MovementType prefabMoveType;
        private float prefabWaveAmp;
        private float prefabWaveFreq;
        private float prefabSpiralSpacing;
        private ProjectileData defaultPd;
        private Vector3 defaultScale;
        private ProjectileData registeredData;

        private static readonly Dictionary<ProjectileData, int> liveDataRefs = new();

        public static bool IsDataLive(ProjectileData data)
            => data != null && liveDataRefs.TryGetValue(data, out int count) && count > 0;

        private void RegisterData()
        {
            UnregisterData();
            if (pd == null) return;

            liveDataRefs.TryGetValue(pd, out int count);
            liveDataRefs[pd] = count + 1;
            registeredData = pd;
        }

        private void UnregisterData()
        {
            if (ReferenceEquals(registeredData, null)) return;

            if (liveDataRefs.TryGetValue(registeredData, out int count))
            {
                if (count <= 1) liveDataRefs.Remove(registeredData);
                else liveDataRefs[registeredData] = count - 1;
            }
            registeredData = null;
        }

        private bool UsePrefabMove => prefabMoveType != MovementType.Default;
        private MovementType MoveType => UsePrefabMove ? prefabMoveType : pd.movementType;
        private float WaveAmp => UsePrefabMove ? prefabWaveAmp : pd.waveAmplitude;
        private float WaveFreq => UsePrefabMove ? prefabWaveFreq : pd.waveFrequency;
        private float SpiralSpacing => UsePrefabMove ? prefabSpiralSpacing : pd.spiralSpacing;

        private void Awake()
        {
            hit = new();
            canTriggerAdd = true;
            defaultPd = pd;
            defaultScale = transform.localScale;
            CachePrefabMovement();
        }

        private void CachePrefabMovement()
        {
            if (pd == null) { prefabMoveType = MovementType.Default; return; }

            prefabMoveType = pd.movementType;
            prefabWaveAmp = pd.waveAmplitude;
            prefabWaveFreq = pd.waveFrequency;
            prefabSpiralSpacing = pd.spiralSpacing;
        }

        private void OnDestroy()
        {
            UnregisterFromOwner();
            UnregisterData();
        }

        private void UnregisterFromOwner()
        {
            if (pd != null && pd.orbitRadius > 0 && pd.orbitSelf && ownerObj != null &&
                ownerObj.TryGetComponent<IOrbitRegister>(out var ior))
                ior.UnregisterOrbitingProjectile(this);

            if (pd != null && pd.mainAttack != null && pd.mainAttack.canCharge && ownerObj != null &&
                ownerObj.TryGetComponent<IChargeRegister>(out var icr))
                icr.UnregisterChargedProjectile(this);
        }

        private void Despawn()
        {
            GameObject go = gameObject;
            PrefabPool.Release(ref go);
        }

        public void OnPoolAcquire() { }

        public void OnPoolRelease()
        {
            StopAllCoroutines();
            UnregisterFromOwner();
            UnregisterData();

            hit?.Clear();
            ownerObj = null;
            followTarget = null;
            orbitTarget = null;
            sourceRb = null;

            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        public void Setup(Vector2 direction, GameObject owner, ProjectileData pdOverride)
        {
            pd = pdOverride != null ? pdOverride : defaultPd;

            if (pd == null)
            {
                Debug.LogError($"Projectile '{name}' has no ProjectileData assigned.", this);
                Despawn();
                return;
            }

            RegisterData();

            ownerObj = owner;
            dir = direction;

            hit.Clear();
            canTriggerAdd = true;
            pierced = 0;

            orbitCancelled = false;
            orbitInitialized = false;
            orbitTarget = null;
            orbitDirectionSign = 0f;
            orbitAngleOffset = 0f;
            effectiveOrbitRadius = 0f;

            boomerangActive = false;
            boomerangReturning = false;
            boomerangSpeed = 0f;
            boomerangDecel = 0f;

            followTarget = null;
            sourceRb = null;

            patternSuspended = false;
            patternTime = 0f;
            spiralTheta = 0f;

            effSpd = ownerObj != null && ownerObj.TryGetComponent<IStatProvider>(out var esm)
                ? pd.speed * (1f + (esm.GetStat(StatType.ProjSpd) * 0.01f))
                : pd.speed;

            CaptureSnapshot();

            transform.localScale = defaultScale;
            HandleSize();
            HandleDirection();

            rb = GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            InitBoomerang();
            HandleMovement(true);

            if (pd.effects != null && pd.effects.Count > 0)
            {
                foreach (var ed in pd.effects)
                {
                    if (ed.effect != null && ed.applyCondition == ApplyCondition.OnCast && ed.selfApply)
                        ApplyEffect(null, ed);
                }
            }

            if (pd.orbitRadius > 0 && pd.orbitSelf && ownerObj != null &&
                ownerObj.TryGetComponent<IOrbitRegister>(out var iog))
            {
                iog.RegisterOrbitingProjectile(this);
            }

            if (pd.mainAttack != null && pd.mainAttack.canCharge && ownerObj != null &&
                ownerObj.TryGetComponent<IChargeRegister>(out var icr))
            {
                icr.RegisterChargedProjectile(this);
            }

            lifeRemaining = pd.lifetime;
        }

        private void FixedUpdate() => HandleMovement(false);

        private void Update()
        {
            if (pd == null) return;

            lifeRemaining -= Time.deltaTime;
            if (lifeRemaining > 0f) return;

            if (!pd.addAttackRequiresHit) HandleAdditionalSpawns();

            Despawn();
        }

        public void OnChargeTick()
        {
            if (pd == null) return;

            lifeRemaining = pd.lifetime;
            CaptureSnapshot();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (pierced >= pd.numPierce && pd.destroyOnMaxPierce)
            {
                Despawn();
                return;
            }

            if (pierced >= pd.numPierce) return;
            if (hit.Contains(other.gameObject)) return;

            if (other.TryGetComponent<IStatProvider>(out var statManager) && ownerObj != other.gameObject)
                HandleHitEntity(other.gameObject);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (pierced >= pd.numPierce) return;
            if (hit.Contains(other.gameObject)) return;

            if (other.TryGetComponent<IStatProvider>(out var statManager) && ownerObj != other.gameObject)
                HandleHitEntity(other.gameObject);
        }

        private void HandleHitEntity(GameObject target)
        {
            if (target == null || ownerObj == null || target == ownerObj) return;
            if (!target.TryGetComponent<IDamageable>(out var eh)) return;

            var tid = target.TryGetComponent<ITeamMember>(out var itm) ? itm.TeamID : 0;
            var sid = ownerObj.TryGetComponent<ITeamMember>(out var sitm) ? sitm.TeamID : 0;

            if (sid == tid) return;

            DamagePacket dp = DamagePacketBuilder.BuildDamagePacket(pd, damageSnapshot, true, ownerObj, false, 1f);

            bool prevApplyingHit = ApplyingProjectileHit;
            ApplyingProjectileHit = true;
            try { eh.TakeDamage(dp); }
            finally { ApplyingProjectileHit = prevApplyingHit; }

            if (pd.kbForce > 0f && (eh as Component).TryGetComponent<Rigidbody2D>(out var rb2d))
            {
                Vector2 kbDir = (rb2d.transform.position - transform.position).normalized;

                float kbf = ownerObj.TryGetComponent<IStatProvider>(out var esm) ?
                    pd.kbForce * (1f + (esm.GetStat(StatType.kbPct) * 0.01f)) : pd.kbForce;

                if (target.TryGetComponent<IKnockbackable>(out var kb))
                    kb.ApplyKnockback(kbDir, kbf, pd.knockbackTime);
                else if (rb2d.bodyType == RigidbodyType2D.Dynamic)
                    rb2d.AddForce(kbDir * kbf, ForceMode2D.Impulse);
            }

            var (hp, stamina, mana) = CalculateStatGains(ownerObj, pd.mainAttack, dp.GetTotalDamage());
            TriggerStatGains(hp, stamina, mana, ownerObj);

            pierced++;
            hit.Add(target);

            foreach (var e in ownerObj.GetComponents<IOnHitEffect>())
                e.OnHit(ownerObj, target, transform.position);
            if (pd.mainAttack != null && pd.mainAttack.summonCondition == SummonCondition.OnHit && Random.value <= pd.mainAttack.summonChance)
            {
                if (ownerObj.TryGetComponent<ISummonTrigger>(out var ist))
                    ist.TrySummon(target.transform.position);
            }

            if (pd.timeBeforeSameEnemy > 0f) StartCoroutine(RemoveFromHitHistory(target, pd.timeBeforeSameEnemy));

            if (pd.additionalChance > 0f && pd.additionalAttack != null && Random.value <= pd.additionalChance)
                HandleAdditionalSpawns();

            canTriggerAdd = false;

            if (pd.effects != null && pd.effects.Count > 0)
            {
                foreach (var ed in pd.effects)
                {
                    if (ed.effect != null && ed.applyCondition == ApplyCondition.OnHit)
                    {
                        if (ed.selfApply) ApplyEffect(null, ed);
                        else if (ownerObj != target) ApplyEffect(target, ed);
                    }
                }
            }
        }

        private void HandleAdditionalSpawns()
        {
            if (!canTriggerAdd) return;
            if (pd.additionalAttack == null || pd.additionalAttack.projectilePrefab == null) return;
            if (ProjectileSpawner.Instance == null) return;

            ProjectileSpawner spawner = ProjectileSpawner.Instance;

            Vector2? addDir = pd.additionalFollowsMouse ? null : dir;

            spawner.StartCoroutine(spawner.SpawnFromPattern(pd.additionalAttack, ownerObj, transform.position, addDir, pd.additionalAttack.spawnDistance));
        }

        private void HandleSize()
        {
            if (ownerObj == null || pd == null) return;
            if (!ownerObj.TryGetComponent<IStatProvider>(out var esm)) return;

            float sizeMult = pd.size + (esm.GetStat(StatType.aoePct) * 0.01f);
            transform.localScale = Vector2.Max(new Vector2(sizeMult, sizeMult), Vector2.zero);
        }

        private void HandleDirection()
        {
            if (ownerObj == null || pd == null) return;

            if (pd.randomDir)
            {
                float randAngle = Random.Range(0f, 360f);
                dir = new Vector2(Mathf.Cos(randAngle * Mathf.Deg2Rad), Mathf.Sin(randAngle * Mathf.Deg2Rad));
                transform.rotation = Quaternion.Euler(0f, 0f, randAngle + pd.rotationOffset);
                return;
            }

            if (ownerObj.TryGetComponent<ITeamMember>(out var itm) && itm.TeamID == 1 && dir == Vector2.zero)
            {
                Camera cam = MainCam;
                if (cam != null)
                {
                    Vector3 mouseWorldPos = cam.ScreenToWorldPoint(InputState.mousePos);
                    mouseWorldPos.z = 0f;

                    dir = (mouseWorldPos - transform.position).normalized;
                }
            }

            transform.rotation = Quaternion.Euler(0f, 0f, GetSpriteAngle(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg));
        }

        private float GetSpriteAngle(float moveAngle)
        {
            if (!pd.useTrueAngle) return moveAngle + pd.rotationOffset;

            Vector2 trueAngle = new(Mathf.Cos(pd.angleOverride * Mathf.Deg2Rad), Mathf.Sin(pd.angleOverride * Mathf.Deg2Rad));
            return Mathf.Atan2(trueAngle.y, trueAngle.x) * Mathf.Rad2Deg;
        }

        private void InitBoomerang()
        {
            if (pd.maxBoomerangDist > 0f)
            {
                boomerangActive = true;
                boomerangReturning = false;
                boomerangSpeed = effSpd;
                boomerangDecel = effSpd * effSpd / (2f * pd.maxBoomerangDist);
            }
        }

        private void HandleMovement(bool start)
        {
            if (rb == null || ownerObj == null || pd == null) return;

            if (pd.followSource)
            {
                HandleFollowSourceMovement();
                return;
            }

            if (effSpd <= 0) return;

            if (MoveType == MovementType.FollowCursor)
            {
                HandleCursorFollow();
                return;
            }

            if (MoveType != MovementType.Default)
            {
                HandlePatternMovement(start);
                return;
            }

            if (pd.orbitRadius > 0 && !orbitCancelled)
            {
                HandleOrbitMovement();
                return;
            }

            if (start) rb.linearVelocity = dir.normalized * effSpd;

            if (pd.followDistance > 0 && TryHome()) return;

            if (boomerangActive) UpdateBoomerang();
        }

        private bool TryHome()
        {
            if (followTarget == null || !followTarget.gameObject.activeInHierarchy)
            {
                bool searchForPlayer = ownerObj.TryGetComponent<ITeamMember>(out var itm) && itm.TeamID == 0;
                followTarget = FindClosestTargetInRange(pd.followDistance, searchForPlayer);
            }

            if (followTarget == null) return false;

            boomerangActive = false;
            FollowTarget();
            return true;
        }

        private void HandleFollowSourceMovement()
        {
            if (sourceRb == null)
            {
                if (ownerObj == null) return;
                sourceRb = ownerObj.GetComponent<Rigidbody2D>();
                if (sourceRb == null) return;
            }

            rb.linearVelocity = sourceRb.linearVelocity;
        }

        private void HandlePatternMovement(bool start)
        {
            if (start)
            {
                ResetPattern();
                return;
            }

            if (pd.followDistance > 0 && TryHome())
            {
                patternSuspended = true;
                return;
            }

            if (patternSuspended)
            {
                patternSuspended = false;
                if (rb.linearVelocity.sqrMagnitude > 0.0001f) dir = rb.linearVelocity.normalized;
                ResetPattern();
            }

            float dt = Time.fixedDeltaTime;
            if (dt <= 0f) return;

            patternTime += dt;

            Vector2 target = MoveType switch
            {
                MovementType.Wave => GetWavePosition(),
                MovementType.Spiral => GetSpiralPosition(dt),
                _ => (Vector2)transform.position
            };
            rb.linearVelocity = (target - (Vector2)transform.position) / dt;
        }

        private void HandleCursorFollow()
        {
            Vector2 targetPos;

            if (ownerObj.TryGetComponent<ITeamMember>(out var itm) && itm.TeamID == 1)
            {
                Camera cam = MainCam;
                if (cam == null) return;

                Vector3 mouseWorld = cam.ScreenToWorldPoint(InputState.mousePos);
                mouseWorld.z = 0f;
                targetPos = mouseWorld;
            }
            else
            {
                if (followTarget == null || !followTarget.gameObject.activeInHierarchy)
                    followTarget = FindClosestTargetInRange(pd.followDistance > 0f ? pd.followDistance : 50f, true);

                if (followTarget == null) return;
                targetPos = followTarget.position;
            }

            Vector2 toTarget = targetPos - (Vector2)transform.position;
            float dt = Time.fixedDeltaTime;

            if (toTarget.sqrMagnitude > 0.0001f) dir = toTarget.normalized;

            rb.linearVelocity = dt > 0f && toTarget.magnitude <= effSpd * dt ? toTarget / dt : dir * effSpd;
            transform.rotation = Quaternion.Euler(0f, 0f, GetSpriteAngle(Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg));
        }

        private void ResetPattern()
        {
            patternOrigin = transform.position;
            patternTime = 0f;
            spiralTheta = 0f;
            patternSuspended = false;
            patternBaseAngle = dir != Vector2.zero ? Mathf.Atan2(dir.y, dir.x) : 0f;
        }

        private Vector2 GetWavePosition()
        {
            Vector2 fwd = dir.normalized;
            Vector2 perp = Vector2.Perpendicular(fwd);
            float offset = WaveAmp * Mathf.Sin(2f * Mathf.PI * WaveFreq * patternTime);

            return patternOrigin + (fwd * (effSpd * patternTime)) + (perp * offset);
        }

        private Vector2 GetSpiralPosition(float dt)
        {
            float b = Mathf.Max(SpiralSpacing, 0.01f) / (2f * Mathf.PI);
            float r = b * spiralTheta;

            spiralTheta += effSpd * dt / Mathf.Sqrt((r * r) + (b * b));

            float sign = pd.rotateClockwise ? -1f : 1f;
            float angle = patternBaseAngle + (sign * spiralTheta);

            return patternOrigin + (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (b * spiralTheta));
        }

        private void UpdateBoomerang()
        {
            float dt = Time.fixedDeltaTime;

            if (!boomerangReturning)
            {
                boomerangSpeed -= boomerangDecel * dt;

                if (boomerangSpeed <= 0f)
                {
                    boomerangSpeed = 0f;
                    boomerangReturning = true;
                }

                rb.linearVelocity = dir.normalized * boomerangSpeed;
            }
            else
            {
                boomerangSpeed += boomerangDecel * dt;
                boomerangSpeed = Mathf.Min(boomerangSpeed, effSpd);

                rb.linearVelocity = -dir.normalized * boomerangSpeed;
            }
        }

        private void FollowTarget()
        {
            if (followTarget == null) return;

            float dist = Vector2.Distance(transform.position, followTarget.position);
            if (dist <= pd.followDistance)
            {
                Vector2 newDir = (followTarget.position - transform.position).normalized;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, newDir * effSpd, 0.1f);
            }
        }

        private void HandleOrbitMovement()
        {
            if (orbitTarget == null || !orbitTarget.gameObject.activeInHierarchy)
            {
                if (pd != null && pd.orbitSelf && ownerObj != null) orbitTarget = ownerObj.transform;
                else orbitTarget = FindClosestEnemyInDirection();
                orbitDirectionSign = 0f;
                orbitInitialized = false;
            }

            if (orbitTarget == null) return;

            Vector2 center = orbitTarget.position;
            Vector2 offset = (Vector2)transform.position - center;
            float dist = offset.magnitude;

            if (dist < 0.01f)
            {
                rb.linearVelocity = dir.normalized * effSpd;
                return;
            }

            if (!orbitInitialized)
            {
                effectiveOrbitRadius = pd.orbitRadius + Random.Range(0f, pd.randOrbRadOffset);
                orbitDirectionSign = pd.rotateClockwise ? -1f : 1f;

                if (dist < effectiveOrbitRadius * 0.5f && dir != Vector2.zero)
                    orbitAngleOffset = Mathf.Atan2(dir.y, dir.x);
                else
                    orbitAngleOffset = Mathf.Atan2(offset.y, offset.x);

                orbitInitialized = true;
            }

            float currentAngle = Mathf.Atan2(offset.y, offset.x);
            float targetAngle = orbitAngleOffset + (orbitDirectionSign * effSpd * Time.fixedDeltaTime / effectiveOrbitRadius);

            orbitAngleOffset = targetAngle;

            Vector2 desiredPos = center + (new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle)) * effectiveOrbitRadius);
            Vector2 toDesired = desiredPos - (Vector2)transform.position;

            Vector2 tangent = Vector2.Perpendicular(desiredPos - center).normalized;
            Vector2 orbitalVelocity = orbitDirectionSign * effSpd * tangent;

            float radiusError = Vector2.Distance(transform.position, center) - effectiveOrbitRadius;
            Vector2 radialCorrection = -5f * radiusError * (desiredPos - center).normalized;

            rb.linearVelocity = orbitalVelocity + radialCorrection + (toDesired * 5f);
        }

        private static readonly List<Collider2D> OverlapBuffer = new();
        private static Camera cachedMainCam;
        private static Camera MainCam => cachedMainCam != null ? cachedMainCam : cachedMainCam = Camera.main;

        private static int OverlapCircle(Vector2 position, float radius)
        {
            ContactFilter2D filter = default;
            filter.useTriggers = Physics2D.queriesHitTriggers;

            return Physics2D.OverlapCircle(position, radius, filter, OverlapBuffer);
        }

        private static bool IsDead(GameObject go)
            => go.TryGetComponent<IStatProvider>(out var esm)
               && (esm.GetStat(StatType.isAlive) <= 0f || esm.GetStat(StatType.currentHp) <= 0f);

        private int OwnTeam()
            => ownerObj != null && ownerObj.TryGetComponent<ITeamMember>(out var itm) ? itm.TeamID : 0;

        private Transform FindClosestEnemyInDirection()
        {
            Transform closest = null;
            float closestDist = float.MaxValue;

            int targetTeam = OwnTeam() == 0 ? 1 : 0;
            float searchRadius = effSpd * pd.lifetime;
            int count = OverlapCircle(transform.position, searchRadius);

            for (int i = 0; i < count; i++)
            {
                Collider2D col = OverlapBuffer[i];
                if (!col.gameObject.TryGetComponent<ITeamMember>(out var itm) || itm.TeamID != targetTeam) continue;
                if (hit.Contains(col.gameObject)) continue;
                if (col.gameObject == ownerObj) continue;

                Vector2 toEnemy = (col.transform.position - transform.position).normalized;
                float dot = Vector2.Dot(dir.normalized, toEnemy);
                if (dot <= 0) continue;

                if (IsDead(col.gameObject)) continue;

                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = col.transform;
                }
            }
            return closest;
        }

        private Transform FindClosestTargetInRange(float range, bool searchForPlayer)
        {
            Transform closest = null;
            float minDist = range;

            int count = OverlapCircle(transform.position, range);
            int targetTeam = searchForPlayer ? 1 : 0;

            for (int i = 0; i < count; i++)
            {
                Collider2D col = OverlapBuffer[i];
                if (!col.gameObject.TryGetComponent<ITeamMember>(out var itm) || itm.TeamID != targetTeam) continue;

                if (hit.Contains(col.gameObject)) continue;

                if (IsDead(col.gameObject)) continue;

                float dist = Vector2.Distance(transform.position, col.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = col.transform;
                }
            }
            return closest;
        }

        public void Launch(Vector2 direction)
        {
            orbitCancelled = true;
            orbitTarget = null;
            dir = direction.normalized;
            if (pd != null && MoveType != MovementType.Default) ResetPattern();
            if (rb != null) rb.linearVelocity = dir * effSpd;
        }

        public void Explode()
        {
            if (pd.additionalAttack != null && pd.additionalAttack.projectilePrefab != null && ProjectileSpawner.Instance != null)
            {
                ProjectileSpawner spawner = ProjectileSpawner.Instance;
                Vector2? addDir = pd.additionalFollowsMouse ? null : dir;
                spawner.StartCoroutine(spawner.SpawnFromPattern(pd.additionalAttack, ownerObj, transform.position, addDir, pd.additionalAttack.spawnDistance));
            }
            Despawn();
        }

        private void ApplyEffect(GameObject target, EffectData ed)
        {
            if (ed.effect == null) return;

            if (target == null) target = ownerObj;

            if (target.TryGetComponent<IStatusEffectReceiver>(out var sem))
            {
                if (ed.chance <= 0f) return;

                if (Random.value <= ed.chance)
                    sem.Apply(ed.effect, ownerObj);
            }
        }

        private System.Collections.IEnumerator RemoveFromHitHistory(GameObject target, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (hit != null && target != null) hit.Remove(target);
            canTriggerAdd = true;
        }

        public static (float hp, float stamina, float mana) CalculateStatGains(GameObject target, AttackData a, float totalDmg = 0f)
        {
            if (target == null || !target.TryGetComponent<IStatProvider>(out var esm)) return (0f, 0f, 0f);

            float totalStamina = a.staminaGainOnHit;
            float totalHp = a.healthGainOnHit;
            float totalMana = a.manaGainOnHit;

            if (a.basedOnDmgDealt)
            {
                totalStamina += totalDmg * 0.01f * a.staminaPctGainOnHit;
                totalHp += totalDmg * 0.01f * a.healthPctGainOnHit;
                totalMana += totalDmg * 0.01f * a.manaPctGainOnHit;
            }
            else if (totalDmg > 0f)
            {
                totalStamina += a.staminaPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxStamina);
                totalHp += a.healthPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxHp);
                totalMana += a.manaPctGainOnHit * 0.01f * esm.GetStat(StatType.EffMaxMana);
            }
            return (totalHp, totalStamina, totalMana);
        }

        private void TriggerStatGains(float hp, float stamina, float mana, GameObject target)
        {
            if (target == null) return;
            if (target.TryGetComponent<IResourcePool>(out var rp))
            {
                rp.TryGain(ResourceType.Stamina, stamina);
                rp.TryGain(ResourceType.Mana, mana);
            }

            if (target.TryGetComponent<IDamageable>(out var eh))
            {
                var dp = DamagePacketBuilder.BuildDamagePacket(hp, DamageType.Heal, false, Color.green, target, true, 1f);
                eh.TakeDamage(dp);
            }
        }

        private void CaptureSnapshot() => damageSnapshot = ProjectileSnapshot.CaptureSnapshot(pd, ownerObj);
    }
}
