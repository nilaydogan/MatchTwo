using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MatchTwo.Client.Data;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace MatchTwo.Client.Gameplay
{
    public class GameBoard : MonoBehaviour
    {
        #region Fields
        
        public Vector2Int BoardSize { get; private set; }
        public Cell[,] Cells { get; private set; }

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private GameObject _cellPrefab;
        [SerializeField] private Vector2 _cellSize;
        [SerializeField] private GridLayoutGroup _gridLayout;
        [SerializeField] private RectTransform _backgroundImageRectTransform;
        [SerializeField] private BoardItemPrefab[] _boardItemsPrefabs;
        [SerializeField] private Transform _spawner;
        [SerializeField] private RocketBoardItem _rocketBoardItemPrefab;

        private static Dictionary<BoardItemDefinition, BoardItemPrefab> _boardItemPrefabDict;

        #endregion

        #region Unity Methods

        private void CreateBoard()
        {
            _gridLayout.cellSize = new Vector2(_cellSize.x, _cellSize.y);
            
            _gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            
            var pos = _rectTransform.position;
            _rectTransform.anchorMin = Vector2.one * 0.5f;
            _rectTransform.anchorMax = Vector2.one * 0.5f;
            _rectTransform.position = pos;
            
            _boardItemPrefabDict = new Dictionary<BoardItemDefinition, BoardItemPrefab>();
            
            for (var i = 0; i < _boardItemsPrefabs.Length; i++)
            {
                _boardItemPrefabDict.Add(_boardItemsPrefabs[i].Definition, _boardItemsPrefabs[i]);
            }
        }

        #endregion

        #region Public Methods
        
        public void Initialize(string[,] board)
        {
            CreateBoard();
            BoardSize = new Vector2Int(board.GetLength(0), board.GetLength(1));
            SetBoardSize();
            Cells = new Cell[BoardSize.x, BoardSize.y];
            for (int y = 0; y < BoardSize.y; y++)
            {
                for (int x = 0; x < BoardSize.x; x++)
                {
                    Cells[x, y] = Instantiate(_cellPrefab, _gridLayout.transform).GetComponent<Cell>();
                    Cells[x, y].SetCoordinates(x, y);
                    Cells[x, y].Initialize();
                }
            }

            HandlePlacingBoardItems(board);
        }

        public bool IsCoordsInsideTheBoard(Vector2Int coords)
        {
            if (coords.x < Cells.GetLength(0) && coords.x >= 0)
            {
                if (coords.y < Cells.GetLength(1) && coords.y >= 0)
                {
                    return true;
                }
            }

            return false;
        }
        
        public static BoardItemPrefab GetBoardItemPrefab(string boardItemStringRepresentation)
        {
             BoardItemDefinition type;
             if (!Enum.TryParse(boardItemStringRepresentation, true, out type))
             {
                 var data = boardItemStringRepresentation.Split('_');
                 if (data.Length > 1) // obstacle
                 {
                     if (Enum.TryParse(data[1], ignoreCase: true, out BoardItemDefinition definition) &&
                         _boardItemPrefabDict.TryGetValue(definition, out var itemData))
                     {
                            return _boardItemPrefabDict[itemData.Definition];
                     }
                     else
                     {
                         throw new ArgumentException($"Could not parse {data} to BoardItemType");
                     }
                 }
             }
            
             return _boardItemPrefabDict[BoardItemDefinition.Regular];
        }
        
        public List<Cell> GetCellsBelowWorldPosition(int maxYIndex, int x)
        {
            List<Cell> cellsBelow = new List<Cell>();

            for (var i = 0; i < maxYIndex; i++)
            {
                var cell = Cells[x, i];
                cellsBelow.Add(cell);
            }

            return cellsBelow;
        }

        public async Task FillEmptyCells()
        {
            List<Task> tasks = new List<Task>();
            for (var y = 0; y < BoardSize.y; y++)
            {
                tasks.Clear();
                for (var x = 0; x < BoardSize.y; x++)
                {
                    if (Cells[x, y].BoardItem == null)
                    {
                        var prefab = _boardItemPrefabDict[BoardItemDefinition.Regular];
                        var boardItem = Instantiate(prefab.BoardItemLayerPrefab, Cells[x, y].transform).GetComponent<RegularBoardItem>();
                        var startPos = boardItem.transform.position;
                        startPos.y = _spawner.transform.position.y;
                        boardItem.transform.position = startPos;
                        
                        boardItem.Initialize(GetRandomBoardItemType());
                        tasks.Add(Cells[x, y].PlaceBoardItem(boardItem));
                    }
                }
                
            }
            await Task.WhenAll(tasks);
            
            BoardItemType GetRandomBoardItemType()
            {
                // only include regular items
                var enumCount = Enum.GetValues(typeof(BoardItemType)).Length - 1;

                var randomIndex = Random.Range(0, enumCount);
                
                return (BoardItemType)randomIndex;
            }
        }
        
        public virtual async Task ResetBoard(string[,] board)
        {
            await Task.Yield();
            for (int y = 0; y < BoardSize.y; y++)
            {
                for (int x = 0; x < BoardSize.x; x++)
                {
                    var boardItem = Cells[x, y].BoardItem;
                    Cells[x, y].RemoveBoardItem();
                    Destroy(boardItem.gameObject);
                }
            }

            await Task.Yield();
            HandlePlacingBoardItems(board);
        }

        #endregion

        #region Private Methods
        
        private void SetBoardSize()
        {
            _gridLayout.constraintCount = BoardSize.x;

            _rectTransform.sizeDelta = new Vector2(_cellSize.x * BoardSize.y + 30, (_cellSize.y - 25) * BoardSize.x + 40);
        }

        private void HandlePlacingBoardItems(string[,] board)
        {
            for (var y = 0; y < BoardSize.y; y++)
            {
                for (var x = 0; x < BoardSize.x; x++)
                {
                    Cells[x, y].PlaceBoardItem(board[x, y]);
                }
            }
        }

        #endregion
        
        [Serializable]
        public struct BoardItemPrefab
        {
            //public BoardItemType Type;
            public BoardItemDefinition Definition;
            public BoardItem BoardItemLayerPrefab;
        }
    }
}