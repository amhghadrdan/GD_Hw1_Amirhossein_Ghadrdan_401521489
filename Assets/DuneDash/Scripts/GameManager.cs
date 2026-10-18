using System;
using UnityEngine;

namespace DuneDash
{
    public enum GameState { MainMenu, Running, Paused, Dying, GameOver }

    /// <summary>Owns the run lifecycle, score, difficulty and persistent record.</summary>
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public GameState State { get; private set; } = GameState.MainMenu;
        public bool IsRunning => State == GameState.Running;
        public int Score => Mathf.FloorToInt(distance * 1.7f) + bonus;
        public int HighScore { get; private set; }
        public int Coins { get; private set; }
        public float Distance => distance;
        public float Speed => Mathf.Min(13f, 7f + distance / 160f);
        public DinoController Player { get; private set; }
        public GameUI UI { get; private set; }
        public ObstacleSpawner Spawner { get; private set; }
        public event Action<GameState> StateChanged;
        public const string RecordKey = "DuneDash.HighScore.v1";
        public string PersistenceKey { get; private set; } = RecordKey;
        public bool SuppressRecordWrites { get; set; }
        float distance, deathTime;
        int bonus;
        bool initialized;

        void Awake() { Instance = this; }
        public void Initialize(DinoController player, GameUI ui, ObstacleSpawner spawner)
        {
            Player = player; UI = ui; Spawner = spawner;
            // Automated checks never overwrite the player's real record.
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-smokeTest") >= 0)
                PersistenceKey = "DuneDash.Test." + Guid.NewGuid().ToString("N");
            HighScore = PlayerPrefs.GetInt(PersistenceKey, 0);
            initialized = true;
            Player.ShowOnMenu();
            SetState(GameState.MainMenu);
        }
        void Update()
        {
            if (!initialized) return;
            if (IsRunning)
            {
                distance += Speed * Time.deltaTime;
                if (Score > HighScore) HighScore = Score;
            }
            if (State == GameState.Dying && Time.unscaledTime >= deathTime)
                SetState(GameState.GameOver);
            UI.Refresh();
        }
        public void StartRun()
        {
            Time.timeScale = 1;
            distance = 0; bonus = 0; Coins = 0;
            Spawner.Clear();
            Player.ResetForRun();
            SetState(GameState.Running);
            Spawner.Begin();
            AudioDirector.Instance.Play("Click");
        }
        public void TogglePause()
        {
            if (State == GameState.Running) Pause();
            else if (State == GameState.Paused) Resume();
        }
        public void Pause()
        {
            if (!IsRunning) return;
            SaveRecord();
            Time.timeScale = 0;
            SetState(GameState.Paused);
        }
        public void Resume()
        {
            if (State != GameState.Paused) return;
            Time.timeScale = 1;
            SetState(GameState.Running);
        }
        public void ReturnToMenu()
        {
            SaveRecord(); Time.timeScale = 1;
            Spawner.Clear(); Player.ShowOnMenu();
            SetState(GameState.MainMenu);
        }
        public void AddCoin(Vector3 position)
        {
            if (!IsRunning) return;
            Coins++; bonus += 25;
            HighScore = Mathf.Max(HighScore, Score);
            AudioDirector.Instance.Play("Coin");
            VisualEffects.Instance.Burst(position, new Color(1f,.78f,.32f), 10);
            UI.ShowToast("+25", position);
        }
        public void Smash(Vector3 position)
        {
            bonus += 5; HighScore = Mathf.Max(HighScore, Score);
            VisualEffects.Instance.Burst(position, new Color(.38f,.67f,.57f), 14);
            UI.ShowToast("DASH +5", position);
        }
        public void EndRun()
        {
            if (!IsRunning) return;
            SaveRecord();
            deathTime = Time.unscaledTime + .85f;
            SetState(GameState.Dying);
            AudioDirector.Instance.Play("Death");
        }
        public void SaveRecord()
        {
            if (SuppressRecordWrites) return;
            HighScore = Mathf.Max(HighScore, Score);
            PlayerPrefs.SetInt(PersistenceKey, HighScore);
            PlayerPrefs.Save();
        }
        void SetState(GameState state)
        {
            State = state;
            Player.SetSimulation(state == GameState.Running);
            UI.ShowState(state);
            StateChanged?.Invoke(state);
        }
        public void Quit()
        {
            SaveRecord();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        void OnApplicationPause(bool paused) { if (paused && IsRunning) Pause(); }
        void OnApplicationFocus(bool focused) { if (!focused && IsRunning) Pause(); }
        void OnApplicationQuit() { if (initialized) SaveRecord(); }
        void OnDestroy() { Time.timeScale = 1; if (Instance == this) Instance = null; }
    }
}
