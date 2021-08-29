using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class CreditMenu : SingletonGameMenu<CreditMenu>
    {
        [SerializeField] Button _backButton;

        protected override void Awake()
        {
            base.Awake();
            _backButton.onClick.AddListener(OnBackPressed);
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();
        }
    }
}