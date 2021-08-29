using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PuzzleGame.UI;
using UltEvents;
using System;

namespace PuzzleGame
{
    [RequireComponent(typeof(Actor))]
    public class ItemPickup : MonoBehaviour
    {
        //used to delete the pick-up point from all rooms
        [SerializeField] InventoryItemDef _itemToPickup;

        //must be shared across the room
        [SerializeField] int _quantityPerPickup = 1;
        [SerializeField] float _roomRelativeScale = 1;
        [SerializeField] bool _oneTime = true;
        Actor _actor;

        private void Awake()
        {
            _actor = GetComponent<Actor>();
        }

        public void PickupItem()
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;
            GameContext.s_player.AddToInventory(_itemToPickup, _quantityPerPickup, _roomRelativeScale, curRoom);

            if(_oneTime)
            {
                enabled = false;
                GameContext.s_gameMgr.DestroyActor(_actor.actorId);
            }
        }
    }
}
