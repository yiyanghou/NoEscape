using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    public class Actor : MonoBehaviour, IAnimationClipSource
    {
        [SerializeField] AnimationClip[] _animClips;

        [Tooltip("range in which the actor is functioning, i.e. having components other than sprite renderer")]
        [SerializeField] Vector2Int _roomRange = new Vector2Int(0, GameConst.k_totalNumRooms-1);

        public Room room { get; set; } = null;
        public int actorId { get; set; } = -1;

        public Vector2Int roomRange
        {
            get
            {
                return _roomRange;
            }
        }

        public SpriteRenderer spriteRenderer 
        {
            get
            {
                if (!_hasSpriteRend)
                    return null;

                if (!_spriteRenderer)
                    _spriteRenderer = GetComponent<SpriteRenderer>();
                if (!_spriteRenderer)
                    _hasSpriteRend = false; //next time we know we don't have this

                return _spriteRenderer;
            }
        }
        public Rigidbody2D actorRigidBody
        {
            get
            {
                if (!_hasRgBody)
                    return null;

                if (!_rigidBody)
                    _rigidBody = GetComponent<Rigidbody2D>();
                if (!_rigidBody)
                    _hasRgBody = false; //next time we know we don't have this

                return _rigidBody;
            }
        }
        public Collider2D actorCollider
        {
            get
            {
                if (!_hasCollider)
                    return null;

                if (!_collider)
                    _collider = GetComponent<Collider2D>();
                if (!_collider)
                    _hasCollider = false; //next time we know we don't have this

                return _collider;
            }
        }
        private SpriteRenderer _spriteRenderer = null;
        private Collider2D _collider = null;
        private Rigidbody2D _rigidBody = null;

        private bool _hasCollider = true, _hasRgBody = true, _hasSpriteRend = true;

        protected virtual void Awake()
        {
            //init work before game systems are initialized
           
            //these booleans are lazily updated
            _hasCollider = _hasRgBody = _hasSpriteRend = true;
        }

        protected virtual void Start()
        {
            //put common init work here
        }

        void IAnimationClipSource.GetAnimationClips(List<AnimationClip> results)
        {
            if(_animClips != null)
                results.AddRange(_animClips);
        }

        /// <summary>
        /// cleans the node the actor is on, but does not destroy the node itself
        /// </summary>
        public void Destroy()
        {
            Behaviour[] comps = GetComponents<Behaviour>();
            foreach (var comp in comps)
            {
                comp.enabled = false;
            }

            Renderer[] renderers = GetComponents<Renderer>();
            foreach (var rend in renderers)
            {
                rend.enabled = false;
            }
        }

        /// <summary>
        /// disable all the behaviors on this actor
        /// </summary>
        public void Reduce()
        {
            List<Behaviour> delList = new List<Behaviour>();

            ReduceInternal(transform, delList);
            _collider = null;
            _rigidBody = null;

            foreach (var comp in delList)
            {
                comp.enabled = false;
            }
        }

        // strip everything except sprite renderers and myself
        void ReduceInternal(Transform cur, List<Behaviour> delList)
        {
            Behaviour[] comps = cur.GetComponents<Behaviour>();
            foreach(var comp in comps)
            {
                if (!(comp is Actor))
                    delList.Add(comp);
            }

            for (int i = 0; i < cur.childCount; i++)
            {
                //if the child node is an actor, don't recurse, as it will be processed
                var actor = cur.GetChild(i).GetComponent<Actor>();
                if (actor)
                {
                    ReduceInternal(cur.GetChild(i), delList);
                }
            }
        }
    }
}