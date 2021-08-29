using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] Transform _barHolder;

        private void Awake()
        {
            SetProgress(0);
        }

        public void SetProgress(float progress)
        {
            Vector3 scale = _barHolder.transform.localScale;
            scale.x = Mathf.Clamp(progress, 0, 1);
            _barHolder.localScale = scale;
        }
    }
}
