using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [CreateAssetMenu(menuName = "PuzzleGame/Audio/AudioCollection")]
    public class AudioCollection : ScriptableObject
    {
        [SerializeField] AudioClip[] _clips;
        [SerializeField] bool _allowConsecutiveSameClip = false;

        AudioClip _lastClipPlayed;

        /// <summary>
        /// gets a specific clip
        /// </summary>
        /// <param name="i">the index of the clip in the collection</param>
        /// <returns>the clip reference</returns>
        public AudioClip this[int i]
        {
            get
            {
                return _clips[i];
            }
        }

        /// <summary>
        /// Randomly chooses a clip from bank i
        /// </summary>
        public AudioClip GetClip()
        {
            if (_clips == null || _clips.Length == 0)
                return null;

            AudioClip clip;
            if (!_allowConsecutiveSameClip && _clips.Length > 1)
            {
                do
                {
                    clip = _clips[UnityEngine.Random.Range(0, _clips.Length)];
                }
                while (ReferenceEquals(_lastClipPlayed, clip));

                _lastClipPlayed = clip;
            }
            else
            {
                clip = _clips[UnityEngine.Random.Range(0, _clips.Length)];
            }

            return clip;
        }
    }
}