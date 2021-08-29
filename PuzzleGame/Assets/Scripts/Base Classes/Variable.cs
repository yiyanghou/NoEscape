using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    public abstract class Variable : ScriptableObject { }

    public abstract class Variable<T> : Variable
    {
        public T defaultValue;
        private T _val;
        /// Current value of the variable
        public T val
        {
            get
            {
                return _val;
            }
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_val, value))
                {
                    _val = value;
                    valueChanged?.Invoke(value);
                }
            }
        }

        public event Action<T> valueChanged;
        public void Set(Variable<T> value)
        {
            val = value.val;
        }

        private void OnEnable()
        {
            Messenger.AddPersistentListener(M_EventType.ON_GAME_RESTART, Init);
            Init();
        }

        public static implicit operator T(Variable<T> variable)
        {
            if (variable == null)
            {
                return default(T);
            }
            return variable.val;
        }

        private void Init()
        {
            _val = defaultValue;
            valueChanged = null;
        }
    }
}