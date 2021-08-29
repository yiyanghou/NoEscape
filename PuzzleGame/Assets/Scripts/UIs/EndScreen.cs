using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PuzzleGame.EventSystem;

namespace PuzzleGame.UI
{
    public class EndScreen : SingletonGameMenu<EndScreen>
    {
        [SerializeField] float _blackFadeInDuration;
        [SerializeField] float _textAppearDuration;

        [SerializeField] Image _blackScreenImage;
        [SerializeField] Text _endText;

        [SerializeField] Button _restartButton;
        [SerializeField] Button _quitGameButton;

        protected override void Awake()
        {
            base.Awake();
            Messenger.AddListener<GameEndEventData>(M_EventType.ON_GAME_END, OnGameEnd);
        }

        protected override void Start()
        {
            base.Start();

            _quitGameButton.onClick.AddListener(GameContext.s_gameMgr.QuitGame);
            _restartButton.onClick.AddListener(GameContext.s_gameMgr.RestartGame);
            _restartButton.gameObject.SetActive(false);
            _quitGameButton.gameObject.SetActive(false);

            SetImageAlpha(0);
            SetTextAlpha(0);
        }

        public override bool CanClose()
        {
            return false;
        }

        public void OnGameEnd(GameEndEventData data)
        {
            switch(data.type)
            {
                case EGameEndingType.DEATH:
                    _endText.text = "You Died";
                    break;
                case EGameEndingType.ESCAPE:
                    _endText.text = "You Escaped";
                    break;
            }

            GameContext.s_UIMgr.OpenMenu(Instance);
        }

        public override void OnEnterMenu()
        {
            base.OnEnterMenu();
            StartCoroutine(_endingSequence());
        }

        public override void OnLeaveMenu()
        {
            base.OnLeaveMenu();
        }

        void SetImageAlpha(float alpha)
        {
            Color c = _blackScreenImage.color;
            c.a = alpha;
            _blackScreenImage.color = c;
        }
        void SetTextAlpha(float alpha)
        {
            Color c = _endText.color;
            c.a = alpha;
            _endText.color = c;
        }

        IEnumerator _endingSequence()
        {
            yield return StartCoroutine(_lerpRoutine(SetImageAlpha, 0, 1, _blackFadeInDuration));
            StartCoroutine(_lerpRoutine(SetTextAlpha, 0, 1, _textAppearDuration));

            _restartButton.gameObject.SetActive(true);
            _quitGameButton.gameObject.SetActive(true);
        }

        IEnumerator _lerpRoutine(Action<float> setter, float start, float end, float time)
        {
            for(float t=0; t<time; t+=Time.deltaTime)
            {
                setter(Mathf.Lerp(start, end, t / time));
                yield return new WaitForEndOfFrame();
            }
            setter(end);
        }
    }
}
