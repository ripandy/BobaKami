using UnityEngine;
using UnityEngine.InputSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BobaKami.Gameplay
{
    /// <summary>
    /// Input System processor that maps a screen-space pointer position (pixels) to [-1, 1] on
    /// each axis, so touch/mouse position feeds <see cref="FaceDirectionConverterVectorVariable"/>'s
    /// threshold space (x around [-1, 1]) instead of raw pixels.
    /// Attach it to a <c>&lt;Pointer&gt;/position</c> binding by the name "ScreenNormalize"
    /// (the Input System auto-clips the "Processor" suffix).
    /// </summary>
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class ScreenNormalizeProcessor : InputProcessor<Vector2>
    {
#if UNITY_EDITOR
        static ScreenNormalizeProcessor()
        {
            // Register in the editor too, so the .inputactions asset resolves the processor at import time.
            Register();
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            InputSystem.RegisterProcessor<ScreenNormalizeProcessor>();
        }

        public override Vector2 Process(Vector2 value, InputControl control)
        {
            var width = Screen.width;
            var height = Screen.height;
            var x = width > 0 ? Mathf.Clamp(value.x / width * 2f - 1f, -1f, 1f) : 0f;
            var y = height > 0 ? Mathf.Clamp(value.y / height * 2f - 1f, -1f, 1f) : 0f;
            return new Vector2(x, y);
        }
    }
}
