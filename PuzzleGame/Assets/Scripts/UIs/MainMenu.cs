using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class MainMenu : SingletonGameMenu<MainMenu>
    {
        [SerializeField] Button _startButton;
        [SerializeField] Button _creditButton;
        [SerializeField] Button _settingButton;
        [SerializeField] Button _quitButton;

        protected override void Start()
        {
            base.Start();

            _startButton.onClick.AddListener(StartGame);
            _creditButton.onClick.AddListener(OpenCreditMenu);
            _settingButton.onClick.AddListener(OpenSettingMenu);
            _quitButton.onClick.AddListener(QuitGame);

            GameContext.s_UIMgr.OpenMenu(this);
        }

        public void StartGame()
        {
            GameContext.s_gameMgr.StartGame();
            OnBackPressed();
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }

        public void OpenSettingMenu()
        {
            GameContext.s_UIMgr.OpenMenu(SettingMenu.Instance);
        }

        public void OpenCreditMenu()
        {
            GameContext.s_UIMgr.OpenMenu(CreditMenu.Instance);
        }
    }
}
