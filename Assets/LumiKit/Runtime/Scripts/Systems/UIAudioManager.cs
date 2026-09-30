using LumiKit.Utils;
using UnityEngine;

namespace LumiKit.Systems
{
    /// <summary>Sonidos de interfaz que reproduce UIAudioManager (GDD §4.7).</summary>
    public enum UISound
    {
        Hover,
        Click,
        Select
    }

    /// <summary>
    /// Manager persistente del audio de interfaz (LK-23): los sonidos cortos por el grupo UI de
    /// AMX_LumiKit y la música en bucle por el grupo Music.
    /// </summary>
    /// <remarks>
    /// Las dos AudioSource, sus grupos y el clip de música los configura UIAudioBuilder en el
    /// prefab PRF_UIAudioManager. Aquí sólo se reproducen.
    /// Sobrevive al cambio de escena: el duplicado que traiga la escena nueva lo destruye la base
    /// y nunca llama a Play, así que la música ni se corta ni se reinicia. DontDestroyOnLoad exige
    /// que el objeto esté en la raíz de la escena.
    /// </remarks>
    [AddComponentMenu("LumiKit/UI Audio Manager")]
    [DisallowMultipleComponent]
    public class UIAudioManager : Singleton<UIAudioManager>
    {
        /// <summary>
        /// Separación mínima entre dos Hover, en segundos. Es el tope de duración del clip
        /// (spec LK-23): dos no se solapan y recorrer las opciones del enum no suena a ráfaga.
        /// </summary>
        private const float HOVER_MIN_INTERVAL = 0.15f;

        [Tooltip("Fuente de los sonidos cortos. Salida: grupo UI de AMX_LumiKit.")]
        [SerializeField] private AudioSource _sfxSource;

        [Tooltip("Fuente de la música, en bucle. Salida: grupo Music de AMX_LumiKit.")]
        [SerializeField] private AudioSource _musicSource;

        [SerializeField] private AudioClip _hoverClip;
        [SerializeField] private AudioClip _clickClip;
        [SerializeField] private AudioClip _selectClip;

        private float _lastHoverTime = float.NegativeInfinity;

        // Un bit por UISound: el aviso de clip nulo sale una sola vez por sonido.
        private int _warnedMissingClips;

        private bool _warnedMissingSource;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                // La base ya destruyó este duplicado.
                return;
            }

            DontDestroyOnLoad(gameObject);
            StartMusic();
        }

        /// <summary>Reproduce un sonido de interfaz por el grupo UI.</summary>
        public void Play(UISound sound)
        {
            if (sound == UISound.Hover)
            {
                float now = Time.unscaledTime;
                if (now - _lastHoverTime < HOVER_MIN_INTERVAL)
                {
                    return;
                }

                _lastHoverTime = now;
            }

            if (_sfxSource == null)
            {
                if (!_warnedMissingSource)
                {
                    _warnedMissingSource = true;
                    Debug.LogWarning($"[LumiKit] UIAudioManager en '{name}' no tiene fuente de SFX. No suena nada.", this);
                }

                return;
            }

            AudioClip clip = GetClip(sound);
            if (clip == null)
            {
                int bit = 1 << (int)sound;
                if ((_warnedMissingClips & bit) == 0)
                {
                    _warnedMissingClips |= bit;
                    Debug.LogWarning($"[LumiKit] UIAudioManager en '{name}' no tiene clip para {sound}.", this);
                }

                return;
            }

            _sfxSource.PlayOneShot(clip);
        }

        private void StartMusic()
        {
            if (_musicSource == null || _musicSource.clip == null)
            {
                Debug.LogWarning($"[LumiKit] UIAudioManager en '{name}' no tiene música asignada. Sin música.", this);
                return;
            }

            if (!_musicSource.isPlaying)
            {
                _musicSource.Play();
            }
        }

        private AudioClip GetClip(UISound sound)
        {
            switch (sound)
            {
                case UISound.Hover:
                    return _hoverClip;
                case UISound.Click:
                    return _clickClip;
                case UISound.Select:
                    return _selectClip;
                default:
                    return null;
            }
        }
    }
}
