using PuzzleGame.EventSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class PauseMenu : SingletonGameMenu<PauseMenu>
    {
        [SerializeField] Button _resumeButton;
        [SerializeField] Button _settingButton;
        [SerializeField] Button _quitGameButton;

        protected override void Start()
        {
            base.Start();

            _resumeButton.onClick.AddListener(ResumeGame);
            _settingButton.onClick.AddListener(OpenSettingMenu);
            _quitGameButton.onClick.AddListener(QuitGame);
        }

        public override void OnEnterMenu()
        {
            base.OnEnterMenu();
            Messenger.Broadcast(M_EventType.ON_GAME_PAUSED);
        }

        public override void OnLeaveMenu()
        {
            base.OnLeaveMenu();
            Messenger.Broadcast(M_EventType.ON_GAME_RESUMED);
        }

        void QuitGame()
        {
            GameContext.s_gameMgr.QuitGame();
        }

        void ResumeGame()
        {
            OnBackPressed();
        }

        void OpenSettingMenu()
        {
            GameContext.s_UIMgr.OpenMenu(SettingMenu.Instance);
        }
    }
}
