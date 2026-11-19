using UnityEngine;

namespace OldAlley.Audio
{
    /// <summary>
    /// 占位音效合成：方波 + 频率滑动 + 衰减包络。
    /// 后续放入 Assets/_Game/Audio/SFX 同名文件后可替换为真实素材。
    /// </summary>
    public static class SfxSynth
    {
        private const int SampleRate = 44100;

        public static AudioClip Tone(string name, float startHz, float endHz, float seconds, float volume = 0.35f)
        {
            var count = Mathf.Max(1, Mathf.CeilToInt(SampleRate * seconds));
            var data = new float[count];
            var phase = 0f;

            for (var i = 0; i < count; i++)
            {
                var t = i / (float)count;
                var hz = Mathf.Lerp(startHz, endHz, t);
                phase += hz / SampleRate;

                var square = Mathf.Sign(Mathf.Sin(phase * Mathf.PI * 2f));
                var attack = Mathf.Min(1f, t * 24f);
                var decay = Mathf.Pow(1f - t, 2f);
                data[i] = square * attack * decay * volume;
            }

            var clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
