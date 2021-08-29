using PuzzleGame.EventSystem;
using System.Collections;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;

namespace PuzzleGame
{
    [CreateAssetMenu(menuName = "PuzzleGame/Dialogue")]
    public class DialogueDef : ScriptableObject
    {
        public bool skippable = true;
        public string[] dialogues;
        public UltEvent onDialogueFinishEvents;

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
