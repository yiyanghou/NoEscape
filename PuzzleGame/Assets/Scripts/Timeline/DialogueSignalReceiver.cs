using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace PuzzleGame
{
    public class DialogueSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        [SerializeField] DialogueEventEmitterPair[] _targetSignalAssets;

        void INotificationReceiver.OnNotify(Playable origin, INotification notification, object context)
        {
            if(notification is DialogueSignalEmitter dialogueSignalEmitter)
            {
                var matches = _targetSignalAssets.Where(x => ReferenceEquals(x.signalAsset, dialogueSignalEmitter.asset));

                foreach(var m in matches)
                {
                    m.dialogueEvent.Invoke(dialogueSignalEmitter.dialogueDef);
                }
            }
        }
    }
}
