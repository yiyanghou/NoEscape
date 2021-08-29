using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Canvas))]
    public abstract class InspectionCanvas : GameMenu, IAnimationClipSource
    {
        [Tooltip("for things that rotate with the inspectable")]
        [SerializeField] protected Transform _rotationRoot;
        [SerializeField] protected Button _backButton;
        protected Canvas _canvas;
        protected RectTransform _rectTransform;
        protected Inspectable _inspectable;

        public void GetAnimationClips(List<AnimationClip> results)
        {
            FieldInfo[] objMember = GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach(var info in objMember)
            {
                if (info.FieldType == typeof(AnimationClip))
                {
                    results.Add((AnimationClip)info.GetValue(this));
                }
            }
        }

        public virtual void Open(Inspectable inspectable)
        {
            SetInspectable(inspectable);
            GameContext.s_UIMgr.OpenMenu(this);
        }

        /// <summary>
        /// set per inspectable instance data here
        /// </summary>
        /// <param name="inspectable"></param>
        protected virtual void SetInspectable(Inspectable inspectable)
        {
            _inspectable = inspectable;
            
            Vector2 offset = _inspectable.inspectionCamera.transform.localPosition;
            _rotationRoot.localPosition = -offset;

            UpdateRotation();
        }

        protected void UpdateRotation()
        {
            //camera rotates with the inspectable, so canvas should not rotate (because from the camera's view, the inspectable is not rotated)
            if (_inspectable.enableInspectCamRotation)
            {
                _rotationRoot.localRotation = Quaternion.identity;
            }
            //camera has no rotation, canvas should rotate (because it represents the inspectable)
            else
            {
                _rotationRoot.localRotation = Quaternion.Euler(0, 0, _inspectable.transform.eulerAngles.z);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _canvas = GetComponent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
        }

        protected override void Start()
        {
            base.Start();
            if (_backButton)
            {
                _backButton.onClick.AddListener(OnBackPressed);
            }
        }

        public override void OnBackPressed()
        {
            if (GameContext.s_UIMgr != null && ReferenceEquals(GameContext.s_UIMgr.GetActiveMenu(), this))
            {
                GameContext.s_UIMgr.CloseCurrentMenu();
            }
        }

        public override void OnEnterMenu()
        {
            gameObject.SetActive(true);
        }

        public override void OnLeaveMenu()
        {
            gameObject.SetActive(false);
            _inspectable.EndInspect();
        }
    }
}