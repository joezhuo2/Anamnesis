using CrystalFlux.EntitySystem;
using UnityEngine;

public class BlackoutHider : MonoBehaviour
{
    private SpriteRenderer[] srs;
    private SpriteMaskInteraction[] orig;
    private EntityHealth eh;
    private bool hidden;

    private void Awake()
    {
        if (BlackoutVision.Current == null)
        {
            Destroy(this);
            return;
        }

        srs = GetComponentsInChildren<SpriteRenderer>(true);
        orig = new SpriteMaskInteraction[srs.Length];

        for (int i = 0; i < srs.Length; i++)
        {
            orig[i] = srs[i].maskInteraction;
            srs[i].maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }

        TryGetComponent(out eh);
    }

    private void Update()
    {
        BlackoutVision v = BlackoutVision.Current;
        if (v == null)
        {
            Destroy(this);
            return;
        }

        bool outside = !v.InSight(transform.position);
        if (outside == hidden) return;

        hidden = outside;
        if (eh != null) eh.SetBarHidden(hidden);
    }

    private void OnDestroy()
    {
        if (srs != null)
            for (int i = 0; i < srs.Length; i++)
                if (srs[i] != null) srs[i].maskInteraction = orig[i];

        if (hidden && eh != null) eh.SetBarHidden(false);
    }
}
