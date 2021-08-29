using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PuzzleGame.UI;
using System;

namespace PuzzleGame
{
    public class SpeakerBlade : MonoBehaviour
    {
        static bool s_canRotate = true;

        public SpeakerPuzzleCanvas puzzleCanvas { get; set; }
        [SerializeField] Button _cwRotButton, _ccwRotButton;
        [SerializeField] ItemPlacePoint _bladePlacePoint;
        [SerializeField] Transform _radiusAnchor;
        [SerializeField] float _rotationStep;
        [SerializeField] float _rotationTime;
        [SerializeField] AudioCollection _rotationSound;

        Transform _rotationRoot;

        public ItemPlacePoint placePoint { get => _bladePlacePoint; }

        public float zRotation
        {
            get
            {
                return _rotationRoot.localRotation.eulerAngles.z;
            }
            set
            {
                _rotationRoot.localRotation = Quaternion.Euler(0, 0, value);
            }
        }

        public bool isActive { get; private set; }
        
        private void Awake()
        {
            _rotationRoot = _bladePlacePoint.transform;
            _cwRotButton.gameObject.SetActive(false);
            _ccwRotButton.gameObject.SetActive(false);

            _cwRotButton.onClick.AddListener(() => 
            {
                if(s_canRotate)
                    Rotate(true, _rotationTime); 
            });

            _ccwRotButton.onClick.AddListener(() => 
            {
                if (s_canRotate)
                    Rotate(false, _rotationTime); 
            });

            _bladePlacePoint.onSuccessDrop.AddPersistentCall((Action) (() =>
            {
                _cwRotButton.gameObject.SetActive(true);
                _ccwRotButton.gameObject.SetActive(true);
                isActive = true;
            }));
            _bladePlacePoint.onPickupItem.AddPersistentCall((Action)(() =>
            {
                _cwRotButton.gameObject.SetActive(false);
                _ccwRotButton.gameObject.SetActive(false);
                isActive = false;
            }));
        }

        public void SetBladeActive(bool active)
        {
            _cwRotButton.gameObject.SetActive(active);
            _ccwRotButton.gameObject.SetActive(active);
            _bladePlacePoint.hasItem = active;
            isActive = active;
        }

        public void Rotate(bool clockwise, float rotationTime)
        {
            if (!s_canRotate || !_bladePlacePoint.hasItem)
                return;

            List<Transform> nodes = new List<Transform>(4);
            float radius = Vector2.Distance(_radiusAnchor.position, _rotationRoot.position);

            void DoRaycast(Vector2 dir)
            {
                var results = Physics2D.RaycastAll(_rotationRoot.position, dir, radius, 1 << GameConst.k_UILayer);
                foreach (var hit in results)
                {
                    if (hit.transform.GetComponent<SpeakerPuzzleNode>())
                    {
                        nodes.Add(hit.transform);
                    }
                }
            }

            IEnumerator _RotateRoutine()
            {
                s_canRotate = false;

                GameActions.PlaySounds(_rotationSound);

                float start = _rotationRoot.localRotation.eulerAngles.z;
                float end;

                if (clockwise)
                    end = start - _rotationStep;
                else
                    end = start + _rotationStep;

                Vector2[] relativePositions = new Vector2[nodes.Count];
                for(int i=0; i< relativePositions.Length; i++)
                {
                    relativePositions[i] = _rotationRoot.InverseTransformPoint(nodes[i].position);
                }

                if(!Mathf.Approximately(0, rotationTime))
                {
                    for (float t = 0; t < rotationTime; t += Time.deltaTime)
                    {
                        _rotationRoot.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(start, end, Mathf.Min(1f, t / rotationTime)));

                        for (int i = 0; i < relativePositions.Length; i++)
                        {
                            nodes[i].position = _rotationRoot.TransformPoint(relativePositions[i]);
                        }

                        yield return new WaitForEndOfFrame();
                    }
                }

                _rotationRoot.localRotation = Quaternion.Euler(0, 0, end);
                for (int i = 0; i < relativePositions.Length; i++)
                {
                    nodes[i].position = _rotationRoot.TransformPoint(relativePositions[i]);
                }

                s_canRotate = true;
                puzzleCanvas.CheckGoalState();
            }

            DoRaycast(_rotationRoot.up);
            DoRaycast(-_rotationRoot.up);
            DoRaycast(_rotationRoot.right);
            DoRaycast(-_rotationRoot.right);

            if(nodes.Count > 0)
            {
                StartCoroutine(_RotateRoutine());
            }
        }
    }
}
