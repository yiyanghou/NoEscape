using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UltEvents;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Image))]
    public class InventoryItemDragReceiver : OutlineElement, IDropHandler
    {
        [SerializeField] protected UltEvent _onSuccessDrop;
        [SerializeField] protected InventoryItemDef _targetItem;
        [SerializeField] protected int _requiredQuantity = 1;
        [SerializeField] protected float _requiredItemScale = 1;
        [SerializeField] protected PromptDef _wrongItemPrompt;
        [SerializeField] protected PromptDef _wrongQuantityOrScalePrompt;

        public UltEvent onSuccessDrop { get { return _onSuccessDrop; } }

        public virtual void OnDrop(PointerEventData eventData)
        {
            InventoryItem draggingItem = GameContext.s_curDraggingItem;
            if (draggingItem != null && _isMouseOver)
            {
                if (!ReferenceEquals(draggingItem.def, _targetItem))
                {
                    DialogueMenu.Instance.DisplayPrompt(_wrongItemPrompt);
                }
                else
                {
                    //reuqiredItemScale is local, so we need to convert it into global scale to be compared
                    Room curRoom = GameContext.s_gameMgr.curRoom;
                    float curRoomScale = curRoom.roomScale;

                    if (draggingItem.quantity < _requiredQuantity || !Mathf.Approximately(draggingItem.scale, curRoomScale * _requiredItemScale))
                    {
                        DialogueMenu.Instance.DisplayPrompt(_wrongQuantityOrScalePrompt);
                    }
                    else
                    {
                        _onSuccessDrop?.Invoke();

                        GameContext.s_player.RemoveFromInventory(draggingItem, _requiredQuantity);
                    }
                }
            }
        }
    }
}