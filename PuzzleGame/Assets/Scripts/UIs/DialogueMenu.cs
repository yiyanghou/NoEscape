using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using PuzzleGame.EventSystem;

using UltEvents;

namespace PuzzleGame.UI
{
    public class DialogueMenu : SingletonGameMenu<DialogueMenu>
    {
        class DialogueBufferEntry
        {
            public DialogueDef def;
            public int cur = 0;
        }

        [SerializeField] GameObject _dialoguePanel;
        [SerializeField] Text _dialogueText;
        [SerializeField] Button _dialogueButton;
        Text _dialogueButtonText;

        [SerializeField] GameObject _promptPanel;
        [SerializeField] GameObject _picturePromptContentRoot, _textPromptContentRoot;
        [SerializeField] Text _promptTitleText;
        [SerializeField] Text _promptText;
        [SerializeField] Text _promptImageText;
        [SerializeField] Image _promptImage;
        [SerializeField] GridLayoutGroup _promptLayoutGroup;
        Button _backButton;
        Button[] _optionButtons;

        #region Audio
        [SerializeField] AudioClip _promptPopupSound;
        [SerializeField] AudioClip _dialoguePopupSound;
        #endregion

        //buffered dialogues
        Stack<PromptDef> _prompts;
        DialogueBufferEntry _curDialogue = null;
        Queue<DialogueBufferEntry> _bufferedDialogues = new Queue<DialogueBufferEntry>();

        public void DisplayDialogue(DialogueDef dialogueDef)
        {
            if(dialogueDef)
            {
                //if the dialogue box is inactive, there shouldn't be any dialogues left in the buffer
                Debug.Assert(!(!_dialoguePanel.activeSelf && _bufferedDialogues.Count > 0));


                DialogueBufferEntry entry = new DialogueBufferEntry { def = dialogueDef };
                //activate the dialogue box and advance the dialogue if there is no dialogue yet
                if (!_dialoguePanel.activeSelf)
                {
                    Messenger.Broadcast(M_EventType.ON_CHANGE_PLAYER_CONTROL, new PlayerControlEventData(false));

                    _dialoguePanel.SetActive(true);
                    _curDialogue = entry;
                    OnPressDialogueButton();
                }
                else
                {
                    _bufferedDialogues.Enqueue(entry);
                }

                GameContext.s_UIMgr.OpenMenu(Instance);
                GameActions.PlaySounds(_dialoguePopupSound);
            }
        }
        void FinishDialogue(DialogueDef def)
        {
            def.onDialogueFinishEvents?.Invoke();
            def.hasPlayed = true;
        }
        void FinishPrompt(PromptDef def)
        {
            def.hasPlayed = true;
        }

        void OnPressDialogueButton()
        {
            if (_curDialogue.cur == _curDialogue.def.dialogues.Length)
            {
                Messenger.Broadcast(M_EventType.ON_CHANGE_PLAYER_CONTROL, new PlayerControlEventData(true));

                if (_bufferedDialogues.Count > 0)
                {
                    FinishDialogue(_curDialogue.def);
                    _curDialogue = _bufferedDialogues.Dequeue();
                }
            }

            if (_curDialogue.cur < _curDialogue.def.dialogues.Length)
            {
                _dialogueText.text = _curDialogue.def.dialogues[_curDialogue.cur++];

                //nothing left
                if (_curDialogue.cur == _curDialogue.def.dialogues.Length && _bufferedDialogues.Count == 0)
                {
                    _dialogueButtonText.text = "Close";
                }
                else
                {
                    _dialogueButtonText.text = "Next";
                }
            }
            else
            {
                CloseDialogue();
            }
        }

        public void DisplayPrompt(PromptDef promptDef)
        {
            _prompts.Push(promptDef);
            _promptPanel.SetActive(true);
            DisplayPromptInternal(promptDef);
            GameContext.s_UIMgr.OpenMenu(Instance);

            if (promptDef.popUpSoundOverride)
                GameActions.PlaySounds(promptDef.popUpSoundOverride);
            else
                GameActions.PlaySounds(_promptPopupSound);
        }

        public void DisplaySimplePrompt(string title, string prompt, Sprite image, string back)
        {
            var ins = ScriptableObject.CreateInstance<PromptDef>();
            ins.title = title;
            ins.prompt = prompt;
            ins.backButtonName = back;
            ins.promptImage = image;

            DisplayPrompt(ins);
        }

        private void DisplayPromptInternal(PromptDef promptDef)
        {
            _promptTitleText.text = promptDef.title;
            Sprite image = promptDef.promptImage;
            string prompt = promptDef.prompt;
            var options = promptDef.options;

            _textPromptContentRoot.SetActive(!image);
            _picturePromptContentRoot.SetActive(image);

            if (!image)
            {
                _promptText.text = prompt;
            }
            else
            {
                _promptImage.sprite = image;
                _promptImageText.text = prompt;
            }

            int i = 0;
            if (options != null)
            {
                Debug.Assert(options.Length + 1 <= _optionButtons.Length);

                for (; i < options.Length; i++)
                {
                    var button = _optionButtons[i];
                    var optionDesc = options[i];

                    button.gameObject.SetActive(true);
                    button.transform.parent.SetParent(_promptLayoutGroup.transform, false);
                    button.GetComponentInChildren<Text>().text = optionDesc.optionName;

                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.AddListener(() =>
                    {
                        optionDesc.optionEvents?.Invoke();
                    });
                }
            }

            while (i < _optionButtons.Length)
            {
                _optionButtons[i++].gameObject.SetActive(false);
            }

            if (promptDef.hasBackButton && promptDef.backButtonName != null)
            {
                _backButton.gameObject.SetActive(true);
                _backButton.GetComponentInChildren<Text>().text = promptDef.backButtonName;
            }
            else
            {
                _backButton.gameObject.SetActive(false);
            }
        }

        protected override void Awake()
        {
            base.Awake();

            _prompts = new Stack<PromptDef>();

            //prompt button pool
            _optionButtons = _promptLayoutGroup.GetComponentsInChildren<Button>();
            foreach (var button in _optionButtons)
                button.gameObject.SetActive(false);

            //config back button
            _backButton = Instantiate(_optionButtons[0]);
            _backButton.transform.SetParent(_promptLayoutGroup.transform, false);
            _backButton.transform.localScale = Vector3.one;
            _backButton.GetComponentInChildren<Text>().text = "Back";
            _backButton.onClick.AddListener(ClosePrompt);

            //dialogue button
            _dialogueButtonText = _dialogueButton.GetComponentInChildren<Text>();
            _dialogueButton.onClick.AddListener(OnPressDialogueButton);
            //start with nothing
            _dialoguePanel.SetActive(false);
            _promptPanel.SetActive(false);
        }

        public override bool CanClose()
        {
            return _prompts.Count == 0 && _curDialogue == null && _bufferedDialogues.Count == 0;
            /*
            foreach(var prompt in _prompts)
            {
                if (!prompt.skippable)
                    return false;
            }

            foreach(var dialogueBuffer in _bufferedDialogues)
            {
                if (!dialogueBuffer.def.skippable)
                    return false;
            }
            if (_curDialogue != null && !_curDialogue.def.skippable)
                return false;

            return true;
            */
        }

        public override void OnLeaveMenu()
        {
            //TODO: hotfix, brute force close everything, will come back to this if time permits

            bool hasOngoingDialogue = false;

            //clear all prompts
            while (_prompts.Count > 0)
            {
                FinishPrompt(_prompts.Pop());
            }
            
            _promptPanel.SetActive(false);

            //clear current dialogue
            if(_curDialogue != null)
            {
                FinishDialogue(_curDialogue.def);
                _curDialogue = null;

                hasOngoingDialogue = true;
            }
            _dialoguePanel.SetActive(false);

            //flush other buffered dialogues
            if (_bufferedDialogues.Count > 0)
            {
                foreach(var buffer in _bufferedDialogues)
                {
                    FinishDialogue(buffer.def);
                }
                _bufferedDialogues.Clear();

                hasOngoingDialogue = true;
            }

            //release control
            if(hasOngoingDialogue)
            {
                Messenger.Broadcast(M_EventType.ON_CHANGE_PLAYER_CONTROL, new PlayerControlEventData(true));
            }

            base.OnLeaveMenu();
        }

        public void ClosePrompt()
        {
            if(_prompts.Count > 0)
            {
                FinishPrompt(_prompts.Pop());
            }

            if(_prompts.Count == 0)
            {
                _promptPanel.SetActive(false);
            }
            else
            {
                DisplayPromptInternal(_prompts.Peek());
            }

            if (!_promptPanel.activeSelf && !_dialoguePanel.activeSelf)
            {
                OnBackPressed();
            }
        }
        public void CloseDialogue()
        {
            if (_curDialogue != null)
            {
                FinishDialogue(_curDialogue.def);
                _curDialogue = null;
            }

            _dialoguePanel.SetActive(false);

            if (!_promptPanel.activeSelf && !_dialoguePanel.activeSelf)
            {
                OnBackPressed();
            }
        }

        public override void OnEnterMenu()
        {
            base.OnEnterMenu();
        }
    }
}
