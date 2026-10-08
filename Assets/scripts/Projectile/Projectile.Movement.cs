using System.Collections;
using System.Collections.Generic;
using CrystalFlux.Core;
using CrystalFlux.SettingsSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CrystalFlux.ProjectileSystem
{
    public partial class Projectile
    {
        private void HandleSize()
        {
            if (ownerObj == null || pd == null || ownerStats == null) return;

            float sizeMult = Mathf.Max(0f, pd.Size * (1f + (ownerStats.GetStat(StatType.aoePct) * 0.01f)));
            transform.localScale = new Vector3(defaultScale.x * sizeMult, defaultScale.y * sizeMult, defaultScale.z);
        }

        private void HandleDirection()
        {
            if (ownerObj == null || pd == null) return;

            if (pd.RandomDir)
            {
                float randAngle = Random.Range(0f, 360f);
                dir = new Vector2(Mathf.Cos(randAngle * Mathf.Deg2Rad), Mathf.Sin(randAngle * Mathf.Deg2Rad));
                transform.rotation = Quaternion.Euler(0f, 0f, randAngle + pd.RotationOffset);
                return;
            }

            if (ownerTeam == 1 && dir == Vector2.zero)
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
            if (!pd.UseTrueAngle) return moveAngle + pd.RotationOffset;

            Vector2 trueAngle = new(Mathf.Cos(pd.AngleOverride * Mathf.Deg2Rad), Mathf.Sin(pd.AngleOverride * Mathf.Deg2Rad));
            return Mathf.Atan2(trueAngle.y, trueAngle.x) * Mathf.Rad2Deg;
        }

        private void InitBoomerang()
        {
            if (pd.MaxBoomerangDist > 0f)
            {
                boomerangActive = true;
                boomerangReturning = false;
                boomerangSpeed = effSpd;
                boomerangDecel = effSpd * effSpd / (2f * pd.MaxBoomerangDist);
            }
        }

        private void HandleMovement(bool start)
        {
            if (rb == null || ownerObj == null || pd == null) return;

            if (pd.FollowSource)
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

            if (pd.OrbitRadius > 0 && !orbitCancelled)
            {
                HandleOrbitMovement();
                return;
            }

            if (start) rb.linearVelocity = dir.normalized * effSpd;

            if (pd.FollowDistance > 0 && TryHome()) return;

            if (boomerangActive) UpdateBoomerang();
        }

        private bool TryHome()
        {
            if (followTarget != null && !followTarget.gameObject.activeInHierarchy) followTarget = null;
            if (followTarget == null && RetargetReady())
                followTarget = FindClosestTargetInRange(pd.FollowDistance, ownerTeam == 0);

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

            if (pd.FollowDistance > 0 && TryHome())
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

            if (ownerTeam == 1)
            {
                Camera cam = MainCam;
                if (cam == null) return;

                Vector3 mouseWorld = cam.ScreenToWorldPoint(InputState.mousePos);
                mouseWorld.z = 0f;
                targetPos = mouseWorld;
            }
            else
            {
                if (followTarget != null && !followTarget.gameObject.activeInHierarchy) followTarget = null;
                if (followTarget == null && RetargetReady())
                    followTarget = FindClosestTargetInRange(pd.FollowDistance > 0f ? pd.FollowDistance : 50f, true);

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

            float sign = pd.RotateClockwise ? -1f : 1f;
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
            if (dist <= pd.FollowDistance)
            {
                Vector2 newDir = (followTarget.position - transform.position).normalized;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, newDir * effSpd, 0.1f);
            }
        }

        private void HandleOrbitMovement()
        {
            if (orbitTarget == null || !orbitTarget.gameObject.activeInHierarchy)
            {
                if (pd != null && pd.OrbitSelf && ownerObj != null) orbitTarget = ownerObj.transform;
                else orbitTarget = RetargetReady() ? FindClosestEnemyInDirection() : null;
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
                effectiveOrbitRadius = pd.OrbitRadius + Random.Range(0f, pd.RandOrbRadOffset);
                orbitDirectionSign = pd.RotateClockwise ? -1f : 1f;

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

        private static int OverlapCircle(Vector2 position, float radius)
        {
            ContactFilter2D filter = default;
            filter.useTriggers = false;

            return Physics2D.OverlapCircle(position, radius, filter, OverlapBuffer);
        }

        private static bool IsDead(GameObject go)
            => go.TryGetComponent<IStatProvider>(out var esm)
               && (esm.GetStat(StatType.isAlive) <= 0f || esm.GetStat(StatType.currentHp) <= 0f);

        private Transform FindClosestEnemyInDirection()
        {
            Transform closest = null;
            float closestDist = float.MaxValue;

            int targetTeam = ownerTeam == 0 ? 1 : 0;
            float searchRadius = effSpd * pd.Lifetime;
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
    }
}
