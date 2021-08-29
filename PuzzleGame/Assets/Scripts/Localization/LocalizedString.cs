using System;
using UnityEngine;

namespace PuzzleGame
{
    [Serializable]
    public class LocalizedString
    {
        [SerializeField] string[] _locMapping = new string[(int)ELanguageType._MAX];

        public static implicit operator string(LocalizedString locStr)
        {
            return locStr._locMapping[(int)GameContext.s_curLanguage];
        }
    }
}