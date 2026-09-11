using UnityEngine;

namespace LumiKit.Utils
{
    /// <summary>
    /// Base genérica para los managers persistentes descritos en GDD §4.4.
    /// </summary>
    /// <remarks>
    /// No llama a DontDestroyOnLoad: lo decide cada subclase, porque no todos los
    /// managers deben sobrevivir al cambio de escena. Tampoco crea la instancia por su
    /// cuenta: el objeto vive en una escena o en un prefab de Prefabs/Systems/.
    /// Las subclases que sobrescriban Awake u OnDestroy deben llamar a base.
    /// </remarks>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;

        public static T Instance => _instance;

        public static bool HasInstance => _instance != null;

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning(
                    $"[LumiKit] Ya existe una instancia de {typeof(T).Name}. Se destruye la duplicada en '{name}'.",
                    this);
                Destroy(gameObject);
                return;
            }

            _instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
