using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class SpeakerPuzzleCanvas : InspectionCanvas
    {
        class ColliderComparer : IComparer<Collider2D>
        {
            int IComparer<Collider2D>.Compare(Collider2D x, Collider2D y)
            {
                return y.transform.localPosition.y.CompareTo(x.transform.localPosition.y);
            }
        }

        [SerializeField] BoolVariable _isComputerOn;
        [SerializeField] float _successSeqStepTime;
        [SerializeField] SpeakerBlade[] _blades;
        [SerializeField] EdgeCollider2D _goalCheckCollider;
        [SerializeField] Button _resetButton;

        SpeakerPuzzleNode[] _nodes;

        Collider2D[] _middleNodes;
        ContactFilter2D _filter;

        protected override void Awake()
        {
            base.Awake();

            _middleNodes = new Collider2D[10];

            _filter = new ContactFilter2D();
            _filter.useTriggers = true;
            _filter.layerMask = 1 << GameConst.k_UILayer;

            _resetButton.onClick.AddListener(ResetPuzzle);

            foreach(var blade in _blades)
            {
                blade.puzzleCanvas = this;
            }

            _nodes = GetComponentsInChildren<SpeakerPuzzleNode>();
        }

        public void CheckGoalState()
        {
            int overlapCount = _goalCheckCollider.OverlapCollider(_filter, _middleNodes);
            Array.Sort(_middleNodes, 0, overlapCount, new ColliderComparer());

            StringBuilder builder = new StringBuilder();
            List<SpeakerPuzzleNode> nodes = new List<SpeakerPuzzleNode>();
            for (int i = 0; i < overlapCount; i++)
            {
                var node = _middleNodes[i].GetComponent<SpeakerPuzzleNode>();
                if (node)
                {
                    builder.Append(node.letter);
                    nodes.Add(node);
                }
            }

            string str = builder.ToString().ToLower();
            if (str.Equals("pizza"))
            {
                StartCoroutine(_successRoutine(nodes, _successSeqStepTime));
            }
        }
        public void ResetPuzzle()
        {
            ReturnAllBlades();

            for (int i = 0; i < _blades.Length; i++)
            {
                _blades[i].zRotation = 0;
            }

            foreach(var node in _nodes)
            {
                node.ResetNode();
            }
        }

        public void ReturnAllBlades()
        {
            for (int i = 0; i < _blades.Length; i++)
            {
                if (_blades[i].isActive)
                {
                    _blades[i].placePoint.GiveItem();
                }

                _blades[i].SetBladeActive(false);
            }
        }


        IEnumerator _successRoutine(List<SpeakerPuzzleNode> middleNodes, float stepTime)
        {
            ReturnAllBlades();
            //no need to sync state here as the puzzle becomes uninteractable

            Color color = Color.green;
            foreach (var node in middleNodes)
            {
                node.SetColor(color);
                yield return new WaitForSeconds(stepTime);
            }

            foreach (var node in middleNodes)
            {
                node.SetColor(color);
            }

            _inspectable.canInspect = false;
            _isComputerOn.val = true;
        }
    }
}