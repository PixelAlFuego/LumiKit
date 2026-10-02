using UnityEngine;
using UnityEngine.SceneManagement;

namespace LumiKit.Systems
{
    /// <summary>
    /// Carga de escenas por nombre, asíncrona, con el sonido de transición (LK-17). También cierra
    /// la aplicación. Estática: la llama cualquier botón sin buscar un objeto en la escena.
    /// </summary>
    /// <remarks>
    /// El sonido va por UIAudioManager, que sobrevive al cambio de escena: no se corta al cargar.
    /// Sin pantalla de carga ni GameStateManager del GDD (§4.4): fuera del MVP.
    /// </remarks>
    public static class SceneLoader
    {
        private static bool _isLoading;

        /// <summary>True mientras hay una carga en curso; otra petición se ignora.</summary>
        public static bool IsLoading => _isLoading;

        /// <summary>
        /// Con el recargado de dominio desactivado, los estáticos sobreviven entre dos Play.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _isLoading = false;
        }

        /// <summary>
        /// Carga la escena en modo Single. Devuelve false si no la carga: otra carga en curso o
        /// escena fuera de la lista del build.
        /// </summary>
        public static bool Load(string sceneName)
        {
            if (_isLoading)
            {
                return false;
            }

            if (string.IsNullOrEmpty(sceneName) || !Application.CanStreamedLevelBeLoaded(sceneName))
            {
                Debug.LogError(
                    $"[LumiKit] La escena '{sceneName}' no está en la lista del build (Build Profiles). No se carga.");
                return false;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError($"[LumiKit] No se pudo empezar a cargar '{sceneName}'.");
                return false;
            }

            _isLoading = true;
            operation.completed += OnLoadCompleted;

            if (UIAudioManager.HasInstance)
            {
                UIAudioManager.Instance.Play(UISound.Transition);
            }

            return true;
        }

        /// <summary>
        /// Cierra la aplicación. En el editor Application.Quit no hace nada: se prueba en el build.
        /// </summary>
        public static void Quit()
        {
            Application.Quit();
        }

        private static void OnLoadCompleted(AsyncOperation operation)
        {
            _isLoading = false;
        }
    }
}
