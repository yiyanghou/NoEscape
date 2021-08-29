using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UltEvents;

namespace PuzzleGame.UI
{
    public class GameButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] AudioClip _clickSound;
        [SerializeField] UltEvent _additionalOnClickEvents;
        [SerializeField] UltEvent _pointerEnterEvents, _pointerExitEvents;

        public UltEvent onPointerEnter { get => _pointerEnterEvents; set => _pointerEnterEvents = value; }
        public UltEvent onPointerExit { get => _pointerExitEvents; set => _pointerExitEvents = value; }
        public UltEvent onClick { get => _additionalOnClickEvents; set => _additionalOnClickEvents = value; }


        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(_clickSound)
                GameActions.PlaySounds(_clickSound);
            _additionalOnClickEvents?.Invoke();
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _pointerEnterEvents?.Invoke();
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _pointerExitEvents?.Invoke();
        }
    }
}