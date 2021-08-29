using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.EventSystem;
using PuzzleGame.UI;
namespace PuzzleGame
{
    public class TutorialManager : MonoBehaviour
    {
        [Serializable]
        class PromptInfo
        {
            public Condition condition;
            public PromptDef prompt;
        }

        [Serializable]
        class DialogueInfo
        {
            public Condition condition;
            public DialogueDef dialogue;
        }

        [SerializeField] PromptInfo _inspectionPrompt;
        [SerializeField] DialogueInfo _roomConnectionDialogue;

        private void Awake()
        {
            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener<InspectionEventData>(M_EventType.ON_INSPECTION_START, OnInspectionStart);
        }

        private void PlayPrompt(PromptInfo info)
        {
            if (info.prompt.hasPlayed)
                return;

            if(info.condition == null || info.condition.Evaluate())
            {
                DialogueMenu.Instance.DisplayPrompt(info.prompt);
            }
        }
        private void PlayDialogue(DialogueInfo info)
        {
            if (info.dialogue.hasPlayed)
                return;

            if (info.condition == null || info.condition.Evaluate())
            {
                DialogueMenu.Instance.DisplayDialogue(info.dialogue);
            }
        }

        private void OnInspectionStart(InspectionEventData data)
        {
            PlayPrompt(_inspectionPrompt);
        }

        private void OnEnterRoom(RoomEventData data)
        {
            PlayDialogue(_roomConnectionDialogue);
        }
    }
}
