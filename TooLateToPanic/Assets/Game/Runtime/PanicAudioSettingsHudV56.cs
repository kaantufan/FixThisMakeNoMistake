using UnityEngine;

namespace TooLateToPanic
{
    public sealed partial class PanicHomeHud
    {
        bool audioSettingsOpenV56;
        int audioChoiceV56;
        public bool AudioSettingsOpenV56 { get { return audioSettingsOpenV56; } }
        public void OpenAudioSettingsV56()
        {
            if (!World || DeveloperMenuOpen) return;
            if (!_paused) SetPaused(true);
            audioChoiceV56 = 0; audioSettingsOpenV56 = true;
        }
        public void CloseAudioSettingsV56() { audioSettingsOpenV56 = false; }
        void DrawAudioSettingsV56()
        {
            Fill(LogicalCanvasBounds, new Color(.055f, .05f, .045f, .72f));
            Rect panel = new Rect(280, 142, 1040, 640);
            Frame(panel, Night, Line, 1); Region(panel);
            Label(new Rect(330, 178, 940, 63), "SES AYARLARI", _title);
            Label(new Rect(334, 248, 920, 35), "Değişiklikler kaydedilir. Oyun ve süre duruyor.", _small);
            DrawAudioVolumeRowV56(0, 310, "GENEL", PanicAudioSettingsV56.MasterVolume);
            DrawAudioVolumeRowV56(1, 400, "MÜZİK", PanicAudioSettingsV56.MusicVolume);
            DrawAudioVolumeRowV56(2, 490, "EFEKTLER", PanicAudioSettingsV56.SfxVolume);
            Button(new Rect(334, 591, 450, 54), PanicAudioSettingsV56.Muted ? "SESSİZ · AÇIK" : "SESSİZ · KAPALI",
                audioChoiceV56 == 3, Brass, () => PanicAudioSettingsV56.SetMuted(!PanicAudioSettingsV56.Muted));
            Button(new Rect(812, 591, 450, 54), PanicAudioSettingsV56.MusicEnabled ? "MÜZİK · AÇIK" : "MÜZİK · KAPALI",
                audioChoiceV56 == 4, Sea, () => PanicBackgroundMusic.Enabled = !PanicBackgroundMusic.Enabled);
            Button(new Rect(334, 687, 450, 54), "GERİ · ESC", audioChoiceV56 == 5, Sea, CloseAudioSettingsV56);
            Label(new Rect(812, 686, 450, 57), "↑ ↓ seç  ·  ← → seviye\nEnter aç / kapat", _small);
            var e = Event.current;
            if (e.type != EventType.KeyDown || DeveloperInputBlockedThisFrame) return;
            if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow)
            { audioChoiceV56 = (audioChoiceV56 + (e.keyCode == KeyCode.UpArrow ? 5 : 1)) % 6; e.Use(); }
            else if (audioChoiceV56 < 3 && (e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.RightArrow))
            {
                float value = AudioVolumeV56(audioChoiceV56) + (e.keyCode == KeyCode.LeftArrow ? -.05f : .05f);
                SetAudioVolumeV56(audioChoiceV56, value); e.Use();
            }
            else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
            {
                if (audioChoiceV56 == 3) PanicAudioSettingsV56.SetMuted(!PanicAudioSettingsV56.Muted);
                else if (audioChoiceV56 == 4) PanicBackgroundMusic.Enabled = !PanicBackgroundMusic.Enabled;
                else if (audioChoiceV56 == 5) CloseAudioSettingsV56();
                e.Use();
            }
        }
        float AudioVolumeV56(int index)
        { return index == 0 ? PanicAudioSettingsV56.MasterVolume : index == 1 ? PanicAudioSettingsV56.MusicVolume : PanicAudioSettingsV56.SfxVolume; }
        void SetAudioVolumeV56(int index, float value)
        {
            value = Mathf.Round(Mathf.Clamp01(value) * 100f) / 100f;
            if (Mathf.Abs(value - AudioVolumeV56(index)) < .001f) return;
            PanicAudioSettingsV56.SetVolumes(index == 0 ? value : PanicAudioSettingsV56.MasterVolume,
                index == 1 ? value : PanicAudioSettingsV56.MusicVolume,
                index == 2 ? value : PanicAudioSettingsV56.SfxVolume);
        }
        void DrawAudioVolumeRowV56(int index, float y, string title, float value)
        {
            Rect row = new Rect(334, y, 928, 69);
            Frame(row, Panel, audioChoiceV56 == index ? Sea : Line, 1); Region(row);
            Label(new Rect(row.x + 15, y + 12, 198, 38), title, _body);
            var slider = new Rect(576, y + 24, 540, 22);
            float next = GUI.HorizontalSlider(slider, value, 0f, 1f);
            Mark(slider);
            if (Mathf.Abs(next - value) > .004f) { audioChoiceV56 = index; SetAudioVolumeV56(index, next); }
            Label(new Rect(1140, y + 14, 108, 36), Mathf.RoundToInt(value * 100) + "%", _right);
        }
    }
}
