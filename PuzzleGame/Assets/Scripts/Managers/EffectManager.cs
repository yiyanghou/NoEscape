using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    /// <summary>
    /// entry point for all visual effect sprites
    /// for now all effect sprites are "singletons"
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        [SerializeField] GameObject _arrowGameObj;
        [SerializeField] ProgressBar _progressBar;

        private void Awake()
        {
            if (GameContext.s_effectMgr != null)
                Destroy(this);
            else
                GameContext.s_effectMgr = this;
        }

        private void Start()
        {
            _arrowGameObj = Instantiate(_arrowGameObj);
            _progressBar = Instantiate(_progressBar.gameObject).GetComponent<ProgressBar>();

            _arrowGameObj.SetActive(false);
            _progressBar.gameObject.SetActive(false);
        }

        public void HideArrow()
        {
            _arrowGameObj.SetActive(false);
        }

        public void HideProgressBar()
        {
            _progressBar.transform.parent = null;
            _progressBar.gameObject.SetActive(false);
        }

        public void ShowArrow(ArrowDef def, bool animate, Vector2 position, Quaternion rotation, bool flipX = false, bool flipY = false)
        {
            CorrectToRoomScale(_arrowGameObj.transform);

            SpriteRenderer rend = _arrowGameObj.GetComponentInChildren<SpriteRenderer>();
            rend.sprite = def.sprite;
            rend.flipX = flipX;
            rend.flipY = flipY;

            Animator anim = _arrowGameObj.GetComponentInChildren<Animator>();
            if (def.animControl && animate)
            {
                anim.runtimeAnimatorController = def.animControl;
                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            }
            else
            {
                anim.runtimeAnimatorController = null;
            }

            _arrowGameObj.SetActive(true);
            _arrowGameObj.transform.position = position;
            _arrowGameObj.transform.rotation = rotation;
        }

        public void ShowProgressBar(Vector2 pos, Quaternion rotation, Transform parent)
        {
            CorrectToRoomScale(_progressBar.transform);

            _progressBar.gameObject.SetActive(true);
            _progressBar.transform.position = pos;
            _progressBar.transform.rotation = rotation;
            _progressBar.transform.parent = parent;
        }

        public void SetProgress(float progress)
        {
            _progressBar.SetProgress(progress);
        }

        private void CorrectToRoomScale(Transform trans)
        {
            Room room = GameContext.s_gameMgr.curRoom;
            Vector3 scale = new Vector3(room.roomScale, room.roomScale, 1f);
            trans.transform.localScale = scale;
        }
    }
}
