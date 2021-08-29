using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using UltEvents;

namespace PuzzleGame.UI
{
    [Serializable]
    public class ButtonDesc
    {
        public ButtonDesc(string text, UltEvent onClick)
        {
            this.text = text;
            this.onClick = onClick;
        }
        public string text;
        public UltEvent onClick;
    }

    /// <summary>
    /// A screen which is just a bunch of buttons that can be configured and allocated
    /// </summary>
    public class ButtonPoolScreen : SingletonBehavior<ButtonPoolScreen>
    {
        [SerializeField] VerticalLayoutGroup _verticalLayout;
        Button[] _buttons;

        protected override void Awake()
        {
            base.Awake();

            _buttons = _verticalLayout.GetComponentsInChildren<Button>();
            gameObject.SetActive(false);
        }

        public static void Open(ButtonDesc[] options)
        {
            Instance.Configure(options);
            Instance.gameObject.SetActive(true);
        }

        public static void Close()
        {
            Instance.gameObject.SetActive(false);
        }

        public void Configure(ButtonDesc[] options)
        {
            Debug.Assert(_buttons.Length >= options.Length);

            int i;
            for(i=0; i< options.Length; i++)
            {
                _buttons[i].gameObject.SetActive(true);
                _buttons[i].GetComponentInChildren<Text>().text = options[i].text;
                _buttons[i].onClick = new Button.ButtonClickedEvent();
                _buttons[i].onClick.AddListener(() => { options[i].onClick?.Invoke(); });
            }

            for(; i<_buttons.Length; i++)
            {
                _buttons[i].gameObject.SetActive(false);
            }
        }
    }
}