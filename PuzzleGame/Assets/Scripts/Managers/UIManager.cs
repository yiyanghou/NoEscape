using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using PuzzleGame.EventSystem;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UIManager : MonoBehaviour
    {
        //a linkedlist that behaves like a stack
        LinkedList<IGameMenu> _MenuStack = new LinkedList<IGameMenu>();
        List<IGameMenu> _MenuInstances = new List<IGameMenu>();

        [SerializeField] GameObject _cutSceneFrame;

        private void Awake()
        {
            if (GameContext.s_UIMgr != null)
                Destroy(this);
            else
                GameContext.s_UIMgr = this;

            Messenger.AddListener(M_EventType.ON_CUTSCENE_START, (CutSceneEventData data) => { StartCutScene(); });
            Messenger.AddListener(M_EventType.ON_CUTSCENE_END, (CutSceneEventData data) => { EndCutScene(); });
            EndCutScene();
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void RegisterMenu(IGameMenu menu)
        {
            bool found = false;
            foreach(var ins in _MenuInstances)
            {
                if(ReferenceEquals(ins, menu))
                {
                    found = true;
                }
            }

            if(!found)
            {
                _MenuInstances.Add(menu);
            }
        }

        public void OpenMenu(IGameMenu menuInstance)
        {
            if (menuInstance == null)
            {
                Debug.LogWarning("MenuManager.OpenMenu()- menu instance is null");
                return;
            }

            if (!menuInstance.CanOpen())
            {
                return;
            }

            if (_MenuStack.Count > 0)
            {
                for(var curNode = _MenuStack.First; curNode != null; curNode = curNode.Next)
                {
                    if(ReferenceEquals(curNode.Value, menuInstance))
                    {
                        //already opened
                        _MenuStack.Remove(curNode);
                        break;
                    }
                }

                if (_MenuStack.Count > 0)
                {
                    _MenuStack.First.Value.OnLoseFocus();
                }
            }

            menuInstance.OnEnterMenu();
            _MenuStack.AddFirst(menuInstance);
        }
        public void CloseCurrentMenu()
        {
            if (_MenuStack.Count == 0 || !GetActiveMenu().CanClose())
            {
                return;
            }

            IGameMenu topMenu = _MenuStack.First.Value;
            _MenuStack.RemoveFirst();
            topMenu.OnLeaveMenu();

            if (_MenuStack.Count > 0)
            {
                IGameMenu nextMenu = _MenuStack.First.Value;
                nextMenu.OnFocus();
            }
        }
        public IGameMenu GetActiveMenu()
        {
            if (_MenuStack.Count > 0)
            {
                return _MenuStack.First.Value;
            }
            else
            {
                return null;
            }
        }
        public int GetOpenMenuCount()
        {
            return _MenuStack.Count;
        }

        public void StartCutScene()
        {
            _cutSceneFrame.SetActive(true);
        }

        public void EndCutScene()
        {
            _cutSceneFrame.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(0, 150, 400, 400));
            GUI.color = Color.green;
            GUILayout.Label("===========UIManager===========");
            GUILayout.Label("\tmenu stack:");

            for(var node = _MenuStack.Last; node != null; node = node.Previous)
            {
                GUILayout.Label($"\t\t{node.Value.GetType().Name}");
            }
            GUILayout.EndArea();
        }
#endif
    }
}