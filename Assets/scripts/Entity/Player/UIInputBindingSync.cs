using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace CrystalFlux.EntitySystem
{
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class UIInputBindingSync : MonoBehaviour
    {
        private InputSystemUIInputModule module;

        private void Awake()
        {
            module = GetComponent<InputSystemUIInputModule>();
            if (module != null) GameInput.RegisterExternalAsset(module.actionsAsset);
        }

        private void OnDestroy()
        {
            if (module != null) GameInput.UnregisterExternalAsset(module.actionsAsset);
        }
    }
}
