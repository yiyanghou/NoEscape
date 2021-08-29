using System.Reflection;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using System;

using UltEvents;

namespace PuzzleGame
{
    public class Inspectable : Interactable
    {
        static Dictionary<int, InspectionCanvas> s_InspectionCanvasDict = null;

        [Header("Inspectable Config")]
        [SerializeField] protected Camera _inspectionCamera = null;
        [SerializeField] protected bool _enableInspectCamRotation = false;
        [SerializeField] protected UltEvent _successEvents;
        public UltEvent successEvents => _successEvents;

        public Camera inspectionCamera { get => _inspectionCamera; }
        public bool enableInspectCamRotation
        {
            get
            {
                return _enableInspectCamRotation;
            }
            set
            {
                _enableInspectCamRotation = value;

                if (_enableInspectCamRotation)
                {
                    _inspectionCamera.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    _inspectionCamera.transform.rotation = Quaternion.identity;
                }
            }
        }

        [SerializeField] protected DialogueDef _firstEncounterDialogue;
        //canvas for details on the object
        [SerializeField] protected InspectionCanvas _inspectionCanvasPrefab;

        protected bool _canInspect = true;
        public virtual bool canInspect
        {
            get => _canInspect;
            set
            {
                Actor[] actors = GameContext.s_gameMgr.GetAllActorsByID(actorId);
                foreach(var actor in actors)
                {
                    Debug.Assert(actor is Inspectable);
                    Inspectable ins = actor as Inspectable;
                    ins._canInspect = value;
                }                
            }
        }
        public override bool canInteract
        {
            get => _inspectionCanvasPrefab && _canInspect && base.canInteract;
        }

        protected override void Awake()
        {
            base.Awake();

            _interactionEvent.AddPersistentCall((Action)BeginInspect);

            if (s_InspectionCanvasDict == null)
                s_InspectionCanvasDict = new Dictionary<int, InspectionCanvas>();

            if (!s_InspectionCanvasDict.ContainsKey(actorId))
                s_InspectionCanvasDict.Add(actorId, Instantiate(_inspectionCanvasPrefab, null));
        }

        protected override void Start()
        {
            base.Start();

            _inspectionCamera.gameObject.SetActive(false);
            _inspectionCamera.orthographic = true;
            _inspectionCamera.cullingMask = ~(1 << GameConst.k_playerLayer);
            _inspectionCamera.orthographicSize *= room.roomScale;
        }

        public virtual void BeginInspect()
        {
            Debug.Assert(_canInspect);

            _inspectionCamera.gameObject.SetActive(true);
            enableInspectCamRotation = _enableInspectCamRotation;

            spriteRenderer.enabled = false;

            //open world space canvas
            GetInspectionCanvas().Open(this);

            //display first dialogue
            if (_firstEncounterDialogue && !_firstEncounterDialogue.hasPlayed)
            {
                DialogueMenu.Instance.DisplayDialogue(_firstEncounterDialogue);
            }

            Messenger.Broadcast(M_EventType.ON_CHANGE_PLAYER_CONTROL, new PlayerControlEventData(false));
            Messenger.Broadcast(M_EventType.ON_INSPECTION_START, new InspectionEventData(this));
        }


        /// <summary>
        /// this is called from the back button of screen space canvas
        /// </summary>
        public virtual void EndInspect()
        {
            if (!canInspect)
                _successEvents?.Invoke();

            _inspectionCamera.gameObject.SetActive(false);

            spriteRenderer.enabled = true;

            //enable player ctrl
            Messenger.Broadcast(M_EventType.ON_CHANGE_PLAYER_CONTROL, new PlayerControlEventData(true));
            Messenger.Broadcast(M_EventType.ON_INSPECTION_END, new InspectionEventData(this));
        }

        protected InspectionCanvas GetInspectionCanvas()
        {
            return s_InspectionCanvasDict[actorId];
        }

        protected virtual void Update()
        {

        }

        private void OnDrawGizmos()
        {
            
        }

        private void OnDestroy()
        {
            if(s_InspectionCanvasDict != null)
            {
                s_InspectionCanvasDict = null;
            }
        }
    }
}