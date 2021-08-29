using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(RectTransform))]
    public class ComputerRightClickMenu : MonoBehaviour
    {
        public RectTransform rect { get; private set; }
        public ButtonDesc[] _buttonConfig;

        GameButton[] _buttons;
        ComputerApp _target;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            _buttons = GetComponentsInChildren<GameButton>();

            Debug.Assert(_buttonConfig.Length == _buttons.Length);
            
            for(int i=0; i<_buttons.Length; i++)
            {
                string text = _buttonConfig[i].text;
                
                if(text.Length > 0)
                {
                    _buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = $"<u>{text[0]}</u>" + text.Substring(1);
                }
                else
                {
                    _buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                }

                _buttons[i].onClick = _buttonConfig[i].onClick;
                _buttons[i].onClick.AddPersistentCall((System.Action)OnLeaveMenu);
            }
        }

        public void OnEnterMenu(ComputerApp context = null)
        {
            _target = context;
            gameObject.SetActive(true);
        }

        public void OnLeaveMenu()
        {
            foreach(var button in _buttons)
            {
                button.onPointerExit?.Invoke();
            }

            if(_target)
            {
                _target.Deselect();
                _target = null;
            }

            gameObject.SetActive(false);
        }
    }
}
