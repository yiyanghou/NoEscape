using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Image))]
    public class OutlineElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] Color _outlineColor = Color.red;

        protected Image _img;
        protected bool _isMouseOver;

        protected virtual void Awake()
        {
            _img = GetComponent<Image>();

            //can't figure out a way to do per instance property block on UI so just make a new material
            _img.material = new Material(Shader.Find(GameConst.k_outlineShaderPath));
            _isMouseOver = false;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            _isMouseOver = true;
            _img.material.SetFloat("_Outline", 1f);
            _img.material.SetColor("_OutlineColor", _outlineColor);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            _isMouseOver = false;
            _img.material.SetFloat("_Outline", 0f);
        }
    }
}
