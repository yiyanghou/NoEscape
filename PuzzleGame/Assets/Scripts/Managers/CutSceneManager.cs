using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Playables;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using System.Linq;
using System;

namespace PuzzleGame
{
    public class CutSceneManager : MonoBehaviour
    {
        [Serializable]
        class CutSceneDesc
        {
            [NonSerialized][HideInInspector] public bool hasPlayed;
            public Condition condition;
            public PlayableDirector director;
        }

        [SerializeField] CutSceneDesc[] _cutScenes;
        [SerializeField] Camera _cutSceneCam;

        bool _hasOnGoingCutScene;

        private void Awake()
        {
            foreach (var desc in _cutScenes)
            {
                desc.director.extrapolationMode = DirectorWrapMode.None;
                desc.director.playOnAwake = false;
                desc.director.stopped += Director_stopped;
            }
            _cutSceneCam.gameObject.SetActive(false);
            _hasOnGoingCutScene = false;
        }

        private void Director_stopped(PlayableDirector director)
        {
            Messenger.Broadcast(M_EventType.ON_CUTSCENE_END, new CutSceneEventData(director.playableAsset as TimelineAsset));
            GameContext.s_gameMgr.OnEndCutScene(director.playableAsset as TimelineAsset);
            _hasOnGoingCutScene = false;
        }

        public void Play(TimelineAsset timeline)
        {
            PlayableDirector targetDirector = null;
            foreach (var desc in _cutScenes)
            {
                if(ReferenceEquals(desc.director.playableAsset, timeline))
                {
                    targetDirector = desc.director;
                    break;
                }
            }

            Debug.Assert(targetDirector);
            Play(targetDirector);
        }

        public void Play(PlayableDirector director)
        {
            director.Play();
            Messenger.Broadcast(M_EventType.ON_CUTSCENE_START, new CutSceneEventData((TimelineAsset)director.playableAsset));
            _hasOnGoingCutScene = true;
        }

        public void CheckCutScenes()
        {
            if(!_hasOnGoingCutScene)
            {
                foreach (var desc in _cutScenes)
                {
                    if (!desc.hasPlayed)
                    {
                        if (desc.condition && desc.condition.Evaluate())
                        {
                            Play(desc.director);
                            desc.hasPlayed = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
