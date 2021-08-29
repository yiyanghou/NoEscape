using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace PuzzleGame.UI
{
    public class CheatMenu : MonoBehaviour
    {
        [SerializeField] InventoryItemDef[] _items;
        [SerializeField] Dropdown _itemDropDown;
        [SerializeField] Button _addButton;
        [SerializeField] InputField _quantityInput, _scaleInput;

        int _quant;
        float _scale;
        InventoryItemDef _curItem;

        void Awake()
        {
            List<Dropdown.OptionData> options = _items.Select(x => new Dropdown.OptionData(x.name, x.inventoryDisplaySprite)).ToList();
            _itemDropDown.AddOptions(options);
            _itemDropDown.onValueChanged.AddListener(OnItemDropDown);
            _quantityInput.onValueChanged.AddListener(OnQuantInput);
            _scaleInput.onValueChanged.AddListener(OnScaleInput);
            _addButton.onClick.AddListener(OnClickAddButton);

            OnQuantInput("1");
            OnScaleInput("1");
            OnItemDropDown(0);
        }

        void OnQuantInput(string val)
        {
            if(int.TryParse(val, out int intVal))
            {
                _quant = intVal;
            }
        }
        void OnScaleInput(string val)
        {
            if (float.TryParse(val, out float floatVal))
            {
                _scale = floatVal;
            }
        }
        void OnItemDropDown(int option)
        {
            _curItem = _items[option];
        }
        void OnClickAddButton()
        {
            GameActions.AddToInventory(_curItem, _scale, _quant, true);
        }
    }
}
