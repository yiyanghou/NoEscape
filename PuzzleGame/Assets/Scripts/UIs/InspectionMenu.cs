using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PuzzleGame.UI
{
    public class InspectionMenu : SingletonGameMenu<InspectionMenu>
    {
        [SerializeField] Button[] _buttons;
    }
}
