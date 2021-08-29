using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [CreateAssetMenu(menuName = "PuzzleGame/Arrow Prompt Def")]
    public class ArrowDef : ScriptableObject
    {
        public Sprite sprite;
        public RuntimeAnimatorController animControl;
        public bool flipX, flipY;
    }

}