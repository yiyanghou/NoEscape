using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using System;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    [Serializable]
    public class DialogueEventEmitterPair
    {
        public SignalAsset signalAsset;
        public DialogueEvent dialogueEvent;
    }
}

