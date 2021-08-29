using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class ComputerDesktop : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] ComputerWindow _dummyWindow;
        [SerializeField] ComputerWindow _successWindow;

        [SerializeField] Button _passwordSubmitButton;
        [SerializeField] TMP_InputField _passwordInput;
        public Button passwordSubmitButton => _passwordSubmitButton;
        public TMP_InputField passwordInput => _passwordInput;

        LinkedList<ComputerWindow> _windowStack;

        ComputerRightClickMenu _curRightClickWindow;
        ComputerApp _curSelectedApp;

        RectTransform _rect;

        ComputerNumberText[] _numberTexts;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            _numberTexts = GetComponentsInChildren<ComputerNumberText>();
        }

        void Start()
        {
            _windowStack = new LinkedList<ComputerWindow>();
            _curRightClickWindow = null;

            var windows = GetComponentsInChildren<ComputerWindow>();
            var apps = GetComponentsInChildren<ComputerApp>();
            var rightClickMenus = GetComponentsInChildren<ComputerRightClickMenu>();

            foreach (var win in windows)
            {
                win.Init(this);
                win.gameObject.SetActive(false);
            }

            foreach (var app in apps)
            {
                app.Init(this);
            }

            foreach (var rightClickMenu in rightClickMenus)
            {
                rightClickMenu.gameObject.SetActive(false);
            }
        }

        public void EncodeNumberTexts(ComputerPuzzleCanvas.PasswordMappingDef def)
        {
            foreach(var numText in _numberTexts)
            {
                numText.EncodeText(def);
            }
        }

        public void CloseWindow()
        {
            if (_windowStack.Count > 0)
            {
                _windowStack.First.Value.OnLeaveMenu();
                _windowStack.RemoveFirst();
            }
        }

        public void OpenOrFocusWindow(ComputerWindow window)
        {
            CloseRightClickMenu();

            bool isFocus = false;
            for (var node = _windowStack.First; node != null; node = node.Next)
            {
                if (ReferenceEquals(window, node.Value))
                {
                    _windowStack.Remove(node);
                    isFocus = true;
                    break;
                }
            }

            if (_windowStack.Count > 0)
                _windowStack.First.Value.OnLoseFocus();

            _windowStack.AddFirst(window);

            if (isFocus)
            {
                window.OnFocus();
            }
            else
            {
                window.OnEnterMenu();
            }

            FixMenuPosition(window.rect);
        }

        public void OpenRightClickMenu(ComputerRightClickMenu rightClickWindow, Vector2 screenPos, ComputerApp context = null)
        {
            if (_curRightClickWindow && !ReferenceEquals(_curRightClickWindow, rightClickWindow))
            {
                CloseRightClickMenu();
            }

            _curRightClickWindow = rightClickWindow;
            _curRightClickWindow.rect.position = screenPos;
            _curRightClickWindow.OnEnterMenu(context);

            FixMenuPosition(_curRightClickWindow.rect);
        }

        public void CloseRightClickMenu()
        {
            if (_curRightClickWindow)
            {
                _curRightClickWindow.OnLeaveMenu();
                _curRightClickWindow = null;
            }
        }

        public void OpenDummyWindow()
        {
            OpenOrFocusWindow(_dummyWindow);
        }
        public void OpenSuccessWindow()
        {
            OpenOrFocusWindow(_successWindow);
        }
        public void SetCurrentSelectedApp(ComputerApp app)
        {
            if (_curSelectedApp && !ReferenceEquals(app, _curSelectedApp))
            {
                _curSelectedApp.Deselect();
            }

            _curSelectedApp = app;
            app.Select();
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            var hit = eventData.pointerCurrentRaycast;
            if (hit.isValid)
            {
                ComputerApp app = hit.gameObject.GetComponent<ComputerApp>();
                ComputerRightClickMenu menu = hit.gameObject.GetComponent<ComputerRightClickMenu>();

                if (!app)
                {
                    if(_curSelectedApp)
                    {
                        _curSelectedApp.Deselect();
                    }
                }

                if(!menu)
                {
                    CloseRightClickMenu();
                }
            }
        }

        
        void FixMenuPosition(RectTransform menuRect)
        {
            Vector2 offset1 = menuRect.anchoredPosition - menuRect.offsetMin;
            Vector2 offset2 = menuRect.anchoredPosition - menuRect.offsetMax;

            Rect bound = _rect.rect;
            Vector2 min = _rect.TransformPoint(-bound.size / 2 + offset1);
            Vector2 max = _rect.TransformPoint(bound.size / 2 - offset2);

            menuRect.position = new Vector2(
                    Mathf.Clamp(menuRect.position.x, min.x, max.x),
                    Mathf.Clamp(menuRect.position.y, min.y, max.y)
                );
        }
    }
}
