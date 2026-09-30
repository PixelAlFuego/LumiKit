using LumiKit.Core;
using LumiKit.Demo;
using UnityEngine;

namespace LumiKit.Systems
{
    /// <summary>
    /// Sonido Select al elegir un objeto de la demo (LK-23). Escucha a ObjectSelector: Systems
    /// conoce a Demo y no al revés (GDD línea 1061).
    /// </summary>
    /// <remarks>
    /// Deseleccionar calla. Pasar de un objeto a otro suena. Volver a pulsar el mismo objeto no
    /// dispara OnSelectionChanged, así que tampoco suena.
    /// </remarks>
    [AddComponentMenu("LumiKit/UI Selection Sound")]
    [DisallowMultipleComponent]
    public class UISelectionSound : MonoBehaviour
    {
        [SerializeField] private ObjectSelector _selector;

        private void OnEnable()
        {
            if (_selector == null)
            {
                Debug.LogWarning($"[LumiKit] UISelectionSound en '{name}' no tiene ObjectSelector. No sonará.", this);
                return;
            }

            _selector.OnSelectionChanged += HandleSelectionChanged;
        }

        private void OnDisable()
        {
            if (_selector != null)
            {
                _selector.OnSelectionChanged -= HandleSelectionChanged;
            }
        }

        private void HandleSelectionChanged(EffectController selected)
        {
            if (selected == null || !UIAudioManager.HasInstance)
            {
                return;
            }

            UIAudioManager.Instance.Play(UISound.Select);
        }
    }
}
