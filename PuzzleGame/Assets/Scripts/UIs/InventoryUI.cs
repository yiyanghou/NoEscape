using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using PuzzleGame.EventSystem;

namespace PuzzleGame.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] GameObject _slotRoot;
        InventorySlot[] _itemSlots;

        private void Awake()
        {
            Messenger.AddListener<InventoryChangeEventData>(M_EventType.ON_INVENTORY_CHANGE, OnInventoryChange);
            _itemSlots = _slotRoot.GetComponentsInChildren<InventorySlot>();
            foreach(var slot in _itemSlots)
            {
                slot.UnSet();
            }
        }

        void OnInventoryChange(InventoryChangeEventData data)
        {
            Assert.IsTrue( data.slotIndex >= 0 && data.slotIndex < _itemSlots.Length && data.itemIns.quantity >= 0);

            //item is used up
            if (data.itemIns.quantity == 0)
            {
                _itemSlots[data.slotIndex].UnSet();
            }
            else
            {
                _itemSlots[data.slotIndex].Set(data.itemIns);
            }
        }
    }
}
