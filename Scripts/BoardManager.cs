using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MatchTwo.Client.Data;
using UnityEngine;

namespace MatchTwo.Client.Gameplay
{
    public class BoardManager : MonoBehaviour
    {
        #region Fields

        private GameBoard _gameBoard;
        private GameManager _gameManager;
        
        #endregion

        #region Public Methods
        public void Initialize(GameManager gameManager, GameBoard gameBoard)
        {
            _gameManager = gameManager;
            _gameBoard = gameBoard;
        }
        
        public async Task PerformMatch(BoardItem selectedBoardItem)
        {
            if (selectedBoardItem?.ParentCell == null)
            {
                Debug.LogError("Selected Board Item or Parent Cell is null.");
                return;
            }

            if (selectedBoardItem.Definition != BoardItemDefinition.Regular)
                return;
            
            var boardItemsToMatch = new List<BoardItem>
            {
                selectedBoardItem
            };
            CheckBoardForMatches(selectedBoardItem,boardItemsToMatch);

            if (boardItemsToMatch.Count <= 1)
            {
                boardItemsToMatch.Clear();
                return;
            }

            await PerformCollection(boardItemsToMatch);
            
            await SpawnRandomRegularBlocks();
            await _gameManager.AfterMatch();
        }
        
        #endregion

        #region Private Methods
        private void CheckBoardForMatches(BoardItem boardItemToCheck, List<BoardItem> boardItemsToMatch)
        {
            if (boardItemToCheck == null || boardItemToCheck.ParentCell == null) return;
            
            var directionsToCheck = new List<Vector2Int>()
            {
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.right
            };
            
            var toCheck = new Stack<BoardItem>();
            toCheck.Push(boardItemToCheck);

            while (toCheck.Count > 0)
            {
                var currentItem = toCheck.Pop();
                var cell = currentItem.ParentCell.Coordinates;

                foreach (var direction in directionsToCheck)
                {
                    var coords = cell + direction;

                    if (!_gameBoard.IsCoordsInsideTheBoard(coords)) continue;

                    var boardItem = _gameBoard.Cells[coords.x, coords.y]?.BoardItem;

                    if (boardItem == null || boardItemsToMatch.Contains(boardItem)) continue;

                    if (boardItem.Type == currentItem.Type)
                    {
                        boardItemsToMatch.Add(boardItem);
                        toCheck.Push(boardItem);
                    }
                    else if (boardItem.Type == BoardItemType.Balloon)
                    {
                        boardItemsToMatch.Add(boardItem);
                        //toCheck.Push(boardItem);
                    }
                }
            }
        }

        private async Task PerformCollection(List<BoardItem> boardItemsToMatch)
        {
            var collectionTasks = new List<Task>();
            foreach (var boardItem in boardItemsToMatch)
            {
                boardItem.ParentCell.RemoveBoardItem();
                collectionTasks.Add(boardItem.CollectBoardItem(_gameManager.GetTargetTransform(boardItem.Type),_gameManager.OnBoardItemCollected, _gameManager.PlayBoardItemCollectedSound));
            }
            await Task.WhenAll(collectionTasks);
            
            await RepositionBoardItems();

            await CheckForDuckCollection();
        }

        private async Task RepositionBoardItems()
        {
            var cells = _gameBoard.Cells;

            var repositioningTasks = new List<Task>();
            var occupiedCells = new HashSet<Cell>();
            
            foreach (var cell in cells)
            {
                if (cell == null || cell.BoardItem == null)
                    continue;

                var cellsBelow = _gameBoard.GetCellsBelowWorldPosition(cell.Coordinates.y, cell.Coordinates.x);
                if (cellsBelow.Count == 0 || cellsBelow.All(c => c.BoardItem != null))
                    continue;

                Cell targetCell = null;
                for (var i = cellsBelow.Count - 1; i >= 0; i--)
                {
                    if (cellsBelow[i].BoardItem == null)
                    {
                        targetCell = cellsBelow[i];
                    }
                    else
                    {
                        break;
                    }
                }

                if (targetCell != null && !occupiedCells.Contains(targetCell))
                {
                    var boardItem = cell.BoardItem;
                    cell.RemoveBoardItem();
                    occupiedCells.Add(targetCell);
                    repositioningTasks.Add(RepositionBoardItem(targetCell, boardItem));
                }
            }

            await Task.WhenAll(repositioningTasks);
            
            async Task RepositionBoardItem(Cell targetCell, BoardItem boardItem)
            {
                await targetCell.PlaceBoardItem(boardItem);
            }
        }

        private async Task SpawnRandomRegularBlocks()
        {
            await _gameBoard.FillEmptyCells();
        }

        private async Task CheckForDuckCollection()
        {
            var cells = _gameBoard.Cells;
            var firstRow = new List<Cell>();
            for (var x = 0; x < _gameBoard.BoardSize.x; x++)
            {
                firstRow.Add(cells[x, 0]);
            }
            
            var ducks = firstRow.Where(c => c.BoardItem != null && c.BoardItem.Type == BoardItemType.Duck).ToList();
            if (ducks.Count == 0)
                return;
            await PerformCollection(ducks.Select(c => c.BoardItem).ToList());
        }
        
        #endregion
    }
}