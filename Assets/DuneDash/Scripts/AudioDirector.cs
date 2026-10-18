using System.Collections.Generic;
using UnityEngine;
namespace DuneDash
{
    public sealed class AudioDirector : MonoBehaviour
    {
        public static AudioDirector Instance { get; private set; }
        public bool Muted { get; private set; }
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioSource effects, music;
        void Awake()
        {
            Instance = this;
            effects = gameObject.AddComponent<AudioSource>(); effects.playOnAwake = false; effects.volume = .6f;
            music = gameObject.AddComponent<AudioSource>(); music.playOnAwake = false;
            music.clip = Resources.Load<AudioClip>("Audio/Ambience"); music.loop = true; music.volume = .4f;
            foreach (var name in new[]{"Jump","Dash","Hit","Coin","Death","Click"}) clips[name] = Resources.Load<AudioClip>("Audio/"+name);
            Muted = PlayerPrefs.GetInt("DuneDash.Muted",0) == 1;
            ApplyMute(); music.Play();
        }
        public void Play(string name)
        { if (!Muted && clips.TryGetValue(name,out var clip) && clip != null) effects.PlayOneShot(clip); }
        public void ToggleMute()
        {
            Muted = !Muted; ApplyMute();
            PlayerPrefs.SetInt("DuneDash.Muted",Muted?1:0); PlayerPrefs.Save();
        }
        void ApplyMute() { effects.mute = Muted; music.mute = Muted; }
        void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
