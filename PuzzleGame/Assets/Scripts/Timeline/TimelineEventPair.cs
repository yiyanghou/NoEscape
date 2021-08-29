using UltEvents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace PuzzleGame
{
    [Serializable]
    public class TimelineEventPair
    {
        public SignalAsset signalAsset;
        public UltEvent events;
    }
}

