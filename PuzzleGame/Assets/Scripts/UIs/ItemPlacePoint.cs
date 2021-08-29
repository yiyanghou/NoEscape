using System;
using System.Collections;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Image), typeof(RectTransform))]
    public class ItemPlacePoint : InventoryItemDragReceiver, IPointerClickHandler
    {
        [SerializeField] UltEvent _onPickupItem;
        [SerializeField] Sprite _placedSprite;
        [SerializeField] Sprite _emptySprite;
        [SerializeField] Vector2 _emptySpriteSize;
        [SerializeField] Vector2 _placedSpriteSize;
        [SerializeField] PromptDef _inspectionPrompt;

        RectTransform _rect;
        public UltEvent onPickupItem { get { return _onPickupItem; } }

        bool _hasItem;
        public bool hasItem 
        { 
            get
            {
                return _hasItem;
            }
            set
            {
                _hasItem = value;
                _img.sprite = value ? _placedSprite : _emptySprite;
                _rect.sizeDelta = value ? _placedSpriteSize : _emptySpriteSize;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _rect = GetComponent<RectTransform>();
            _onSuccessDrop.AddPersistentCall((Action)(() =>
            {
                hasItem = true;
            }));

            hasItem = false;
        }

        public override void OnDrop(PointerEventData eventData)
        {
            //only do the drop if it has no item
            if(!hasItem)
            {
                base.OnDrop(eventData);
            }
        }

        public void GiveItem()
        {
            Room curRoom = GameContext.s_gameMgr.curRoom;
            GameContext.s_player.AddToInventory(_targetItem, _requiredQuantity, _requiredItemScale * curRoom.roomScale);
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if(_isMouseOver)
            {
                if(hasItem)
                {
                    //pickup
                    GiveItem();
                    _onPickupItem?.Invoke();

                    hasItem = false;
                }
                else
                {
                    //inspect
                    if(_inspectionPrompt)
                    {
                        DialogueMenu.Instance.DisplayPrompt(_inspectionPrompt);
                    }
                }
            }
        }
    }
}
