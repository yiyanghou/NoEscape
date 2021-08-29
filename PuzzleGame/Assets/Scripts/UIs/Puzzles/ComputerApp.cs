using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Image))]
    public class ComputerApp : MonoBehaviour, 
        IPointerClickHandler
    {
        [SerializeField] Color _iconSelectionColor;
        [SerializeField] Color _nameBackgroundColor;
        [SerializeField] Image _appNameBackgroundImg;
        [SerializeField] ComputerWindow _appWindow;
        [SerializeField] ComputerRightClickMenu _rightClickMenu;

        Image _iconImg;

        ComputerDesktop _desktop;
        Color _originalIconColor;
        Color _originalNameColor;

        public void Init(ComputerDesktop canvas)
        {
            _desktop = canvas;
        }
        public void Select()
        {
            _iconImg.color = _iconSelectionColor;
            _appNameBackgroundImg.color = _nameBackgroundColor;
        }
        public void Deselect()
        {
            _iconImg.color = _originalIconColor;
            _appNameBackgroundImg.color = _originalNameColor;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            switch(eventData.clickCount)
            {
                case 1:
                    _desktop.SetCurrentSelectedApp(this);

                    if (eventData.button == PointerEventData.InputButton.Right)
                    {
                        _desktop.OpenRightClickMenu(_rightClickMenu, eventData.position, this);
                    }

                    break;
                case 2:
                    _desktop.SetCurrentSelectedApp(this);

                    if (eventData.button == PointerEventData.InputButton.Left)
                    {
                        _desktop.OpenOrFocusWindow(_appWindow);
                    }

                    break;
                default:
                    break;
            }
        }

        void Awake()
        {
            _iconImg = GetComponent<Image>();
            _originalIconColor = _iconImg.color;
            _originalNameColor = _appNameBackgroundImg.color;
        }
    }

}
