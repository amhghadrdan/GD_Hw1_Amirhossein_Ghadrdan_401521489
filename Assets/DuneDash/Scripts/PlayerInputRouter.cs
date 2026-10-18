using UnityEngine;
using UnityEngine.InputSystem;

namespace DuneDash
{
    /// <summary>All game controls use InputAction; UI uses InputSystemUIInputModule.</summary>
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        InputActionAsset controls;
        InputAction jump, dash, pause, restart;
        void Start()
        {
            controls = Instantiate(Resources.Load<InputActionAsset>("RunnerControls"));
            jump = controls.FindAction("Runner/Jump", true);
            dash = controls.FindAction("Runner/Dash", true);
            pause = controls.FindAction("Runner/Pause", true);
            restart = controls.FindAction("Runner/Restart", true);
            controls.Enable();
        }
        void Update()
        {
            var game = GameManager.Instance;
            if (controls == null || game == null) return;
            if (pause.WasPressedThisFrame()) { if (game.UI.IsHelpOpen) game.UI.CloseHelp(); else game.TogglePause(); return; }
            if (restart.WasPressedThisFrame() && (game.State == GameState.GameOver || game.State == GameState.Paused))
            { game.StartRun(); return; }
            if (jump.WasPressedThisFrame())
            {
                if (game.State == GameState.MainMenu) game.StartRun();
                else if (game.IsRunning) game.Player.Jump();
            }
            if (dash.WasPressedThisFrame() && game.IsRunning) game.Player.Dash();
        }
        void OnEnable() { if (controls != null) controls.Enable(); }
        void OnDisable() { if (controls != null) controls.Disable(); }
        void OnDestroy() { if (controls != null) Destroy(controls); }
    }
}
