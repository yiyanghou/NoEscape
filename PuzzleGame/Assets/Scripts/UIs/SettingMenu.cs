using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class SettingMenu : SingletonGameMenu<SettingMenu>
    {
        [SerializeField] Button _backButton;
        [SerializeField] Slider _volumeSlider;

        protected override void Awake()
        {
            base.Awake();
            _backButton.onClick.AddListener(OnBackPressed);
            _volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        // Start is called before the first frame update
        protected override void Start()
        {
            base.Start();

            _volumeSlider.value = GameContext.s_audioMgr.volumeScale;
        }

        void SetVolume(float value)
        {
            GameContext.s_audioMgr.volumeScale = value;
        }
    }
}