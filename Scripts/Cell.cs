using System.Threading.Tasks;
using UnityEngine;

namespace MatchTwo.Client.Gameplay
{
    public class Cell : MonoBehaviour
    {
        public Vector2Int Coordinates { get; private set; }
        public BoardItem BoardItem { get; private set; }
        
        public void SetCoordinates(int x, int y)
        {
            Coordinates = new Vector2Int(x, y);
            gameObject.name = $"[{x}, {y}]";
        }
        
        public void Initialize()
        {
            //
        }
        
        public void PlaceBoardItem(string boardItemStringRepresentation)
        {
            var prefabToSpawn = GameBoard.GetBoardItemPrefab(boardItemStringRepresentation);
            BoardItem boardItem = Instantiate(prefabToSpawn.BoardItemLayerPrefab, transform);
            boardItem.Initialize(boardItemStringRepresentation);
            _ = PlaceBoardItem(boardItem);
        }
        
        public async Task PlaceBoardItem(BoardItem boardItem)
        {
            boardItem.AddParentCell(this);
            boardItem.transform.SetParent(transform);
            BoardItem = boardItem;
            
            await boardItem.OnBoardItemPlaced();
        }

        public void RemoveBoardItem()
        {
            BoardItem = null;
        }
    }
}