using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PuzzleGame
{
    [RequireComponent(typeof(Collider2D))]
    public class IgnoreCollision : MonoBehaviour
    {
        [SerializeField] Collider2D[] _targets;

        // Start is called before the first frame update
        void Awake()
        {
            Collider2D myCollider = GetComponent<EdgeCollider2D>();

            foreach (var collider in _targets)
            {
                Physics2D.IgnoreCollision(myCollider, collider, true);
            }
        }
    }
}
