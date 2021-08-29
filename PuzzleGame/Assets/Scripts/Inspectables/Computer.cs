using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.UI;

using UnityEngine.UI;

namespace PuzzleGame
{
    public class Computer : Inspectable
    {
        [Header("Computer Config")]
        [SerializeField] BoolVariable _isComputerOn;
        [SerializeField] Sprite _onSprite, _offSprite;

        protected override void Awake()
        {
            base.Awake();

            UpdateComputerState(false);
            _isComputerOn.valueChanged += UpdateComputerState;
        }

        protected override void Start()
        {
            base.Start();
        }

        void UpdateComputerState(bool isOn)
        {
            spriteRenderer.sprite = isOn ? _onSprite : _offSprite;
        }
    }
}