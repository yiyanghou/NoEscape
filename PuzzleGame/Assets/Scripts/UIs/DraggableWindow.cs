using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using UltEvents;
using UnityEngine.Events;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(RectTransform), typeof(Image))]
    public class DraggableWindow : MonoBehaviour, 
        IBeginDragHandler, 
        IDragHandler
    {
        public RectTransform boundingRect;

        RectTransform _rect;
        Vector2 _relativeMousePos;

        Vector2 _resolution;
        Vector2 _minCorner, _maxCorner;


        void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.enterEventCamera, out _relativeMousePos);
        }

        void IDragHandler.OnDrag(PointerEventData eventData)
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.enterEventCamera, out pos);
            _rect.position = (Vector2)_rect.TransformPoint(pos - _relativeMousePos);

            pos = _rect.position;
            pos.Set(
                Mathf.Clamp(_rect.position.x, _minCorner.x, _maxCorner.x),
                Mathf.Clamp(_rect.position.y, _minCorner.y, _maxCorner.y));
            _rect.position = pos;
        }

        private void OnScreenResize()
        {
            _resolution = new Vector2(Screen.width, Screen.height);
            Rect rect = _rect.rect;
            Rect bound = boundingRect.rect;
            _minCorner = boundingRect.TransformPoint(-bound.size / 2 + rect.size / 2);
            _maxCorner = boundingRect.TransformPoint(bound.size / 2 - rect.size / 2);
        }

        private void OnDrawGizmos()
        {

        }


        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        // Start is called before the first frame update
        void Start()
        {
            OnScreenResize();
        }

        // Update is called once per frame
        void Update()
        {
            if(!Mathf.Approximately(_resolution.x, Screen.width) || !Mathf.Approximately(_resolution.y, Screen.height))
            {
                OnScreenResize();
            }
        }
    }
}
