using System;
using System.Linq;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using PuzzleGame.EventSystem;
using PuzzleGame.UI;
using UnityEngine.Playables;
using UnityEngine.UI;

using UltEvents;
using UnityEngine.SceneManagement;

namespace PuzzleGame
{
    public enum EGameEndingType
    {
        DEATH,
        ESCAPE
    }

    public class GameManager : MonoBehaviour
    {
        enum EGameState
        {
            NONE,
            RUNNING,
        }

        public Room roomPrefab = null;
        public Player playerPrefab = null;

        class ObjectEvents<T>
        {
            public T objectRef;
            public UltEvent events;
        }

        #region Game Audio
        [Header("Audio Configuration")]
        [SerializeField] float _bgmVolume;
        [SerializeField] AudioClip _mainMenuClip;
        [SerializeField] AudioClip _gameClip;
        [SerializeField] AudioClip _enterRoomClip;
        [SerializeField] AudioClip _leaveRoomClip;
        [SerializeField] AudioClip _initRoomClip;
        #endregion

        #region Game Logic
        [Serializable]
        class DialogueEvents : ObjectEvents<DialogueDef> { }
        [Serializable]
        class CutSceneEvents : ObjectEvents<TimelineAsset> { }
        [Serializable]
        class PromptEvents : ObjectEvents<PromptDef> { }
        
        [Header("Game Logic Configuration")]
        [SerializeField] CutSceneEvents[] _cutSceneEvents;
        [SerializeField] UltEvent _enterRoomEvents;

        #endregion

        EGameState _gameState = EGameState.NONE;
        public Room curRoom { get; set; } = null;

        private void Awake()
        {
            if (GameContext.s_gameMgr != null)
                Destroy(this);
            else
                GameContext.s_gameMgr = this;

            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener<RoomEventData>(M_EventType.ON_BEFORE_ENTER_ROOM, OnBeforeEnterRoom);
        }

        private void Start()
        {
            //play main menu theme
            GameContext.s_audioMgr.PlayConstantSound(_mainMenuClip, _bgmVolume);
        }

        private void Update()
        {
            if(_gameState == EGameState.RUNNING)
            {
                curRoom.UpdateRoom();
            }
        }

        public void StartGame()
        {
            curRoom = Room.SpawnChain(GameConst.k_totalNumRooms, GameConst.k_startingRoomIndex);
            _gameState = EGameState.RUNNING;

            //play game theme
            GameContext.s_audioMgr.PlayConstantSound(_gameClip, _bgmVolume);
            GameContext.s_audioMgr.PlayOneShotSound(_initRoomClip, transform.position, 1);

            Messenger.Broadcast(M_EventType.ON_GAME_START);
        }

        public void RestartGame()
        {
            _gameState = EGameState.NONE;
            GameContext.Flush();

            Messenger.Broadcast(M_EventType.ON_GAME_RESTART);
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        public void QuitGame()
        {
            _gameState = EGameState.NONE;
            Application.Quit();
        }

        public void TriggerEnding(EGameEndingType type)
        {
            Messenger.Broadcast(M_EventType.ON_GAME_END, new GameEndEventData(type));
        }

        /// <summary>
        /// return all instances of the same actor in each room
        /// </summary>
        /// <param name="actorId"></param>
        /// <returns></returns>
        public Actor[] GetAllActorsByID(int actorId)
        {
            Actor[] ret = new Actor[GameConst.k_totalNumRooms];
            Room room = curRoom;

            while (room.roomIndex != 0)
            {
                room = room.prev;
            }

            while (room != null)
            {
                ret[room.roomIndex] = room.GetActorByID(actorId);
                room = room.next;
            }

            return ret;
        }

        /// <summary>
        /// returns all instances of an actor of type T, grouped by actor id (first dimension)
        /// e.g. ret[i] contains all instances with id = i
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<IGrouping<int, Actor>> GetAllActorsByType<T>()
        {
            Room room = curRoom;
            List<Actor> result = new List<Actor>();

            while (room.roomIndex != 0)
            {
                room = room.prev;
            }

            while (room != null)
            {
                result.AddRange(room.GetActorsByType<T>());
                room = room.next;
            }

            return result.GroupBy(x => x.actorId);
        }

        public Actor[] GetAllActorsByName(string name)
        {
            Room room = curRoom;
            Actor[] ret = new Actor[GameConst.k_totalNumRooms];

            while (room.roomIndex != 0)
            {
                room = room.prev;
            }

            while (room != null)
            {
                ret[room.roomIndex] = room.GetActorByName(name);
                room = room.next;
            }

            return ret;
        }

        #region Messenger Events
        private void OnBeforeEnterRoom(RoomEventData data)
        {
            if (curRoom)
            {
                if (curRoom.roomIndex < data.room.roomIndex)
                {
                    GameContext.s_audioMgr.PlayOneShotSound(_enterRoomClip, transform.position, 1);
                }
                else
                {
                    GameContext.s_audioMgr.PlayOneShotSound(_leaveRoomClip, transform.position, 1);
                }
            }
        }

        private void OnEnterRoom(RoomEventData data)
        {
            curRoom = data.room;

            if (!GameContext.s_player)
                GameContext.s_player = Instantiate(playerPrefab, curRoom.playerSpawnPos, Quaternion.identity);
            else
                GameContext.s_player.transform.position = curRoom.playerSpawnPos;
            
            _enterRoomEvents?.Invoke();
        }
        public void OnEndCutScene(TimelineAsset cutScene)
        {
            foreach (var eventlist in _cutSceneEvents)
            {
                if (ReferenceEquals(cutScene, eventlist.objectRef))
                {
                    eventlist.events?.Invoke();
                }
            }
        }

        public void DestroyActor(int actorId)
        {
            Room room = curRoom;

            while (room.roomIndex != 0)
            {
                room = room.prev;
            }

            while(room != null)
            {
                room.DestroyActor(actorId);
                room = room.next;
            }
        }
        public void DestroyActorRange(int actorId, int startRoomIdx, int endRoomIdx)
        {
            Debug.Assert(startRoomIdx <= endRoomIdx && startRoomIdx >= 0 && startRoomIdx <= GameConst.k_totalNumRooms-1);

            Room startRoom = curRoom;
            if(curRoom.roomIndex > startRoomIdx)
            {
                do
                {
                    startRoom = startRoom.prev;
                }
                while (startRoom.roomIndex != startRoomIdx);
            }
            else if(curRoom.roomIndex < startRoomIdx)
            {
                do
                {
                    startRoom = startRoom.next;
                }
                while (startRoom.roomIndex != startRoomIdx);
            }

            int numRooms = endRoomIdx - startRoomIdx + 1;
            for(int i=0; i < numRooms; i++)
            {
                startRoom.DestroyActor(actorId);
                startRoom = startRoom.next;
            }
        }
        #endregion

        void TestRoomTransition()
        {
            Room outermost = curRoom.prev.prev.prev.prev;

            IEnumerator _gotoRoomRoutine()
            {
                while(outermost)
                {
                    yield return new WaitForSecondsRealtime(2f);
                    outermost.GoToNext();
                    outermost = outermost.next;
                }
            }

            StartCoroutine(_gotoRoomRoutine());
        }
    }
}
