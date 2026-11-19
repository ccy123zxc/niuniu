using System.Collections.Generic;
using OldAlley.Core;
using UnityEngine;

namespace OldAlley.Audio
{
    public enum SfxId
    {
        Shoot,
        Hit,
        Ko,
        Pickup,
        HouseHurt,
        Wave,
        UiClick
    }

    /// <summary>
    /// 音效与 BGM 入口。没有音频文件时用 SfxSynth 生成占位音，
    /// 音效音量读取 GameSettings。
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource _sfx;
        private AudioSource _bgm;
        private readonly Dictionary<SfxId, AudioClip> _clips = new Dictionary<SfxId, AudioClip>();

        private void Awake()
        {
            Instance = this;

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.playOnAwake = false;

            _bgm = gameObject.AddComponent<AudioSource>();
            _bgm.loop = true;
            _bgm.playOnAwake = false;
            _bgm.volume = 0.25f;

            BuildClips();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildClips()
        {
            _clips[SfxId.Shoot] = SfxSynth.Tone("shoot", 880f, 880f, 0.06f);
            _clips[SfxId.Hit] = SfxSynth.Tone("hit", 220f, 220f, 0.08f);
            _clips[SfxId.Ko] = SfxSynth.Tone("ko", 330f, 110f, 0.12f);
            _clips[SfxId.Pickup] = SfxSynth.Tone("pickup", 1320f, 1320f, 0.12f);
            _clips[SfxId.HouseHurt] = SfxSynth.Tone("house_hurt", 110f, 110f, 0.2f);
            _clips[SfxId.Wave] = SfxSynth.Tone("wave", 660f, 660f, 0.15f);
            _clips[SfxId.UiClick] = SfxSynth.Tone("ui_click", 1000f, 1000f, 0.04f, 0.25f);

            Debug.Log("[AudioManager] 使用 SfxSynth 占位音效（当前无音频文件）");
        }

        public static void Play(SfxId id)
        {
            if (Instance == null || Instance._sfx == null) return;

            AudioClip clip;
            if (!Instance._clips.TryGetValue(id, out clip) || clip == null) return;

            Instance._sfx.pitch = Random.Range(0.95f, 1.05f);
            Instance._sfx.PlayOneShot(clip, Mathf.Clamp01(GameSettings.SfxVolume));
        }

        public static void PlayBgm(AudioClip clip, float volume)
        {
            if (Instance == null || Instance._bgm == null) return;

            Instance._bgm.clip = clip;
            Instance._bgm.volume = volume;
            if (clip != null) Instance._bgm.Play();
            else Instance._bgm.Stop();
        }
    }
}
