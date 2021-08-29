using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class ClockPuzzleCanvas : InspectionCanvas
    {
        // 8 triggers, trigger state is packed into a shared integer, indexed as follows
        // default state
        /*
         *           0
         *        1     7
         *    2             6
         *        3     5
         *           4
         */

        const int k_numSwitches = 8;
        const int k_allsetState = 0b11111111;
        const float k_rotationStep = 45;
        [Header("shared states")]
        [SerializeField] FloatVariable _handsRotation;
        [SerializeField] BoolVariable _isGameUnlocked;
        
        [Header("puzzle config")]
        [SerializeField] float _maxRotation;
        [SerializeField] AudioClip _rotationSound;
        [SerializeField] AudioClip _handsSound;
        [SerializeField] AudioClip _jammedSound;
        [SerializeField] DialogueDef _clockUnlockDialogue;

        [SerializeField] AnimationClip _successClip;
        [SerializeField] AnimationClip _handsClip;
        [SerializeField] Button _handsTrigger;
        [SerializeField] Sprite _setSwitch, _unsetSwitch;
        [SerializeField] Transform _switchStartingAnchor;
        [SerializeField] GameObject _switchPrefab;
        [SerializeField] GameObject _unlockedGameRoot;
        [SerializeField] GameObject _lockedGameRoot;
        [SerializeField] Button _keyPickupButton;
        [SerializeField] Button _resetButton, _CWButton, _CCWButton;

        int _state;
        int state
        {
            get => _state;
            set
            {
                _state = value;
                UpdateSwitches(value);
            }
        }

        Image[] _switches;
        Dictionary<EdgeCollider2D, int> _switchCollider2SwitchIdx;
        float _switchRadius;
        bool _isPlayingClip;

        protected override void Awake()
        {
            base.Awake();
            _isGameUnlocked.valueChanged += UpdatePuzzleLockState;

            _resetButton.onClick.AddListener(ResetPuzzle);
            _handsTrigger.onClick.AddListener(ExtendHands);
            _CWButton.onClick.AddListener(RotateHandsCW);
            _CCWButton.onClick.AddListener(RotateHandsCCW);

            //generate switches
            _switches = new Image[k_numSwitches];
            int step = 360 / k_numSwitches;
            for (int i = 0; i < k_numSwitches; i++)
            {
                _switches[i] = Instantiate(_switchPrefab, _switchStartingAnchor.parent).GetComponent<Image>();
                _switches[i].transform.localPosition = _switchStartingAnchor.localPosition;
                _switches[i].transform.RotateAround(_switchStartingAnchor.parent.position, Vector3.forward, step * i);
            }

            _switchCollider2SwitchIdx = new Dictionary<EdgeCollider2D, int>();
            for (int i = 0; i < k_numSwitches; i++)
            {
                var collider = _switches[i].GetComponent<EdgeCollider2D>();
                collider.isTrigger = true;
                _switchCollider2SwitchIdx[collider] = i;
            }

            _switchRadius = Vector2.Distance(_switchStartingAnchor.position, _switchStartingAnchor.parent.position);
            _isPlayingClip = false;
            _handsRotation.val = 0;


            _keyPickupButton.gameObject.SetActive(false);
            //configure world UI
            _keyPickupButton.onClick.AddListener(OnBackPressed);
        }

        protected override void Start()
        {
            base.Start();

            state = 0;
            _isGameUnlocked.val = false;

            UpdateSwitches(0);
            UpdatePuzzleLockState(false);
        }

        protected override void SetInspectable(Inspectable inspectable)
        {
            base.SetInspectable(inspectable);
            _handsTrigger.transform.rotation = Quaternion.Euler(0, 0, _handsRotation);
        }
        public override void OnLeaveMenu()
        {
            base.OnLeaveMenu();
        }
        /// <summary>
        /// which switches will be triggered given the room's global orientation?
        /// </summary>
        private int GetTargetSwitches()
        {
            /*
             * default hands state
             * 
             *   2 --- x --- 6
             *         |
             *         4
             */

            void DoHandRaycast(ref int returnValue, ref int totalOverlap, Vector2 dir)
            {
                var results = Physics2D.RaycastAll(_handsTrigger.transform.position, dir, _switchRadius, 1 << GameConst.k_UILayer);
                foreach (var hit in results)
                {
                    if (hit.collider is EdgeCollider2D edgeCollider)
                    {
                        if (_switchCollider2SwitchIdx.ContainsKey(edgeCollider))
                        {
                            returnValue |= 1 << _switchCollider2SwitchIdx[edgeCollider];
                            totalOverlap++;
                        }
                    }
                }
            }

            int ret = 0, total = 0;
            DoHandRaycast(ref ret, ref total, _handsTrigger.transform.right);
            DoHandRaycast(ref ret, ref total, -_handsTrigger.transform.right);
            DoHandRaycast(ref ret, ref total, -_handsTrigger.transform.up);

            //should have exactly 3 bits set
            Debug.Assert(total == 3);

            return ret;
        }
        public void RotateHandsCW()
        {
            if(_handsRotation - k_rotationStep < -_maxRotation)
            {
                GameActions.PlaySounds(_jammedSound);
            }
            else
            {
                GameActions.PlaySounds(_rotationSound);
            }

            _handsRotation.val = Mathf.Max(_handsRotation - k_rotationStep, -_maxRotation);
            _handsTrigger.transform.rotation = Quaternion.Euler(0, 0, _handsRotation);
        }
        public void RotateHandsCCW()
        {
            if (_handsRotation + k_rotationStep > _maxRotation)
            {
                GameActions.PlaySounds(_jammedSound);
            }
            else
            {
                GameActions.PlaySounds(_rotationSound);
            }

            _handsRotation.val = Mathf.Min(_handsRotation + k_rotationStep, _maxRotation);
            _handsTrigger.transform.rotation = Quaternion.Euler(0, 0, _handsRotation);
        }
        public void ResetPuzzle()
        {
            state = 0;
        }
        private void ExtendHands()
        {
            IEnumerator _successRoutine(float numSeconds)
            {
                AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _successClip, out PlayableGraph graph);
                _isPlayingClip = true;

                yield return new WaitForSeconds(numSeconds);
                _isPlayingClip = false;

                graph.Destroy();
                _keyPickupButton.gameObject.SetActive(true);
            }

            IEnumerator _setSwitchRoutine(float numSeconds)
            {
                AnimationPlayableUtilities.PlayClip(GetComponent<Animator>(), _handsClip, out PlayableGraph graph);
                _isPlayingClip = true;

                yield return new WaitForSeconds(numSeconds);

                graph.Destroy();

                int flags = GetTargetSwitches();
                //flip the values
                state = state ^ flags;

                if (state == k_allsetState)
                {
                    _inspectable.canInspect = false;
                    GameContext.s_gameMgr.StartCoroutine(_successRoutine(_successClip.length));
                }

                _isPlayingClip = false;
            }

            if (!_inspectable.canInspect || _isPlayingClip)
            {
                return;
            }

            GameActions.PlaySounds(_handsSound);
            GameContext.s_gameMgr.StartCoroutine(_setSwitchRoutine(_handsClip.length));
        }

        private void UpdateSwitches(int newValue)
        {
            for (int i = 0; i < k_numSwitches; i++)
            {
                if ((newValue & (1 << i)) != 0)
                {
                    _switches[i].sprite = _setSwitch;
                }
                else
                {
                    _switches[i].sprite = _unsetSwitch;
                }
            }
        }

        private void UpdatePuzzleLockState(bool isUnlocked)
        {
            _lockedGameRoot.SetActive(!isUnlocked);
            _unlockedGameRoot.SetActive(isUnlocked);

            if (isUnlocked)
            {
                DialogueMenu.Instance.DisplayDialogue(_clockUnlockDialogue);

                _resetButton.gameObject.SetActive(true);
                _backButton.gameObject.SetActive(true);
            }
            else
            {
                _resetButton.gameObject.SetActive(false);
                _backButton.gameObject.SetActive(true);
            }
        }
    }
}
