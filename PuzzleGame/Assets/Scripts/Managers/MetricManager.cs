using System.Reflection;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PuzzleGame.EventSystem;
using System;
using System.IO;

using PuzzleGame.UI;

namespace PuzzleGame
{
    public class MetricManager : MonoBehaviour
    {
        class PuzzleMetrics
        {
            public PuzzleMetrics(string puzzleName, float timeSpent, bool solved)
            {
                this.puzzleName = puzzleName;
                this.timeSpent = timeSpent;
                this.solved = solved;
            }
            public string puzzleName;
            public float timeSpent;
            public bool solved;

            public override string ToString()
            {
                return $"puzzleName={puzzleName}, total time spent={timeSpent}, solved={solved}";
            }
        }

        class PaintingMetrics
        {
            public int outOfPaintingCnt = 0;
            public int intoPaintingCnt = 0;
            public int rotateCnt = 0;

            public override string ToString()
            {
                return $"outOfPaintingCnt={outOfPaintingCnt}, intoPaintingCnt={intoPaintingCnt}, rotateCnt={rotateCnt}";
            }
        }

        class GameMetrics
        {
            public bool escaped = false;
            public bool rageQuit = false;
            public bool committedSuicide = false;
            public float gameTime = 0;

            public override string ToString()
            {
                if(escaped)
                {
                    return $"successfully escaped, total time={gameTime}";
                }
                
                if(rageQuit)
                {
                    return $"rage quited, total time={gameTime}";
                }

                if(committedSuicide)
                {
                    return $"committed suicide, total time={gameTime}";
                }

                return "undefined";
            }
        }

        private Dictionary<string, PuzzleMetrics> _puzzleMetricDict = new Dictionary<string, PuzzleMetrics>();

        PaintingMetrics _paintingMetrics = new PaintingMetrics();
        GameMetrics _gameMetrics = new GameMetrics();

        PuzzleMetrics _activePuzzle;
        int _prevRoom = -1;

        Dictionary<int, InspectionCanvas>.ValueCollection _allPuzzles = null;

        private void Awake()
        {
            Messenger.AddListener<InspectionEventData>(M_EventType.ON_INSPECTION_START, OnInspectionStart);
            Messenger.AddListener<InspectionEventData>(M_EventType.ON_INSPECTION_END, OnInspectionEnd);
            Messenger.AddListener<GameEndEventData>(M_EventType.ON_GAME_END, OnGameEnd);
            Messenger.AddListener<RoomEventData>(M_EventType.ON_ENTER_ROOM, OnEnterRoom);
            Messenger.AddListener(M_EventType.ON_GAME_START, OnGameStart);

            _activePuzzle = null;
        }

        private void Start()
        {

        }

        private void OnGameStart()
        {
            var fieldInfo = typeof(Inspectable).GetField("s_InspectionCanvasDict", BindingFlags.Static | BindingFlags.NonPublic);
            var dict = (Dictionary<int, InspectionCanvas>)fieldInfo.GetValue(null);
            _allPuzzles = dict.Values;

            Actor[] rotateCWInteractables = GameContext.s_gameMgr.GetAllActorsByName("RotatePaintingInteractableCW");
            Actor[] rotateCCWInteractables = GameContext.s_gameMgr.GetAllActorsByName("RotatePaintingInteractableCCW");
            foreach(var actor in rotateCWInteractables)
            {
                var interactable = actor as Interactable;
                interactable.interactionEvent.AddPersistentCall((Action)LogPaintingRotation);
            }
            foreach (var actor in rotateCCWInteractables)
            {
                var interactable = actor as Interactable;
                interactable.interactionEvent.AddPersistentCall((Action)LogPaintingRotation);
            }
        }

        private void LogPaintingRotation()
        {
            _paintingMetrics.rotateCnt++;
        }

        private void OnEnterRoom(RoomEventData data)
        {
            if(_prevRoom == -1)
            {
                _prevRoom = data.room.roomIndex;
            }
            else
            {
                if(_prevRoom < data.room.roomIndex)
                {
                    _paintingMetrics.intoPaintingCnt++;
                }
                else
                {
                    _paintingMetrics.outOfPaintingCnt++;
                }
            }
        }

        private void OnGameEnd(GameEndEventData data)
        {
            switch(data.type)
            {
                case EGameEndingType.DEATH:
                    _gameMetrics.committedSuicide = true;
                    _gameMetrics.escaped = false;
                    break;
                case EGameEndingType.ESCAPE:
                    _gameMetrics.committedSuicide = false;
                    _gameMetrics.escaped = true;
                    break;
            }
        }
        private void OnInspectionStart(InspectionEventData data)
        {
            foreach(var canvas in _allPuzzles)
            {
                if(canvas.gameObject.activeSelf)
                {
                    string name = canvas.name;
                    if(!_puzzleMetricDict.ContainsKey(name))
                    {
                        _puzzleMetricDict.Add(name, new PuzzleMetrics(name, 0, false));
                    }

                    _activePuzzle = _puzzleMetricDict[name];

                    break;
                }
            }
        }
        private void OnInspectionEnd(InspectionEventData data)
        {
            if (!data.inspectable.canInspect)
            {
                _activePuzzle.solved = true;
            }

            _activePuzzle = null;
        }
        // Converts all metrics tracked in this script to their string representation
        // so they look correct when printing to a file.
        private string ConvertMetricsToStringRepresentation()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Game Metrics\n");
            builder.Append(_gameMetrics.ToString());

            builder.Append("\n");
            builder.Append("\n");

            builder.Append("Puzzle Metrics\n");
            foreach(var pair in _puzzleMetricDict)
            {
                builder.Append(pair.Value.ToString());
                builder.Append("\n");
            }

            builder.Append("\n");
            builder.Append("Painting Metrics\n");
            builder.Append(_paintingMetrics.ToString());

            return builder.ToString();
        }

        // Uses the current date/time on this computer to create a uniquely named file,
        // preventing files from colliding and overwriting data.
        private string CreateUniqueFileName()
        {
            string dateTime = System.DateTime.Now.ToString();
            dateTime = dateTime.Replace("/", "_");
            dateTime = dateTime.Replace(":", "_");
            dateTime = dateTime.Replace(" ", "___");
            return "No_Escape_Metrics" + dateTime + ".txt";
        }

        // Generate the report that will be saved out to a file.
        private void WriteMetricsToFile()
        {
#if !UNITY_EDITOR
            string totalReport = "Report generated on " + System.DateTime.Now + "\n\n";
            totalReport += "Total Report:\n";
            totalReport += ConvertMetricsToStringRepresentation();
            totalReport = totalReport.Replace("\n", System.Environment.NewLine);
            string reportFile = CreateUniqueFileName();

#if !UNITY_WEBPLAYER
            File.WriteAllText(reportFile, totalReport);
#endif
#endif
        }

        // The OnApplicationQuit function is a Unity-Specific function that gets
        // called right before your application actually exits. You can use this
        // to save information for the next time the game starts, or in our case
        // write the metrics out to a file.
        private void OnApplicationQuit()
        {
            if(!_gameMetrics.escaped && !_gameMetrics.committedSuicide)
            {
                _gameMetrics.rageQuit = true;
            }

            WriteMetricsToFile();
        }

        private void Update()
        {
            if(_activePuzzle != null)
            {
                _activePuzzle.timeSpent += Time.deltaTime;
            }

            _gameMetrics.gameTime += Time.deltaTime;
        }
    }
}
