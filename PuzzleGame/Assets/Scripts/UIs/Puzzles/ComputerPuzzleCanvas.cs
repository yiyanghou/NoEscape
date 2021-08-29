using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Canvas))]
    public class ComputerPuzzleCanvas : InspectionCanvas
    {
        [Serializable]
        public class PasswordMappingDef
        {
            public int roomIndex;
            public IntCharPair[] mapping;
        }
        [Serializable]
        public class IntCharPair
        {
            public int i;
            public char c;
        }

        [Header("Computer Puzzle Config")]
        [SerializeField] BoolVariable _isComputerOn;
        [SerializeField] PasswordMappingDef[] _mappings;
        [SerializeField] ComputerDesktop _desktop;
        [SerializeField] string _password = "NOESCAPE";
        string _encodedPassword;

        Dictionary<int, PasswordMappingDef> _roomIdx2Mapping;
        protected override void SetInspectable(Inspectable inspectable)
        {
            base.SetInspectable(inspectable);

            if(_roomIdx2Mapping.ContainsKey(inspectable.room.roomIndex))
            {
                _desktop.EncodeNumberTexts(_roomIdx2Mapping[inspectable.room.roomIndex]);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _roomIdx2Mapping = new Dictionary<int, PasswordMappingDef>();
            foreach(var mapping in _mappings)
            {
                _roomIdx2Mapping.Add(mapping.roomIndex, mapping);
            }

            Dictionary<char, int> allEncodings = new Dictionary<char, int>();
            foreach (var def in _mappings)
            {
                foreach (var map in def.mapping)
                {
                    allEncodings.Add(map.c, map.i);
                }
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < _password.Length; i++)
            {
                builder.Append(allEncodings[_password[i]].ToString());
            }
            _encodedPassword = builder.ToString();


            _desktop.passwordSubmitButton.onClick.AddListener(ValidatePassword);
        }

        public void ValidatePassword()
        {
            if(_desktop.passwordInput.text == _encodedPassword)
            {
                _inspectable.canInspect = false;
                StartCoroutine(_successRoutine());
            }
        }

        IEnumerator _successRoutine()
        {
            _desktop.OpenSuccessWindow();
            yield return new WaitForSeconds(1f);
            _desktop.CloseWindow();
            _isComputerOn.val = false;
            OnBackPressed();
        }
    }
}