using System.Collections;
using System.Collections.Generic;
using UltEvents;

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

using PuzzleGame.UI;

namespace PuzzleGame
{
    public class Clock : Inspectable
    {
        [Header("Clock Puzzle")]
        [SerializeField] FloatVariable _handsRotation;
        [SerializeField] BoolVariable _isGameUnlocked;
        [SerializeField] Transform _clockPlumb;
        [SerializeField] Transform _clockHands;

        [SerializeField] Sprite _lockedClock, _unlockedClock;

        protected override void Awake()
        {
            base.Awake();
            _isGameUnlocked.valueChanged += UpdatePuzzleLockState;
        }

        protected override void Start()
        {
            base.Start();
            UpdatePuzzleLockState(false);
        }

        protected override void Update()
        {
            base.Update();

            //make sure the plumb is always pointing downwards
            _clockPlumb.rotation = Quaternion.identity;

            //update the hands rotation
            _clockHands.rotation = Quaternion.Euler(0, 0, _handsRotation);
        }

        public override void BeginInspect()
        {
            base.BeginInspect();
        }

        public override void EndInspect()
        {
            base.EndInspect();
        }

        void UpdatePuzzleLockState(bool isUnlocked)
        {
            if(isUnlocked)
            {
                _clockHands.gameObject.SetActive(true);
                spriteRenderer.sprite = _unlockedClock;
            }
            else
            {
                _clockHands.gameObject.SetActive(false);
                spriteRenderer.sprite = _lockedClock;
            }
        }
    }
}
