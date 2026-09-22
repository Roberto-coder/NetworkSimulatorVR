using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Systems.Settings
{
    public sealed class GlobalSettingsManager : MonoBehaviour
    {
        private const string SettingsKey = "global.userSettings";
        private const float MutedDecibels = -80f;

        [SerializeField] private AudioMixer audioMixer;

        public static GlobalSettingsManager Instance { get; private set; }
        public UserSettings Current { get; private set; }
        public event Action<UserSettings> SettingsChanged;

        private readonly List<Slider> boundMusicSliders = new();
        private readonly List<Slider> boundVoiceSliders = new();
        private AudioMixerGroup voiceGroup;

        public void RouteVoice(AudioSource source)
        {
            if (source == null || audioMixer == null) return;
            if (voiceGroup == null)
            {
                foreach (var group in audioMixer.FindMatchingGroups("Voice"))
                    if (group.name == "Voice") { voiceGroup = group; break; }
            }
            if (voiceGroup != null) source.outputAudioMixerGroup = voiceGroup;
        }
        private GameObject persistentRoot;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        private void Awake()
        {
            persistentRoot = transform.parent != null &&
                (transform.parent.name == "GlobalSystems" || transform.parent.name == "GlobalSystems(Clone)")
                ? transform.parent.gameObject
                : gameObject;

            if (Instance != null && Instance != this)
            {
                persistentRoot.SetActive(false);
                Destroy(persistentRoot);
                return;
            }

            Instance = this;
            // El prefab puede estar organizado bajo GameManager en la escena de inicio.
            // Se desacopla para persistir únicamente GlobalSystems y no todo su padre.
            persistentRoot.transform.SetParent(null);
            DontDestroyOnLoad(persistentRoot);
            Load();
        }

        private void OnEnable()
        {
            if (Instance != this) return;
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void Start()
        {
            if (Instance != this) return;
            // El mixer debe recibir sus parámetros después de Awake/OnEnable.
            ApplyAll();
            BindSettingsPanel(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            UnbindMusicSlider();
            UnbindVoiceSliders();
        }

        public void SetMusicVolume(float value)
        {
            Current.musicVolume = Mathf.Clamp01(value);
            Current.musicEnabled = Current.musicVolume > 0.0001f;
            foreach (Slider slider in boundMusicSliders)
            {
                if (slider != null && !Mathf.Approximately(slider.value, Current.musicVolume))
                    slider.SetValueWithoutNotify(Current.musicVolume);
            }
            ApplyMusicVolume();
            SaveAndNotify();
        }

        public void SetMusicEnabled(bool enabled)
        {
            Current.musicEnabled = enabled;
            ApplyMusicVolume();
            SaveAndNotify();
        }

        public void SetMasterVolume(float value)
        {
            Current.masterVolume = Mathf.Clamp01(value);
            SetMixerVolume("MasterVolume", Current.masterVolume);
            SaveAndNotify();
        }

        public void SetVoiceVolume(float value)
        {
            Current.voiceVolume = Mathf.Clamp01(value);
            foreach (Slider slider in boundVoiceSliders)
                if (slider != null) slider.SetValueWithoutNotify(Current.voiceVolume);
            SetMixerVolume("VoiceVolume", Current.voiceVolume);
            SaveAndNotify();
        }

        public void SetSfxVolume(float value)
        {
            Current.sfxVolume = Mathf.Clamp01(value);
            SetMixerVolume("SFXVolume", Current.sfxVolume);
            SaveAndNotify();
        }

        public void SetUiVolume(float value)
        {
            Current.uiVolume = Mathf.Clamp01(value);
            SetMixerVolume("UIVolume", Current.uiVolume);
            SaveAndNotify();
        }

        private void Load()
        {
            string json = PlayerPrefs.GetString(SettingsKey, string.Empty);
            Current = string.IsNullOrWhiteSpace(json)
                ? new UserSettings()
                : JsonUtility.FromJson<UserSettings>(json) ?? new UserSettings();
        }

        private void ApplyAll()
        {
            SetMixerVolume("MasterVolume", Current.masterVolume);
            ApplyMusicVolume();
            SetMixerVolume("VoiceVolume", Current.voiceVolume);
            SetMixerVolume("SFXVolume", Current.sfxVolume);
            SetMixerVolume("UIVolume", Current.uiVolume);
        }

        private void ApplyMusicVolume()
        {
            SetMixerVolume("MusicVolume", Current.musicEnabled ? Current.musicVolume : 0f);
        }

        private void SetMixerVolume(string parameter, float normalizedValue)
        {
            if (audioMixer == null)
                return;

            float decibels = normalizedValue <= 0.0001f
                ? MutedDecibels
                : Mathf.Log10(Mathf.Clamp01(normalizedValue)) * 20f;
            audioMixer.SetFloat(parameter, decibels);
        }

        private void SaveAndNotify()
        {
            PlayerPrefs.SetString(SettingsKey, JsonUtility.ToJson(Current));
            PlayerPrefs.Save();
            SettingsChanged?.Invoke(Current);
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode _)
        {
            BindSettingsPanel(scene);
        }

        private void BindSettingsPanel(Scene scene)
        {
            UnbindMusicSlider();
            UnbindVoiceSliders();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (Slider slider in root.GetComponentsInChildren<Slider>(true))
                {
                    if (slider.name == "SliderVoice") BindVoiceSlider(slider);
                    if (slider.name != "SliderMusic")
                        continue;

                    BindMusicSlider(slider);
                }
            }
        }

        public void BindVoiceSlider(Slider slider)
        {
            if (slider == null || Current == null) return;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(Current.voiceVolume);
            if (boundVoiceSliders.Contains(slider)) return;
            slider.onValueChanged.AddListener(SetVoiceVolume);
            boundVoiceSliders.Add(slider);
        }

        public void UnbindVoiceSlider(Slider slider)
        {
            if (slider == null) return;
            slider.onValueChanged.RemoveListener(SetVoiceVolume);
            boundVoiceSliders.Remove(slider);
        }

        private void UnbindVoiceSliders()
        {
            foreach (Slider slider in boundVoiceSliders)
                if (slider != null) slider.onValueChanged.RemoveListener(SetVoiceVolume);
            boundVoiceSliders.Clear();
        }

        public void BindMusicSlider(Slider slider)
        {
            if (slider == null || Current == null) return;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.SetValueWithoutNotify(Current.musicVolume);
            if (boundMusicSliders.Contains(slider)) return;
            slider.onValueChanged.AddListener(SetMusicVolume);
            boundMusicSliders.Add(slider);
        }

        public void UnbindMusicSlider(Slider slider)
        {
            if (slider == null) return;
            slider.onValueChanged.RemoveListener(SetMusicVolume);
            boundMusicSliders.Remove(slider);
        }

        private void UnbindMusicSlider()
        {
            foreach (Slider slider in boundMusicSliders)
            {
                if (slider != null)
                    slider.onValueChanged.RemoveListener(SetMusicVolume);
            }
            boundMusicSliders.Clear();
        }
    }
}
