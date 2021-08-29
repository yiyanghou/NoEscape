using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UltEvents;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    [Serializable]
    public class PromptOptionDesc
    {
        public string optionName;
        public UltEvent optionEvents;
    }

    [CreateAssetMenu(menuName = "PuzzleGame/Prompt")]
    public class PromptDef : ScriptableObject
    {
        public string title = "Message";
        [TextArea]
        public string prompt = "Message Content";
        public Sprite promptImage;
        public PromptOptionDesc[] options;
        public bool hasBackButton = true;
        public bool skippable = true;
        public string backButtonName = "OK";
        [Tooltip("if not null, this will be used as the popup sound")]
        public AudioClip popUpSoundOverride = null;

        //runtime
        public bool hasPlayed { get; set; }
        private void OnEnable()
        {
            Messenger.AddPersistentListener(M_EventType.ON_GAME_RESTART, Init);
            Init();
        }
        private void Init()
        {
            hasPlayed = false;
        }
    }
}
