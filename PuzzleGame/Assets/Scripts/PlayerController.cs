using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using System;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PuzzleGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Player))]
    public class PlayerController : MonoBehaviour
    {
        #region Movement
        [SerializeField] Collider2D _groundCheckCollider;
        [SerializeField] float _maxAllowedCheckGroundAngle = 75; //allow to stand on a surface X degrees from (0, 1)
        Vector2 _groundCheckSize;
        bool _grounded = false, _lastGrounded;

        ContactFilter2D _groundColliderFilter;
        ContactFilter2D _propAndBoundaryfilter;

        ContactPoint2D[] _contacts;
        Collider2D _curGroundCollider;
#if UNITY_EDITOR
        ContactPoint2D[] _DEBUG_allGroundHits;
#endif

        #endregion

        #region Interaction
        [SerializeField] GenericTrigger _interactionTrigger;
        [SerializeField] [Range(0.2f, 5f)] float _exitPaintInteractTime = 2f;
        float _interactHoldTimer = 0;
        bool _interactHoldKeyDownLastFrame;
        Interactable _curInteractable;
        Interactable curInteractable
        {
            get => _curInteractable;
            set
            {
                if(!ReferenceEquals(value, _curInteractable))
                {
                    if(_curInteractable)
                    {
                        _curInteractable.OnExitRange();
                    }
                    
                    _curInteractable = value;

                    if(value)
                    {
                        value.OnEnterRange();
                    }
                }
            }
        }
        float _interactionTriggerX;
        #endregion

        #region Components
        Rigidbody2D _rgbody;
        BoxCollider2D _collider;
        Animator _animator;
        SpriteRenderer _spriteRenderer;
        Player _player;
        #endregion

        #region Game Specific
        [SerializeField] BoolVariable _canExitStartingRoom;
        [SerializeField] BoolVariable _canExitSmallRoom;
        int _controlLockCnt = 0;
        #endregion

        #region Audio
        [SerializeField] float _walkingSoundsInterval;
        [SerializeField] AudioCollection _walkingSounds;
        [SerializeField] AudioCollection _jumpingSounds;
        [SerializeField] AudioCollection _groundedSounds;
        Vector2? _lastSoundPlayedPos = null;
        #endregion

        [Serializable]
        public class MovementConfig
        {
            public float speed = 1f;
            public float jumpThrust = 20f;
            public float airBorneSpeed = 0.5f;
        }

        [SerializeField] MovementConfig _moveConfig = new MovementConfig();

        public Vector2 curVelocity { get { return _rgbody.velocity; } }

        bool _controlEnabled = true;
        public bool controlEnabled
        {
            get => _controlEnabled;
            private set
            {
                if(_controlEnabled != value)
                {
                    ClearState();
                    _controlEnabled = value;
                }
            }
        }

        private void SetControlEnabled(bool enable)
        {
            if(!enable)
            {
                ++_controlLockCnt;
            }
            else
            {
                if(_controlLockCnt > 0)
                {
                    --_controlLockCnt;
                }
            }

            if(enable && _controlLockCnt == 0)
            {
                controlEnabled = true;
                _interactionTrigger.gameObject.SetActive(true);
            }
            else
            {
                controlEnabled = false;
                _interactionTrigger.gameObject.SetActive(false);
            }
        }

        private void ClearState()
        {
            _interactHoldTimer = 0;
            _interactHoldKeyDownLastFrame = false;
            curInteractable = null;
        }

        private void Awake()
        {
            Messenger.AddListener(M_EventType.ON_CHANGE_PLAYER_CONTROL, (PlayerControlEventData data) =>
            {
                SetControlEnabled(data.enable);
            });
            Messenger.AddListener(M_EventType.ON_BEFORE_ENTER_ROOM, (RoomEventData data) => 
            {
                SetControlEnabled(false);
            });
            Messenger.AddListener(M_EventType.ON_ENTER_ROOM, (RoomEventData data) => 
            {
                SetControlEnabled(true);
            });
            Messenger.AddListener(M_EventType.ON_CUTSCENE_START, (CutSceneEventData data) => 
            {
                SetControlEnabled(false);
            });
            Messenger.AddListener(M_EventType.ON_CUTSCENE_END, (CutSceneEventData data) => 
            {
                SetControlEnabled(true);
            });
            Messenger.AddListener(M_EventType.ON_GAME_PAUSED, () =>
            {
                SetControlEnabled(false);
            });
            Messenger.AddListener(M_EventType.ON_GAME_RESUMED, () =>
            {
                SetControlEnabled(true);
            });
            Messenger.AddListener(M_EventType.ON_GAME_END, (GameEndEventData data) =>
            {
                SetControlEnabled(false);

                if(data.type == EGameEndingType.DEATH)
                    _animator.SetTrigger(GameConst.k_PlayerDeath_AnimParam);
            });
        }

        // Start is called before the first frame update
        void Start()
        {
            _rgbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<BoxCollider2D>();
            _animator = GetComponent<Animator>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _player = GetComponent<Player>();            

            _groundCheckSize = new Vector2(_collider.size.x * transform.localScale.x * 1.1f, 0.2f);

            _interactionTrigger.onTriggerEnter += OnTriggerEnterInteractable;
            _interactionTrigger.onTriggerStay += OnTriggerEnterInteractable;
            _interactionTrigger.onTriggerExit += OnTriggerExitInteractable;

            _interactionTriggerX = _interactionTrigger.transform.localPosition.x;

            _propAndBoundaryfilter = new ContactFilter2D();
            _propAndBoundaryfilter.useTriggers = false;
            _propAndBoundaryfilter.SetLayerMask(1 << GameConst.k_propLayer | 1 << GameConst.k_boundaryLayer);

            _groundColliderFilter = new ContactFilter2D();
            _propAndBoundaryfilter.useTriggers = false;
            _propAndBoundaryfilter.SetLayerMask(1 << GameConst.k_propLayer | 1 << GameConst.k_boundaryLayer | 1 << GameConst.k_groundLayer);

            _contacts = new ContactPoint2D[20];
        }

        void OnTriggerEnterInteractable(Collider2D collider)
        {
            Interactable interactable = collider.GetComponent<Interactable>();
            if (interactable && interactable.canInteract)
            {
                if(curInteractable)
                {
                    float dist = Vector2.Distance(collider.transform.position, transform.position);
                    if(dist < Vector2.Distance(curInteractable.transform.position, transform.position))
                    {
                        curInteractable = interactable;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    curInteractable = interactable;
                }
            }
        }

        void OnTriggerExitInteractable(Collider2D collider)
        {
            Interactable interactable = collider.GetComponent<Interactable>();

            if (interactable && ReferenceEquals(interactable, _curInteractable))
            {
                curInteractable = null;
            }
        }

        void TurnAround(float horizontalVelocity)
        {
            if (!Mathf.Approximately(0, horizontalVelocity))
            {
                float sign = Mathf.Sign(horizontalVelocity);
                _spriteRenderer.flipX = sign < 0;

                Vector2 pos = _interactionTrigger.transform.localPosition;
                pos.x = sign * _interactionTriggerX;
                _interactionTrigger.transform.localPosition = pos;
            }
        }

        // Update is called once per frame
        void Update()
        {
            MovementUpdate();
            InteractionUpdate();

            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if(GameContext.s_UIMgr.GetOpenMenuCount() > 0)
                {
                    if(!ReferenceEquals(GameContext.s_UIMgr.GetActiveMenu(), MainMenu.Instance))
                    {
                        GameContext.s_UIMgr.CloseCurrentMenu();
                    }
                }
                else
                {
                    GameContext.s_UIMgr.OpenMenu(PauseMenu.Instance);
                }
            }
        }

        void MovementUpdate()
        {
            if(Vector2.Dot(GameContext.s_up, _rgbody.velocity) <= 0)
            {
                _lastGrounded = _grounded;
                _grounded = CheckGrounded();

                if(!_lastGrounded && _grounded)
                {
                    GameActions.PlaySounds(_groundedSounds);
                }
            }

            float vertical = 0, horizontal = 0;

            if(controlEnabled)
            {
                if (_grounded)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        GameActions.PlaySounds(_jumpingSounds);

                        vertical = _moveConfig.jumpThrust;

                        _grounded = false;
                    }
                    if (Input.GetKey(KeyCode.A))
                    {
                        horizontal = -_moveConfig.speed;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        horizontal = _moveConfig.speed;
                    }
                    horizontal = CorrectHorizontalVelocity(horizontal);
                    _rgbody.velocity = GameContext.s_right * horizontal + GameContext.s_up * vertical;

                    if (_rgbody.velocity != Vector2.zero)
                    {
                        if (_lastSoundPlayedPos == null || Vector2.Distance(_lastSoundPlayedPos.Value, transform.position) > _walkingSoundsInterval)
                        {
                            GameActions.PlaySounds(_walkingSounds);
                            _lastSoundPlayedPos = transform.position;
                        }
                    }
                }
                else
                {
                    if (Input.GetKey(KeyCode.A))
                    {
                        horizontal = -_moveConfig.airBorneSpeed;
                    }
                    if (Input.GetKey(KeyCode.D))
                    {
                        horizontal = _moveConfig.airBorneSpeed;
                    }
                    horizontal = CorrectHorizontalVelocity(horizontal);
                    _rgbody.velocity = GameContext.s_right * horizontal + GameContext.s_up * Vector2.Dot(GameContext.s_up, _rgbody.velocity);

                    _lastSoundPlayedPos = null;
                }
            }

            _animator.SetBool(GameConst.k_PlayerAirborne_AnimParam, !_grounded);
            _animator.SetBool(GameConst.k_PlayerWalking_AnimParam, !Mathf.Approximately(0, horizontal));
            _animator.SetFloat(GameConst.k_PlayerXSpeed_AnimParam, horizontal);
            _animator.SetFloat(GameConst.k_PlayerYSpeed_AnimParam, _rgbody.velocity.y);

            TurnAround(horizontal);
        }

        //make sure the horizontal vel is 0 when the play is running into a wall or an obstacle
        float CorrectHorizontalVelocity(float horizontal)
        {
            float correctedHorizontalVel = horizontal;

            int numContacts = Physics2D.GetContacts(_rgbody, _propAndBoundaryfilter, _contacts);
            if (numContacts > 0)
            {
                for (int i = 0; i < numContacts; i++)
                {
                    if (ReferenceEquals(_contacts[i].collider, _curGroundCollider))
                        continue;

                    //if either of these two conds are true, something is in our way
                    if (horizontal > 0 && _contacts[i].point.x > transform.position.x)
                    {
                        correctedHorizontalVel = 0;
                        break;
                    }
                    else if (horizontal < 0 && _contacts[i].point.x < transform.position.x)
                    {
                        correctedHorizontalVel = 0;
                        break;
                    }
                }
            }

            return correctedHorizontalVel;
        }

        bool CheckGrounded()
        {
            int numContacts = Physics2D.GetContacts(_collider, _groundColliderFilter, _contacts);
#if UNITY_EDITOR
            _DEBUG_allGroundHits = new ContactPoint2D[numContacts];
            if (numContacts > 0)
                Array.Copy(_contacts, 0, _DEBUG_allGroundHits, 0, numContacts);
#endif

            if (numContacts > 0)
            {
                for (int i=0; i< numContacts; i++)
                {
                    if (Vector2.Angle(_contacts[i].normal, Vector2.up) > _maxAllowedCheckGroundAngle)
                        continue;

                    var collider = _contacts[i].collider;
                    if (!collider.isTrigger && !Object.ReferenceEquals(collider, _collider))
                    {
                        _curGroundCollider = collider;
                        return true;
                    }
                }
            }

            return false;
        }

        void InteractionUpdate()
        {
            if (!controlEnabled)
                return;

            if(Input.GetKeyDown(KeyCode.E) && curInteractable)
            {
                curInteractable.OnInteract();
            }

            bool canGoOut = true;
            if (GameContext.s_gameMgr.curRoom.roomIndex == GameConst.k_startingRoomIndex)
            {
                canGoOut = _canExitStartingRoom.val;
            }
            else if (GameContext.s_gameMgr.curRoom.roomIndex > GameConst.k_startingRoomIndex)
            {
                canGoOut = _canExitSmallRoom.val;
            }

            if(canGoOut)
            {
                bool exitPaintingkeyDown = Input.GetKey(KeyCode.Q);
                //go out of the painting
                if (_interactHoldKeyDownLastFrame && exitPaintingkeyDown)
                {
                    _interactHoldTimer = Mathf.Min(_exitPaintInteractTime, _interactHoldTimer + Time.deltaTime);
                    GameContext.s_effectMgr.SetProgress(_interactHoldTimer / _exitPaintInteractTime);

                    if (Mathf.Approximately(_interactHoldTimer, _exitPaintInteractTime))
                    {
                        _interactHoldTimer = 0;
                        GameContext.s_effectMgr.HideProgressBar();

                        //TODO redo this check here
                        if (GameContext.s_gameMgr.curRoom.roomIndex == 2)
                        {
                            DialogueMenu.Instance.DisplaySimplePrompt("Message", "I will fall to death", null, "Ok then");
                        }
                        else
                        {
                            GameContext.s_gameMgr.curRoom.GoToPrev();
                        }
                    }
                }
                else if (!_interactHoldKeyDownLastFrame && exitPaintingkeyDown)
                {
                    _interactHoldTimer = 0;
                    GameContext.s_effectMgr.ShowProgressBar((Vector2)transform.position + 0.5f * Vector2.down,
                        Quaternion.Euler(0, 0, -90), transform);
                    GameContext.s_effectMgr.SetProgress(0);
                }
                else if (_interactHoldKeyDownLastFrame && !exitPaintingkeyDown)
                {
                    GameContext.s_effectMgr.HideProgressBar();
                }

                _interactHoldKeyDownLastFrame = exitPaintingkeyDown;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;

            if(_DEBUG_allGroundHits != null && _DEBUG_allGroundHits.Length > 0)
            {
                foreach (var hit in _DEBUG_allGroundHits)
                    Gizmos.DrawLine(hit.point, hit.point + hit.normal);
            }
        }

        private void OnGUI()
        {
            GUI.color = Color.green;
            GUILayout.Label("===========PlayerController===========");
            GUILayout.Label($"\tvelocity: {_rgbody.velocity.ToString()}");
            GUILayout.Label(_grounded ? $"\tcur ground collider: {_curGroundCollider.name}" : "\tnot grounded");

            string contactInfo = "";
            foreach(var contact in _DEBUG_allGroundHits)
            {
                contactInfo += contact.collider.gameObject.name + ", ";
            }
            GUILayout.Label($"\tcur contacts: {contactInfo}");

            if (_curInteractable)
                GUILayout.Label($"\tcur interactable: {_curInteractable.gameObject.name}");
        }
#endif
    }
}