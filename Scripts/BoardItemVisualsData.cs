using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchTwo.Client.Data
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BoardItemVisualData", fileName = "BoardItemVisualsData")]
    public class BoardItemVisualsData : ScriptableObject
    {
        #region Singleton

        private static BoardItemVisualsData _instance;

        public static BoardItemVisualsData Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load("BoardItemVisualsData") as BoardItemVisualsData;
                    if (_instance == null)
                    {
                        Debug.LogError("BoardItemVisualData could not be loaded!");
                    }
                    else
                    {
                        _instance.Initialize();
                    }
                }

                return _instance;
            }
        }

        #endregion
        
        public Dictionary<BoardItemType, BoardItemVisualData> BoardItemVisualDataDict { get; private set; }
        [SerializeField] private List<BoardItemVisualData> _boardItemVisualsData;

        private void Initialize()
        {
            BoardItemVisualDataDict = new Dictionary<BoardItemType, BoardItemVisualData>();
            foreach (var data in _boardItemVisualsData)
            {
                BoardItemVisualDataDict.Add(data.Type, data);
            }
        }
    }

    [Serializable]
    public struct BoardItemVisualData
    {
        public BoardItemType Type;
        public BoardItemDefinition Definition;
        public Sprite BoardItemImage;

        // public static bool operator ==(BoardItemVisualData p1, BoardItemVisualData p2)
        // {
        //     return p1.Type == p2.Type;
        // }
        //
        // public static bool operator !=(BoardItemVisualData p1, BoardItemVisualData p2)
        // {
        //     return !(p1 == p2);
        // }
        // public override bool Equals(object obj)
        // {
        //     return obj is BoardItemVisualData other && Equals(other);
        // }
        //
        // public override int GetHashCode()
        // {
        //     return HashCode.Combine((int)Type);
        // }
    }

    public enum BoardItemType
    {
        Blue = 0,
        Green = 1,
        Red = 2,
        Yellow = 3,
        Purple = 4,
        Duck = 5,
        Balloon = 6,
        Rocket = 7
    }
    
    public enum BoardItemDefinition
    {
        Regular = 0,
        Duck = 1,
        Balloon = 2
    }
}