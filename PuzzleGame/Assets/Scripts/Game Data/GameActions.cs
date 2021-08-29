using PuzzleGame.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace PuzzleGame
{
    /// <summary>
    /// contains wrapper functions around game classes to provide game logic building blocks
    /// </summary>
    public static class GameActions
    {
        public static void EnterPainting(Condition condition)
        {
            if (condition && !condition.Evaluate())
            {
                return;
            }
            
            Room curRoom = GameContext.s_gameMgr.curRoom;

            if (curRoom.roomIndex == GameConst.k_maxRoomIndex - 1)
            {
                DialogueMenu.Instance.DisplaySimplePrompt("Message", "I am too large to fit into this room", null, "Ok then");
            }
            else
            {
                curRoom.GoToNext();
            }
        }

        public static void RotatePainting(bool clockwise)
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;
            curRoom.RotateNext(clockwise);
        }
        public static void AddToInventory(InventoryItemDef inventoryItem, float scale, int quantity, bool useGlobalScale)
        {
            Debug.Assert(GameContext.s_player);
            if(!useGlobalScale)
                GameContext.s_player.AddToInventory(inventoryItem, quantity, scale, GameContext.s_gameMgr.curRoom);
            else
                GameContext.s_player.AddToInventory(inventoryItem, quantity, scale);
        }
        public static void DisplayDialogue(DialogueDef dialogue)
        {
            Debug.Assert(DialogueMenu.Instance);
            DialogueMenu.Instance.DisplayDialogue(dialogue);
        }
        public static void DisplayDialogue(BoolVariable condition, DialogueDef dialogue)
        {
            DisplayDialogue(condition.val, dialogue);
        }
        public static void DisplayDialogue(Condition condition, DialogueDef dialogue)
        {
            DisplayDialogue(condition == null || condition.Evaluate(), dialogue);
        }
        public static void DisplayDialogue(bool condition, DialogueDef dialogue)
        {
            if (condition)
            {
                DisplayDialogue(dialogue);
            }
        }
        public static void DisplayPrompt(PromptDef prompt)
        {
            Debug.Assert(DialogueMenu.Instance);
            DialogueMenu.Instance.DisplayPrompt(prompt);
        }
        public static void DisplayPrompt(BoolVariable condition, PromptDef prompt)
        {
            DisplayPrompt(condition.val, prompt);
        }
        public static void DisplayPrompt(Condition condition, PromptDef prompt)
        {
            DisplayPrompt(condition == null || condition.Evaluate(), prompt);
        }
        public static void DisplayPrompt(bool condition, PromptDef prompt)
        {
            if (condition)
            {
                DisplayPrompt(prompt);
            }
        }
        public static void ClosePrompt(BoolVariable condition)
        {
            ClosePrompt(condition.val);
        }
        public static void ClosePrompt(bool condition)
        {
            if(condition)
            {
                ClosePrompt();
            }
        }
        public static void ClosePrompt()
        {
            if (ReferenceEquals(GameContext.s_UIMgr.GetActiveMenu(), DialogueMenu.Instance))
            {
                DialogueMenu.Instance.ClosePrompt();
            }
        }
        public static void PlayCutscene(BoolVariable condition, TimelineAsset timeline)
        {
            PlayCutscene(condition.val, timeline);
        }
        public static void PlayCutscene(Condition condition, TimelineAsset timeline)
        {
            PlayCutscene(condition == null || condition.Evaluate(), timeline);
        }
        public static void PlayCutscene(bool condition, TimelineAsset timeline)
        {
            if (GameContext.s_gameMgr && condition)
            {
                GameContext.s_gameMgr.curRoom.PlayCutScene(timeline);
            }
        }
        public static void SetBoolean(BoolVariable variable, bool value)
        {
            variable.val = value;
        }
        public static bool IsInRoom(int index)
        {
            if(GameContext.s_gameMgr)
            {
                return GameContext.s_gameMgr.curRoom.roomIndex == index;
            }
            return false;
        }
        public static bool HasPlayed(DialogueDef dialogue)
        {
            return dialogue.hasPlayed;
        }
        public static bool HasPlayed(PromptDef prompt)
        {
            return prompt.hasPlayed;
        }
        public static void TriggerEnding(EGameEndingType type)
        {
            if (GameContext.s_gameMgr)
            {
                GameContext.s_gameMgr.TriggerEnding(type);
            }
        }
        public static void PlaySounds(AudioCollection collection)
        {
            if (collection && GameContext.s_audioMgr)
            {
                GameContext.s_audioMgr.PlayOneShotSound(collection.GetClip(), GameContext.s_audioMgr.transform.position, 1);
            }
        }
        public static void PlaySounds(AudioClip clip)
        {
            if (clip && GameContext.s_audioMgr)
            {
                GameContext.s_audioMgr.PlayOneShotSound(clip, GameContext.s_audioMgr.transform.position, 1);
            }
        }
        #region Beta
        public static void DespawnPlayer()
        {
            GameContext.s_player.gameObject.SetActive(false);
        }

        public static bool DoorInteraction(InventoryItemDef keyDef,
            PromptDef playerTooLargePrompt,
            PromptDef playerTooSmallPrompt,
            PromptDef noKeyPrompt, 
            PromptDef insufficientQuantityPrompt)
        {
            if(GameContext.s_gameMgr.curRoom.roomIndex > GameConst.k_startingRoomIndex)
            {
                DialogueMenu.Instance.DisplayPrompt(playerTooLargePrompt);
                return false;
            }
            else if(GameContext.s_gameMgr.curRoom.roomIndex < GameConst.k_startingRoomIndex)
            {
                DialogueMenu.Instance.DisplayPrompt(playerTooSmallPrompt);
                return false;
            }

            bool hasKey = false;
            int quantity = 0;
            var inventory = GameContext.s_player.inventory;
            foreach(var item in inventory)
            {
                if(item != null && ReferenceEquals(item.def, keyDef))
                {
                    hasKey = true;
                    quantity += item.quantity;
                }
            }

            if(!hasKey)
            {
                DialogueMenu.Instance.DisplayPrompt(noKeyPrompt);
            }
            else
            {
                if (quantity < 3)
                {
                    DialogueMenu.Instance.DisplayPrompt(insufficientQuantityPrompt);
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
