using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using PuzzleGame.UI;

namespace PuzzleGame
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class ComputerWindow : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] Sprite _focusedPanel, _unfocusedPanel;
        [SerializeField] Button[] _closeButtons;
        [SerializeField] AudioClip _windowOpenSound;

        public RectTransform rect { get; private set; }
        ComputerDesktop _desktop;
        Image _img;
        bool _hasFocus;

        private void Awake()
        {
            _img = GetComponent<Image>();
            rect = GetComponent<RectTransform>();
            _hasFocus = false;
        }

        public void Init(ComputerDesktop canvas)
        {
            _desktop = canvas;

            foreach (var button in _closeButtons)
            {
                button.onClick.AddListener(OnBackPressed);
            }
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            _desktop.OpenOrFocusWindow(this);
        }

        public void OnEnterMenu()
        {
            if (_windowOpenSound)
                GameActions.PlaySounds(_windowOpenSound);

            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            _hasFocus = true;
        }

        public void OnLeaveMenu()
        {
            gameObject.SetActive(false);
        }

        public void OnBackPressed()
        {
            if(_hasFocus)
            {
                _desktop.CloseWindow();
            }
            else
            {
                _desktop.OpenOrFocusWindow(this);
            }
        }

        public void OnFocus()
        {
            _img.sprite = _focusedPanel;
            transform.SetAsLastSibling();
            _hasFocus = true;
        }

        public void OnLoseFocus()
        {
            _img.sprite = _unfocusedPanel;
            _hasFocus = false;
        }
    }
}
