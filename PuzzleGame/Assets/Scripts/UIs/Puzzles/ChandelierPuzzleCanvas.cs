using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class ChandelierPuzzleCanvas : InspectionCanvas
    {
        [SerializeField] AnimationClip _codeClip;
        [SerializeField] string _correctSequence = "3142";
        [SerializeField] Sprite _litSprite, _unlitSprite, _acceptedSprite;
        [SerializeField] Button _light1, _light2, _light3, _light4;
        [SerializeField] Text _prompt;
        StringBuilder _userSequence = new StringBuilder();

        PlayableGraph _playableGraph;
        AnimationClipPlayable _clipPlayable;

        bool _viewCodeMode = false;
        Button[] _lightButtons;

        protected override void Start()
        {
            base.Start();
            _prompt.text = "<color=red>Locked</color>";
            _lightButtons = new Button[] { _light1, _light2, _light3, _light4 };

            _light1.onClick.AddListener(() => { UserInput(0); });
            _light2.onClick.AddListener(() => { UserInput(1); });
            _light3.onClick.AddListener(() => { UserInput(2); });
            _light4.onClick.AddListener(() => { UserInput(3); });
        }

        protected override void SetInspectable(Inspectable inspectable)
        {
            base.SetInspectable(inspectable);
            _viewCodeMode = inspectable.room.roomIndex == GameConst.k_startingRoomIndex;

            //puzzle mode
            if (!_viewCodeMode)
            {
                _prompt.gameObject.SetActive(true);

                if(_playableGraph.IsValid())
                    _playableGraph.Destroy();
                
                ResetAll();
            }
            //view code mode
            else
            {
                _prompt.gameObject.SetActive(false);

                if(!_playableGraph.IsValid())
                {
                    _clipPlayable = AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _codeClip, out _playableGraph);
                    _clipPlayable.SetSpeed(0.2);
                    _clipPlayable.Play();
                }
                else
                {
                    _clipPlayable.SetTime(0);
                    _clipPlayable.Play();
                }
            }
        }

        void UserInput(int lightId)
        {
            IEnumerator _successRoutine()
            {
                yield return new WaitForSecondsRealtime(1.5f);
                OnBackPressed();
            }

            if (!_inspectable.canInspect || _viewCodeMode)
                return;

            _lightButtons[lightId].image.sprite = _litSprite;

            _userSequence.Append((lightId + 1).ToString());
            int curLen = _userSequence.Length;

            if (curLen < _correctSequence.Length)
            {
                if (_userSequence.ToString() != _correctSequence.Substring(0, curLen))
                {
                    ResetAll();
                }
                else
                {

                }
            }
            else
            {
                if (_userSequence.ToString() != _correctSequence)
                {
                    ResetAll();
                }
                else
                {
                    SetSprites(_acceptedSprite);
                    _prompt.text = "<color=green>Unlocked</color>";
                    _inspectable.canInspect = false;

                    StartCoroutine(_successRoutine());
                }
            }
        }

        private void ResetAll()
        {
            _userSequence.Clear();
            SetSprites(_unlitSprite);
        }

        private void SetSprites(Sprite sprite)
        {
            foreach (var button in _lightButtons)
            {
                button.image.sprite = sprite;
            }
        }

        void Update()
        {
            if (_viewCodeMode && _clipPlayable.IsValid())
            {
                //loop the clip
                if (Mathf.Abs((float)_clipPlayable.GetTime() - _codeClip.length) < 0.01f)
                {
                    _clipPlayable.SetTime(0);
                }
            }
        }

        void OnDestroy()
        {
            if(_playableGraph.IsValid())
                _playableGraph.Destroy();
        }
    }
}
