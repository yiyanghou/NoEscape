using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PuzzleGame.UI;
using System;

namespace PuzzleGame
{
    public class ElectricFan : Inspectable
    {
        [Header("electric fan config")]
        [SerializeField] Sprite[] _stateSprites;
        [SerializeField] IntVariable _state;

        protected override void Awake()
        {
            base.Awake();
            _state.valueChanged += UpdateState;
        }

        private void UpdateState(int newState)
        {
            switch (_state.val)
            {
                case 0b111:
                    spriteRenderer.sprite = _stateSprites[3];
                    break;
                case 0b110:
                    spriteRenderer.sprite = _stateSprites[2];
                    break;
                case 0b100:
                    spriteRenderer.sprite = _stateSprites[1];

                    break;
                case 0:
                    spriteRenderer.sprite = _stateSprites[0];
                    break;
                default:
                    Debug.Assert(false, "unknown blade state");
                    return;
            }
        }
    }
}