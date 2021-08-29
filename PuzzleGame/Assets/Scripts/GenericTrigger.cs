using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [RequireComponent(typeof(Collider2D))]
    public class GenericTrigger : MonoBehaviour
    {
        private Collider2D _collider;

        public delegate void TriggerHandler(Collider2D collider);
        private TriggerHandler _onTriggerEnter;
        private TriggerHandler _onTriggerStay;
        private TriggerHandler _onTriggerExit;

        private void Set(TriggerHandler func, ref TriggerHandler mem)
        {
            if (mem != null)
                throw new InvalidOperationException("Only one eventhandler is supported");
            mem = func;
        }
        private void UnSet(TriggerHandler func, ref TriggerHandler mem)
        {
            // you might want to check if the delegate matches the current.
            if (func == null || func == mem)
                mem = null;
            else
                throw new InvalidOperationException("Unable to unregister, wrong eventhandler");
        }

        public event TriggerHandler onTriggerEnter
        {
            add => Set(value, ref _onTriggerEnter);
            remove => UnSet(value, ref _onTriggerEnter);
        }
        public event TriggerHandler onTriggerStay
        {
            add => Set(value, ref _onTriggerStay);
            remove => UnSet(value, ref _onTriggerStay);
        }
        public event TriggerHandler onTriggerExit
        {
            add => Set(value, ref _onTriggerExit);
            remove => UnSet(value, ref _onTriggerExit);
        }

        //we don't use unity callback here
        //because onTriggerXX can only detect one collider
        /*
        private void FixedUpdate()
        {
            int count = _collider.OverlapCollider(_filter, _overlapResults);
            IEnumerable<Collider2D> curOverlap = _overlapResults.Take(count);
            IEnumerable<Collider2D> lastOverlap = _lastOverlapResults.Take(_lastOverlapCnt);

            IEnumerable<Collider2D> intersection = curOverlap.Intersect(lastOverlap);
            IEnumerable<Collider2D> onlyInLast = lastOverlap.Except(curOverlap);
            IEnumerable<Collider2D> onlyInCur = curOverlap.Except(lastOverlap);

            foreach (var collider in intersection)
                _onTriggerStay?.Invoke(collider);
            foreach (var collider in onlyInLast)
                _onTriggerExit?.Invoke(collider);
            foreach (var collider in onlyInCur)
                _onTriggerEnter?.Invoke(collider);

            Array.Copy(_overlapResults, 0, _lastOverlapResults, 0, count);
            _lastOverlapCnt = count;
        }
        */

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            _onTriggerEnter?.Invoke(collision);
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            _onTriggerStay?.Invoke(collision);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            _onTriggerExit?.Invoke(collision);
        }
    }
}
