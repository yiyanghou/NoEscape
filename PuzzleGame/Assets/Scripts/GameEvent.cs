using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UltEvents;

namespace PuzzleGame.EventSystem
{
    #region Custom Unity Events
    /// <summary>
    /// Used in timeline
    /// </summary>
    [Serializable]
    public class DialogueEvent : UnityEvent<DialogueDef> { }
    #endregion

    #region Messenger Events
    [Serializable]
    public class RoomEventData : MessengerEventData
    {
        private RoomEventData() { }
        public RoomEventData(Room room)
        {
            this.room = room;
        }
        public Room room;
    }

    [Serializable]
    public class InventoryChangeEventData : MessengerEventData
    {
        private InventoryChangeEventData() { }
        public InventoryChangeEventData(InventoryItem itemIns, int slotIndex)
        {
            this.itemIns = itemIns;
            this.slotIndex = slotIndex;
        }

        public InventoryItem itemIns;
        public int slotIndex;
    }

    [Serializable]
    public class CutSceneEventData : MessengerEventData
    {
        private CutSceneEventData() { }
        public CutSceneEventData(TimelineAsset cutScene)
        {
            this.cutScene = cutScene;
        }
        public TimelineAsset cutScene;
    }

    [Serializable]
    public class DialogueEventData : MessengerEventData
    {
        private DialogueEventData() { }
        public DialogueEventData(DialogueDef dialogue)
        {
            this.dialogue = dialogue;
        }
        public DialogueDef dialogue;
    }

    [Serializable]
    public class InspectionEventData : MessengerEventData
    {
        private InspectionEventData() { }
        public InspectionEventData(Inspectable inspectable)
        {
            this.inspectable = inspectable;
        }
        public Inspectable inspectable;
    }

    public class PlayerControlEventData : MessengerEventData
    {
        private PlayerControlEventData() { }
        public PlayerControlEventData(bool enable)
        {
            this.enable = enable;
        }
        public bool enable;
    }

    public class GameEndEventData : MessengerEventData
    {
        private GameEndEventData() { }
        public GameEndEventData(EGameEndingType type)
        {
            this.type = type;
        }
        public EGameEndingType type;
    }

    public enum M_EventType
    {
        ON_BEFORE_ENTER_ROOM, //RoomEventData //ON_BEFORE_ENTER_ROOM will trigger ON_ENTER_ROOM
        ON_ENTER_ROOM, //RoomEventData

        ON_INVENTORY_CHANGE, //InventoryChangeEventData
        ON_CUTSCENE_START, //CutSceneEventData
        ON_CUTSCENE_END, //CutSceneEventData
        ON_CHANGE_PLAYER_CONTROL,  //PlayerControlEventData

        ON_GAME_PAUSED,
        ON_GAME_RESUMED,

        ON_GAME_RESTART,
        ON_GAME_START,
        ON_GAME_END,

        ON_INSPECTION_START,
        ON_INSPECTION_END
    }
    #endregion
}