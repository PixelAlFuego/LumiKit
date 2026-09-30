using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LumiKit.Systems
{
    /// <summary>
    /// Hover y Click de un control de interfaz (LK-23). Va en el mismo GameObject que el
    /// Selectable: ExecuteEvents entrega el evento a todos los componentes del objeto, así que
    /// suena sin tocar el Button, el Toggle ni el widget.
    /// </summary>
    /// <remarks>
    /// Sliders y campo del valor llevan _playClick en false: arrastrar un Slider puede disparar
    /// OnPointerClick. Calla si el control no es interactuable o si no hay UIAudioManager.
    /// </remarks>
    [AddComponentMenu("LumiKit/UI Sound Trigger")]
    [DisallowMultipleComponent]
    public class UISoundTrigger : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, ISubmitHandler
    {
        [Tooltip("Si suena Click al pulsar con el botón izquierdo o con Submit. Hover suena siempre.")]
        [SerializeField] private bool _playClick = true;

        private Selectable _selectable;

        private void Awake()
        {
            _selectable = GetComponent<Selectable>();
            if (_selectable == null)
            {
                Debug.LogWarning($"[LumiKit] UISoundTrigger en '{name}' no tiene un Selectable al lado. No sonará.", this);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Arrastrar el canal R por encima de G y B no suena.
            if (eventData.dragging)
            {
                return;
            }

            Play(UISound.Hover);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Como Button y Toggle: sólo el botón izquierdo.
            if (!_playClick || eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            Play(UISound.Click);
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!_playClick)
            {
                return;
            }

            Play(UISound.Click);
        }

        private void Play(UISound sound)
        {
            if (_selectable == null || !_selectable.IsInteractable() || !UIAudioManager.HasInstance)
            {
                return;
            }

            UIAudioManager.Instance.Play(sound);
        }
    }
}
