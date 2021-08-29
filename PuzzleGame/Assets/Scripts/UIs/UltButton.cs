using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UltEvents;


namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Button))]
    public class UltButton : MonoBehaviour
    {
        public UltEvent onClick;

        // Start is called before the first frame update
        void Awake()
        {
            var button = GetComponent<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(() => { onClick?.Invoke(); });
        }
    }
}