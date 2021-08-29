using PuzzleGame.EventSystem;
using System;
using System.Collections.Generic;
using UltEvents;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Tilemaps;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace PuzzleGame
{
    public class Room : MonoBehaviour
    {
        //how many sorting ids available for each layer locally in the room
        //global sorting id = local sorting id + room index * 10
        //Note: we use 1-based indexing here, min=1, max=k_maxLocalSortingOrder
        const int k_maxLocalSortingOrder = 9;

        Actor[] _actors = null;

        [SerializeField] GameObject _paintingMask = null;
        [SerializeField] Transform _paintingTransform = null;
        [SerializeField] Transform _paintingRotationAnchor = null;
        [SerializeField] Transform _roomMin, _roomMax; //min and max of room OBB
        [SerializeField] CutSceneManager _roomCutsceneMgr = null;

        [SerializeField] Transform _contentRoot = null;
        [SerializeField] Tilemap _roomTile = null;
        [SerializeField] Tilemap _paintingTile = null;

        //relative to the room, top-left (0,0)
        //unscaled painting size
        [SerializeField] Rect _paintingAreaLocal;
        //unscaled visible size
        [SerializeField] Rect _visibleAreaLocal;
        //room camera settings
        [SerializeField] int _cameraViewDistLocal;
        [SerializeField] Transform _viewCenterPos;
        [SerializeField] Transform _playerSpawnPos;
        [SerializeField] float _paintingRotationStep;
        [SerializeField] float _maxPaintingRotation;

        float _paintingRotationAngle = 0;

        //conversion methods
        public Vector2 roomPointToWorldPoint(Vector2 roomSpacePos)
        {
            return _contentRoot.TransformPoint(roomSpacePos);
        }
        public Vector2 roomDirToWorldDir(Vector2 dir)
        {
            return _contentRoot.TransformVector(dir);
        }
        public float roomUnitToWorldUnit(float length)
        {
            return length * _contentRoot.lossyScale.x;
        }

        public float paintingRotationStep { get { return _paintingRotationStep; } }
        public float paintingToVisibleAreaScale { get { return _paintingAreaLocal.width / _visibleAreaLocal.width; } }
        public float roomScale { get { return _contentRoot.lossyScale.x; } }
        public float cameraViewDist { get { return roomUnitToWorldUnit(_cameraViewDistLocal); } }
        public Vector2 playerSpawnPos { get { return _playerSpawnPos.position; } }
        public Vector2 viewCenterPos { get { return _viewCenterPos.position; } }
        public Vector2 roomSize { get { return _roomMax.position - _roomMin.position; } }
        //Note: the rect's size is signed
        [Obsolete("will probably switch to using transform")]
        public Rect paintingArea { get { return new Rect(roomPointToWorldPoint(_paintingAreaLocal.position), roomDirToWorldDir(_paintingAreaLocal.size)); } }
        [Obsolete("will probably switch to using transform")]
        public Rect visibleArea { get { return new Rect(roomPointToWorldPoint(_visibleAreaLocal.position), roomDirToWorldDir(_visibleAreaLocal.size)); } }
        public Bounds roomAABB
        {
            get
            {
                Vector2 diagonal1 = _roomMax.position - _roomMin.position;
                Vector2 topLeft = roomPointToWorldPoint(new Vector2(_roomMin.localPosition.x, _roomMax.localPosition.y));
                Vector2 bottomRight = roomPointToWorldPoint(new Vector2(_roomMax.localPosition.x, _roomMin.localPosition.y));
                Vector2 diagonal2 = bottomRight - topLeft;

                float height = Mathf.Abs(Vector2.Dot(Vector2.down, diagonal1));
                height = Mathf.Max(height, Mathf.Abs(Vector2.Dot(Vector2.down, diagonal2)));
                float width = Mathf.Abs(Vector2.Dot(Vector2.right, diagonal1));
                width = Mathf.Max(width, Mathf.Abs(Vector2.Dot(Vector2.right, diagonal2)));

                return new Bounds((Vector2)_roomMin.position + diagonal1 / 2, new Vector2(width, height));
            }
        }

        public Transform contentRoot { get { return _contentRoot; } }
        public Room next { get; private set; } = null;
        public Room prev { get; private set; } = null;

        public int roomIndex
        {
            get
            {
                return _roomIndex;
            }
            private set
            {
                _roomIndex = value;
                SetSpriteSortingOrder(value);
                ConfigSpriteMask(value);

                foreach(var actor in _actors)
                {
                    if (_roomIndex < actor.roomRange.x || _roomIndex > actor.roomRange.y)
                    {
                        actor.Reduce();
                    }
                }
            }
        }
        int _roomIndex;

        private void ConfigSpriteMask(int roomIdx)
        {
            SpriteMask[] masks = _paintingMask.GetComponentsInChildren<SpriteMask>();
            int[] sortingLayers = new int[]
            {
                GameConst.k_DefaultSortingLayerId,
                GameConst.k_PropsSortingLayerId
            };
            Debug.Assert(masks.Length == sortingLayers.Length);

            int start = roomIdx * (Room.k_maxLocalSortingOrder + 1);

            for (int i=0; i<sortingLayers.Length; i++)
            {
                SpriteMask mask = masks[i];
                mask.isCustomRangeActive = true;
                mask.frontSortingLayerID = sortingLayers[i];
                mask.backSortingLayerID = sortingLayers[i];

#if false
                mask.frontSortingOrder = roomIdx;
                mask.backSortingOrder = roomIdx - 1;
#else
                //interactables sorting order have different signs, see SetSpriteSortingOrder()
                if (sortingLayers[i] == GameConst.k_PropsSortingLayerId)
                {
                    mask.frontSortingOrder = -start;
                    mask.backSortingOrder = -(start + Room.k_maxLocalSortingOrder);
                }
                else
                {
                    mask.frontSortingOrder = start + Room.k_maxLocalSortingOrder;
                    mask.backSortingOrder = start;
                }
#endif            
            }
        }
        
        private void SetSpriteSortingOrder(int roomIdx)
        {
            SetSortingOrder(_roomTile.GetComponent<TilemapRenderer>(), roomIdx);
            SetSortingOrder(_paintingTile.GetComponent<TilemapRenderer>(), roomIdx);

            foreach (var actor in _actors)
            {
                if (!actor)
                    continue;

                if (actor.spriteRenderer)
                {
                    SetSortingOrder(actor.spriteRenderer, roomIdx);
                }
            }
        }

        private void SetSortingOrder(Renderer renderer, int roomIdx)
        {
            Debug.Assert(renderer.sortingOrder <= Room.k_maxLocalSortingOrder && renderer.sortingOrder >= 1, $"{renderer.name} sorting order out of range");

            //child's interactables should be rendered behind parent's
            //but as interactable layer > other layers (except character)
            //they will still appear on top of parent's tiles (i.e. walls/floors/painting frames, etc.)
            if (renderer.sortingLayerID == GameConst.k_PropsSortingLayerId)
            {
                renderer.sortingOrder = roomIdx * (Room.k_maxLocalSortingOrder + 1) + Room.k_maxLocalSortingOrder - renderer.sortingOrder;
                renderer.sortingOrder *= -1;
            }
            else
            {
                renderer.sortingOrder = roomIdx * (Room.k_maxLocalSortingOrder + 1) + renderer.sortingOrder;
            }
        }

        private void SetSpriteMaskInteraction(SpriteMaskInteraction interaction)
        {
            _roomTile.GetComponent<TilemapRenderer>().maskInteraction = interaction;
            _paintingTile.GetComponent<TilemapRenderer>().maskInteraction = interaction;

            foreach (var actor in _actors)
            {
                if (!actor)
                    continue;

                if(actor.spriteRenderer)
                    actor.spriteRenderer.maskInteraction = interaction;
            }
        }
        
        /// <summary>
        /// make the room contained in the parent's painting
        /// </summary>
        /// <param name="parent"></param>
        public void BecomePainting(Room parent)
        {
            transform.parent = parent._paintingTransform;
            transform.localScale = new Vector3(paintingToVisibleAreaScale, paintingToVisibleAreaScale, 1f);
            transform.localPosition = Vector2.zero;
            
            SetSpriteMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);
        }

        public void PlayCutScene(TimelineAsset timeline)
        {
            _roomCutsceneMgr.Play(timeline);
        }

        private void Awake()
        {
            _actors = GetComponentsInChildren<Actor>();

            int i = 0;
            foreach (var actor in _actors)
            {
                actor.room = this;
                actor.actorId = i;
                i++;
            }

            //Note: painting mask is for the display of current room in the previous room
            //so it's only activated when we spawn a child or a parent
            _paintingMask = Instantiate(_paintingMask, transform);
            _paintingMask.transform.parent = _contentRoot;

            SetSpriteMaskInteraction(SpriteMaskInteraction.None);

            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener<RoomEventData>(M_EventType.ON_BEFORE_ENTER_ROOM, OnBeforeEnterRoom);

            //make the room pivot at the visible area pivot
            _contentRoot.localPosition = -_visibleAreaLocal.position;

            //make the painting mask same size as the visible area, and above the room in z axis
            //because our visible area is the same as the painting area of our parent
            _paintingMask.transform.localScale = new Vector3(Mathf.Abs(_visibleAreaLocal.width), Mathf.Abs(_visibleAreaLocal.height), 1);
            _paintingMask.transform.localPosition = _visibleAreaLocal.position;
            Vector3 pos = _paintingMask.transform.position;
            pos.z = -1;
            _paintingMask.transform.position = pos;
        }

        public void UpdateRoom()
        {
            _roomCutsceneMgr.CheckCutScenes();
        }

        /// <summary>
        /// spawns a whole chain of rooms, returns the room with identity scale and sets it to be current room
        /// </summary>
        /// <param name="numLevels">number of levels</param>
        /// <returns>the root</returns>
        public static Room SpawnChain(int numLevels, int identityRoomLevel)
        {
            if (identityRoomLevel < 0 || identityRoomLevel >= numLevels)
                throw new ArgumentOutOfRangeException("identity room level must be between 0 and numLevels-1, inclusive");

            Room ret = null;

            Room cur = Instantiate(GameContext.s_gameMgr.roomPrefab, Vector3.zero, Quaternion.identity).GetComponent<Room>();
            cur.roomIndex = 0;
            float rootRoomScale = Mathf.Pow(1/cur.paintingToVisibleAreaScale, identityRoomLevel);
            cur.transform.localScale = new Vector3(rootRoomScale, rootRoomScale, 1);

            for(int level=0; level<numLevels-1; level++, cur = cur.next)
            {
                cur.SpawnNext();

                if (level == identityRoomLevel)
                {
                    ret = cur;
                }
            }
            ret.SetCurrent();

            return ret;
        }

        void SpawnNext()
        {
            if (next)
                throw new NotSupportedException("reconnection between rooms is not supported");

            GameObject clone = Instantiate(GameContext.s_gameMgr.roomPrefab.gameObject);
            next = clone.GetComponent<Room>();   
            next.BecomePainting(this);
            next.roomIndex = roomIndex + 1;
            next.prev = this;
        }

        public void RotateNext(bool clockwise)
        {
            if (!next)
                return;

            float prev = _paintingRotationAngle;
            if (!clockwise)
            {
                _paintingRotationAngle = Mathf.Min(_paintingRotationAngle + _paintingRotationStep, _maxPaintingRotation);
                RotateNextInternal(_paintingRotationAngle - prev);
            }
            else
            {
                _paintingRotationAngle = Mathf.Max(_paintingRotationAngle - _paintingRotationStep, -_maxPaintingRotation);
                RotateNextInternal(_paintingRotationAngle - prev);
            }
        }

        public void InvertNext()
        {
            if (!next)
                return;

            _paintingRotationAngle = 0;
            RotateNextInternal(180);
        }

        void RotateNextInternal(float angle)
        {
            //rotate painting frame
            //all children should be childed under the painting's transform
            _paintingTransform.RotateAround(_paintingRotationAnchor.position, Vector3.forward, angle);
        }

        private void SetRoomCollision(bool enable)
        {
            foreach (var actor in _actors)
            {
                if (!actor)
                    continue;

                if(actor.actorCollider)
                {
                    actor.actorCollider.enabled = enable;
                }

                if (actor.actorRigidBody)
                {
                    if (enable)
                        actor.actorRigidBody.WakeUp();
                    else
                        actor.actorRigidBody.Sleep();
                }
            }
        }

        private void SetActive(bool enable)
        {
            _paintingTile.GetComponent<TilemapRenderer>().enabled = enable;
            _roomTile.GetComponent<TilemapRenderer>().enabled = enable;
            foreach(var actor in _actors)
            {
                if (!actor)
                    continue;

                if(actor.spriteRenderer)
                {
                    actor.spriteRenderer.enabled = enable;
                }
            }
        }

        /// <summary>
        /// initializes a room as the current room
        /// note: for transition, use GoToNext or GoToPrev instead
        /// </summary>
        void SetCurrent()
        {
            Room ptr = prev;
            //hide parent rooms
            while(ptr)
            {
                ptr.SetRoomCollision(false);
                ptr.SetActive(false);
                ptr = ptr.prev;
            }
            //disable children room collisions
            ptr = next;
            while(ptr)
            {
                ptr.SetRoomCollision(false);
                ptr = ptr.next;
            }

            //disable masking of the current room
            SetSpriteMaskInteraction(SpriteMaskInteraction.None);

            //callbacks
            Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(this));
        }

        public void GoToNext()
        {
            if (!next)
                return;

            // we don't hide the current room here, i.e. this.SetActive(false) 
            // after we entered the next room
            // this is done in OnEnterRoom event handler

            Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(next));
        }

        public void GoToPrev()
        {
            if (!prev)
                return;

            SetSpriteMaskInteraction(SpriteMaskInteraction.VisibleInsideMask);
            prev.SetActive(true);

            Messenger.Broadcast(M_EventType.ON_BEFORE_ENTER_ROOM, new RoomEventData(prev));
        }

        public void DestroyActor(int actorId)
        {
            Actor actor = _actors[actorId];
            if(actor)
            {
                _actors[actorId] = null;
                actor.Destroy();
            }
        }

        public Actor GetActorByID(int actorId)
        {
            return _actors[actorId];
        }

        public Actor[] GetActorsByType<T>()
        {
            List<Actor> ret = new List<Actor>();
            for(int i=0; i<_actors.Length; i++)
            {
                if(_actors[i] && _actors[i] is T)
                {
                    ret.Add(_actors[i]);
                }
            }
            return ret.ToArray();
        }

        public Actor GetActorByName(string name)
        {
            for (int i = 0; i < _actors.Length; i++)
            {
                if (_actors[i] && _actors[i].name == name)
                {
                    return _actors[i];
                }
            }
            return null;
        }

        private void OnDrawGizmos()
        {
            if(GameContext.s_gameMgr && Object.ReferenceEquals(this, GameContext.s_gameMgr.curRoom))
            {

            }
        }

        #region GAME EVENTS
        private void OnBeforeEnterRoom(RoomEventData data)
        {

        }

        private void OnEnterRoom(RoomEventData data)
        {
            if (!Object.ReferenceEquals(data.room, this))
            {
                //is this the ancestor of the room that the player entered?
                if (roomIndex < data.room.roomIndex)
                {
                    SetRoomCollision(false);
                    SetActive(false);
                }
                //is this the successor?
                else
                {
                    SetRoomCollision(false);
                }
            }
            //is this the room that the player entered?
            else if(Object.ReferenceEquals(data.room, this))
            {
                //disable masking, enable collision
                SetRoomCollision(true);
                SetSpriteMaskInteraction(SpriteMaskInteraction.None);
                SetActive(true);
            }
        }
        #endregion
    }
}