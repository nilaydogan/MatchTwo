using System;
using System.Threading.Tasks;
using DG.Tweening;
using MatchTwo.Client.Data;
using UnityEngine;

namespace MatchTwo.Client.Gameplay
{
    public class RegularBoardItem : BoardItem
    {
        #region Fields
        [SerializeField] private ParticleSystem _breakingParticles;
        #endregion

        #region Public Methods
        public override void Initialize(string data)
        {
            base.Initialize(data);
            
            if (Enum.TryParse(data, ignoreCase: true, out BoardItemType boardItemType) &&
                BoardItemVisualsData.Instance.BoardItemVisualDataDict.TryGetValue(boardItemType, out var itemData))
            {
                _boardItemImage.sprite = itemData.BoardItemImage;
                Type = itemData.Type;
            }
            else
            {
                throw new ArgumentException($"Could not parse {data} to BoardItemType");
            }
        }

        public override async Task CollectBoardItem(Transform targetGoalTransform, Action<BoardItem> onCollected, Action<BoardItem> playCollectSfx)
        {
            if (_isCollecting) return;
            
            _isCollecting = true;
            
            onCollected?.Invoke(this);
            
            Vector3[] path = new Vector3[] { };
            try
            {
                var particles = Instantiate(_breakingParticles, ParentCell.transform);
                particles.transform.SetAsLastSibling();
                var main = particles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(GetColor(Type));
                particles.Play();
                playCollectSfx?.Invoke(this);

                if (targetGoalTransform == null)
                {
                    Destroy(gameObject);
                    return;
                }
                
                _canvas.enabled = true;
                _canvas.overrideSorting = true;
                
                var endPos = targetGoalTransform.position;
                var position = transform.position;
                endPos.z = position.z;
            
                path = new Vector3[]
                {
                    position,        // Start point
                    new Vector3((endPos.x + position.x) / 2f,
                        position.y - transform.localScale.y,
                        endPos.z), // Control point
                    endPos           // End point
                };
                
                Sequence seq = DOTween.Sequence();

                seq.Join(transform.DOPath(path, 0.8f, PathType.CatmullRom).SetEase(Ease.InOutCubic))
                    .Join(transform.DOScale(.7f, .8f).SetEase(Ease.InOutCubic))
                    .AppendCallback(() =>
                    {
                        if (gameObject != null)
                        {
                            transform.DOKill();
                            Destroy(particles, 1f);
                            Destroy(gameObject);
                        }
                    });

                await seq.AsyncWaitForCompletion();
            }
            catch (Exception e)
            {
                Debug.LogError($"Error during collection boarditem: {e.Message}");
            }
            finally
            {
                _isCollecting = false;
            }
        }

        public void Initialize(BoardItemType type)
        {
            if (BoardItemVisualsData.Instance.BoardItemVisualDataDict.TryGetValue(type, out var itemData))
            {
                _boardItemImage.sprite = itemData.BoardItemImage;
                Type = itemData.Type;
            }
        }
        #endregion

        #region Private Methods
        private Color GetColor(BoardItemType itemType)
        {
            switch (itemType)
            {
                case BoardItemType.Blue:
                    return new Color(0.1568628f, 0.5450981f, 0.8705883f);
                case BoardItemType.Green:
                    return Color.green;
                case BoardItemType.Red:
                    return Color.red;
                case BoardItemType.Yellow:
                    return Color.yellow;
                case BoardItemType.Purple:
                    return new Color(0.7098039f, 0.06666667f, 0.7411765f);
            }

            return default;
        }
        #endregion
    }
}