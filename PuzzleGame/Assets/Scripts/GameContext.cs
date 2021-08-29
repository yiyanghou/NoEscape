using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.UI;
using PuzzleGame.EventSystem;

namespace PuzzleGame
{
    /// <summary>
    /// this class is a common entry point to access global variables
    /// </summary>
    public static class GameContext
    {
        public static GameManager s_gameMgr;
        public static EffectManager s_effectMgr;
        public static UIManager s_UIMgr;
        public static AudioManager s_audioMgr;
        public static InventoryItem s_curDraggingItem;
        public static ELanguageType s_curLanguage;

        public static Player s_player;
        public static Vector2 s_right = Vector2.right, s_up = Vector2.up;

        public static void Flush()
        {
            s_gameMgr = null;
            s_effectMgr = null;
            s_UIMgr = null;
            s_audioMgr = null;
            s_curDraggingItem = null;
            s_player = null;

            Messenger.Cleanup();
        }
    }

    /// <summary>
    /// put everything constant in the project here (e.g. layer name, string, etc.)
    /// used to store absolute constants
    /// the constants that tend to be tweaked during edit-time is stored in GameConstConfig
    /// </summary>
    public static class GameConst
    {
        public const string k_outlineShaderPath = "Sprites/Outline";
        public const int k_playerInventorySize = 8;

        public const string k_interactableSpriteLayer = "Interactable";
        public const int k_pixelPerWorldUnit = 16;
        public const int k_startingRoomIndex = 2;
        public const int k_totalNumRooms = 7;
        public const int k_maxRoomIndex = 5;

        //layers are set in project and used for collision rules
        //rule: boundary collides with everything
        //      ground collides with props but does not collide with player or jumping player
        //      wall   collides with props and player but does not collide with jumping player
        //      props  collide with player and jumping player

        public const int k_defaultLayer = 0;
        public const int k_UILayer = 5;
        public const int k_playerLayer = 8;
        public const int k_playerJumpingLayer = 9; //not used for now
        public const int k_propLayer = 10;
        public const int k_wallLayer = 11;   //not used for now
        public const int k_groundLayer = 12; //not used for now
        public const int k_boundaryLayer = 13;

        //animator variables
        public static readonly int k_PlayerXSpeed_AnimParam = Animator.StringToHash("HorizontalSpeed");
        public static readonly int k_PlayerYSpeed_AnimParam = Animator.StringToHash("VerticalSpeed");
        public static readonly int k_PlayerWalking_AnimParam = Animator.StringToHash("Walking");
        public static readonly int k_PlayerAirborne_AnimParam = Animator.StringToHash("Airborne");
        public static readonly int k_PlayerDeath_AnimParam = Animator.StringToHash("Death");
        public static readonly int k_DefaultSortingLayerId = SortingLayer.NameToID("Default");
        public static readonly int k_PropsSortingLayerId = SortingLayer.NameToID("Props");
        public static readonly int k_CharacterSortingLayerId = SortingLayer.NameToID("Character");
    }
}