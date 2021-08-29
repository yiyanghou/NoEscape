using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;

namespace PuzzleGame.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class ComputerNumberText : MonoBehaviour
    {
        TextMeshProUGUI _numberText;
        string _originalText;

        private void Awake()
        {
            _numberText = GetComponent<TextMeshProUGUI>();
            _originalText = _numberText.text;
        }

        public void EncodeText(ComputerPuzzleCanvas.PasswordMappingDef def)
        {
            StringBuilder encodeBuffer = new StringBuilder(_originalText);

            for(int i=0; i<_originalText.Length; i++)
            {
                if (!int.TryParse(_originalText[i].ToString(), out int charIntVal))
                    continue;

                foreach(var pair in def.mapping)
                {
                    if(pair.i == charIntVal)
                    {
                        encodeBuffer[i] = pair.c;
                        break;
                    }
                }
            }

            _numberText.text = encodeBuffer.ToString();
        }
    }
}
