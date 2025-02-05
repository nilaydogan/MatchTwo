using System;
using System.Threading.Tasks;
using DG.Tweening;
using MatchTwo.Client.Data;
using UnityEngine;
using UnityEngine.UI;

namespace MatchTwo.Client.Gameplay
{
    public abstract class BoardItem : MonoBehaviour
    {
        public Cell ParentCell { get; private set; }
        public BoardItemType Type;
        public BoardItemDefinition Definition;

        [SerializeField] protected Image _boardItemImage;
        [SerializeField] protected RectTransform _rectTransform;
        [SerializeField] protected Canvas _canvas;
        protected bool _isCollecting = false;
        
        private static Action<BoardItem> _onAnyBoardItemCollected;
        
        public virtual void Initialize(string data)
        {
        }
        public virtual void AddParentCell(Cell boardCell)
        {
            ParentCell = boardCell;
        }

        public abstract Task CollectBoardItem(Transform targetGoalTransform, Action<BoardItem> onCollected, Action<BoardItem> playCollectSfx);
        
        public virtual async Task OnBoardItemPlaced()
        {
            var distanceSqrMag = ((Vector2)transform.position - (Vector2)ParentCell.transform.position).sqrMagnitude;
            var duration = Mathf.Clamp(distanceSqrMag, 3f, 8f) * .03f;
            var targetPosition = ParentCell.transform.position;
            targetPosition.z = transform.position.z;

            if (distanceSqrMag == 0) return;

            await transform.DOMove(targetPosition, duration).SetEase(Ease.InSine)
                .OnComplete(() =>
                {
                    _rectTransform.anchorMin = Vector2.zero;
                    _rectTransform.anchorMax = Vector2.one;
                    transform.localScale = Vector3.one;
                })
                .AsyncWaitForCompletion();
        }
    }
}