using CrystalFlux.EntitySystem;
using CrystalFlux.SkillTree;
using UnityEngine;

namespace CrystalFlux.WaveSystem
{
    public class HomeScreenUI : MonoBehaviour
    {
        [Tooltip("Hidden on the home screen, shown once a run starts.")]
        public GameObject[] hideOnHome;

        private CanvasGroup[] groups;
        private Behaviour[] inputs;
        private bool? shown;

        private void Awake()
        {
            groups = new CanvasGroup[hideOnHome.Length];
            for (int i = 0; i < hideOnHome.Length; i++)
            {
                GameObject go = hideOnHome[i];
                if (go != null) groups[i] = go.TryGetComponent(out CanvasGroup g) ? g : go.AddComponent<CanvasGroup>();
            }
        }

        private void Start()
        {
            inputs = new Behaviour[] { FindAnyObjectByType<PlayerInputHandler>(), FindAnyObjectByType<SkillTreeInputToggle>() };
            Update();
        }

        private void Update()
        {
            bool inRun = WaveManager.Active != null;
            if (shown == inRun) return;
            shown = inRun;

            foreach (CanvasGroup g in groups)
            {
                if (g == null) continue;
                g.alpha = inRun ? 1f : 0f;
                g.interactable = g.blocksRaycasts = inRun;
            }

            foreach (Behaviour b in inputs)
                if (b != null) b.enabled = inRun;
        }
    }
}
