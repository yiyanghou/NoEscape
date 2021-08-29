using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using UnityEngine;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    /// <summary>
    /// an instance of an inventory item
    /// </summary>
    public class InventoryItem
    {
        public InventoryItemDef def;
        public int quantity;

        public float scale; //the global scale of the item, determined by the room in which it's picked up

        public float GetRoomRelativeScale()
        {
            float curRoomScale = GameContext.s_gameMgr.curRoom.roomScale;
            return scale / curRoomScale;
        }
    }


    [RequireComponent(typeof(PlayerController))]
    public class Player : MonoBehaviour
    {
        public PlayerController controller { get; private set; }
        public Actor actor { get; private set; }

        InventoryItem[] _inventory = new InventoryItem[GameConst.k_playerInventorySize];
        public ReadOnlyCollection<InventoryItem> inventory { get => Array.AsReadOnly(_inventory); }

        private void Awake()
        {
            actor = GetComponent<Actor>();
            controller = GetComponent<PlayerController>();
            gameObject.layer = GameConst.k_playerLayer;

            for(int i=0; i<GameConst.k_playerInventorySize; i++)
            {
                _inventory[i] = null;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        private int AllocateSlot()
        {
            for (int i = 0; i < GameConst.k_playerInventorySize; i++)
            {
                if (_inventory[i] == null)
                    return i;
            }

            Debug.Assert(false, "Player inventory is full, cannot allocate");
            return -1;
        }

        /// <summary>
        /// returns if the player has an item of the size same as the <paramref name="location"/>
        /// with at least <paramref name="minQuantity"/> quantities
        /// </summary>
        public enum ItemQueryResult
        {
            NO_ITEM,
            WRONG_SCALE_OR_QUANTITY,
            SUCCESS
        }
        private InventoryItem[] _itemQueryBuffer = new InventoryItem[GameConst.k_playerInventorySize];
        public ItemQueryResult QueryItem(InventoryItemDef def, int minQuantity, float roomRelativeScale, Room location)
        {
            float globalScale = roomRelativeScale * location.roomScale;

            int bufferSize = 0;
            for (int i = 0; i < GameConst.k_playerInventorySize; i++)
            {
                if (_inventory[i] == null)
                    continue;

                if(ReferenceEquals(_inventory[i].def, def))
                {
                    _itemQueryBuffer[bufferSize] = _inventory[i];
                    bufferSize++;
                }
            }

            if(bufferSize == 0)
            {
                return ItemQueryResult.NO_ITEM;
            }
            else
            {
                for(int i=0; i<bufferSize; i++)
                {
                    if(_itemQueryBuffer[i].quantity >= minQuantity && Mathf.Approximately(_inventory[i].scale, globalScale))
                    {
                        return ItemQueryResult.SUCCESS;
                    }
                }

                return ItemQueryResult.WRONG_SCALE_OR_QUANTITY;
            }
        }

        public void AddToInventory(InventoryItemDef def, int quantity, float roomRelativeScale, Room location)
        {
            float globalScale = roomRelativeScale * location.roomScale;
            AddToInventory(def, quantity, globalScale);
        }
        public void AddToInventory(InventoryItemDef def, int quantity, float globalScale)
        {
            //the inventory is very small, so just brute force everything
            InventoryItem itemIns = null;
            int index = -1;

            for (int i = 0; i < GameConst.k_playerInventorySize; i++)
            {
                if (_inventory[i] == null)
                    continue;

                if (ReferenceEquals(_inventory[i].def, def) && Mathf.Approximately(_inventory[i].scale, globalScale))
                {
                    //the item is of the same type AND was picked up in the same room
                    //then stack the items

                    _inventory[i].quantity += quantity;

                    itemIns = _inventory[i];
                    index = i;
                    break;
                }
            }

            //a new item is added
            if (index == -1)
            {
                index = AllocateSlot();

                itemIns = new InventoryItem()
                {
                    def = def,
                    quantity = quantity,
                    scale = globalScale
                };

                _inventory[index] = itemIns;
            }

            Messenger.Broadcast(M_EventType.ON_INVENTORY_CHANGE, new InventoryChangeEventData(itemIns, index));
        }

        public void RemoveFromInventory(InventoryItem item, int quantity)
        {
            InventoryItem itemIns = null;
            int index = -1;

            for (int i = 0; i < GameConst.k_playerInventorySize; i++)
            {
                if (ReferenceEquals(_inventory[i], item))
                {
                    itemIns = _inventory[i];
                    index = i;

                    itemIns.quantity -= quantity;

                    if(itemIns.quantity == 0)
                    {
                        _inventory[i] = null;
                    }
                    
                    break;
                }
            }

            Messenger.Broadcast(M_EventType.ON_INVENTORY_CHANGE, new InventoryChangeEventData(itemIns, index));
        }
    }
}
