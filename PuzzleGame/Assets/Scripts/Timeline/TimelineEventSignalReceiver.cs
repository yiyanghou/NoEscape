using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace PuzzleGame
{
    public class TimelineEventSignalReceiver : MonoBehaviour, INotificationReceiver
    {
        [SerializeField] TimelineEventPair[] _targetSignalAssets;

        void INotificationReceiver.OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SignalEmitter emitter)
            {
                var matches = _targetSignalAssets.Where(x => ReferenceEquals(x.signalAsset, emitter.asset));

                foreach (var m in matches)
                {
                    m.events.Invoke();
                }
            }
        }
    }
}
