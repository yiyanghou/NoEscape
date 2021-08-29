using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class ElectricFanCanvas : InspectionCanvas
    {
        [SerializeField] PromptDef _bladePickupFailPrompt;
        [SerializeField] InventoryItemDef _bladeItem;
        /*
         * state variable layout
         *   blade pickup point active state (3 bits)
         */
        [SerializeField] IntVariable _state;

        [SerializeField] Button[] _bladePickupPoints;
        [SerializeField] InventoryItemDragReceiver[] _bladeInstallPoints;

        protected override void Awake()
        {
            base.Awake();

            _state.valueChanged += UpdateState;

            _bladePickupPoints[0].onClick.AddListener(() => { PickupBlade(0); });
            _bladePickupPoints[1].onClick.AddListener(() => { PickupBlade(1); });
            _bladePickupPoints[2].onClick.AddListener(() => { PickupBlade(2); });

            _bladeInstallPoints[0].onSuccessDrop.AddPersistentCall((Action)(() => { PlaceBlade(0); }));
            _bladeInstallPoints[1].onSuccessDrop.AddPersistentCall((Action)(() => { PlaceBlade(1); }));
            _bladeInstallPoints[2].onSuccessDrop.AddPersistentCall((Action)(() => { PlaceBlade(2); }));
        }

        protected override void Start()
        {
            base.Start();

            if (_state.val != 0b111)
            {
                _state.val = 0b111;
            }
        }

        public void PickupBlade(int index)
        {
            Debug.Assert(index >= 0 && index < 3);

            //can only pickup the outermost blade
            if (index < 2 && _bladePickupPoints[index + 1].gameObject.activeSelf)
            {
                DialogueMenu.Instance.DisplayPrompt(_bladePickupFailPrompt);
                return;
            }

            GameContext.s_player.AddToInventory(_bladeItem, 1, 1, GameContext.s_gameMgr.curRoom);

            switch (index)
            {
                case 0:
                    _state.val = 0;
                    break;
                case 1:
                    _state.val = 0b100;
                    break;
                case 2:
                    _state.val = 0b110;
                    break;
            }
        }

        public void PlaceBlade(int index)
        {
            Debug.Assert(index >= 0 && index < 3);

            switch (index)
            {
                case 0:
                    _state.val = 0b100;
                    break;
                case 1:
                    _state.val = 0b110;
                    break;
                case 2:
                    _state.val = 0b111;
                    break;
            }
        }

        //the state is too simple, engage hardcoding
        private void UpdateState(int newState)
        {
            switch (_state.val)
            {
                case 0b111:
                    _bladePickupPoints[0].gameObject.SetActive(true);
                    _bladePickupPoints[1].gameObject.SetActive(true);
                    _bladePickupPoints[2].gameObject.SetActive(true);

                    _bladeInstallPoints[0].gameObject.SetActive(false);
                    _bladeInstallPoints[1].gameObject.SetActive(false);
                    _bladeInstallPoints[2].gameObject.SetActive(false);

                    break;
                case 0b110:
                    _bladePickupPoints[0].gameObject.SetActive(true);
                    _bladePickupPoints[1].gameObject.SetActive(true);
                    _bladePickupPoints[2].gameObject.SetActive(false);

                    _bladeInstallPoints[0].gameObject.SetActive(false);
                    _bladeInstallPoints[1].gameObject.SetActive(false);
                    _bladeInstallPoints[2].gameObject.SetActive(true);

                    break;
                case 0b100:
                    _bladePickupPoints[0].gameObject.SetActive(true);
                    _bladePickupPoints[1].gameObject.SetActive(false);
                    _bladePickupPoints[2].gameObject.SetActive(false);

                    _bladeInstallPoints[0].gameObject.SetActive(false);
                    _bladeInstallPoints[1].gameObject.SetActive(true);
                    _bladeInstallPoints[2].gameObject.SetActive(false);

                    break;
                case 0:
                    _bladePickupPoints[0].gameObject.SetActive(false);
                    _bladePickupPoints[1].gameObject.SetActive(false);
                    _bladePickupPoints[2].gameObject.SetActive(false);

                    _bladeInstallPoints[0].gameObject.SetActive(true);
                    _bladeInstallPoints[1].gameObject.SetActive(false);
                    _bladeInstallPoints[2].gameObject.SetActive(false);

                    break;
                default:
                    Debug.Assert(false, "unknown blade state");
                    return;
            }
        }
    }
}