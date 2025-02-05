using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MatchTwo.Client.Gameplay
{
    public class BalloonBoardItem : BoardItem
    {
        public override async Task CollectBoardItem(Transform targetGoalTransform, Action<BoardItem> onCollected, Action<BoardItem> playCollectSfx)
        {
            if (_isCollecting) return;
            
            _isCollecting = true;
            
            await MoveBalloonToTarget(targetGoalTransform, onCollected, playCollectSfx);
        }

        private async Task MoveBalloonToTarget(Transform targetGoalTransform, Action<BoardItem> onCollected, Action<BoardItem> playCollectSfx)
        {
            onCollected?.Invoke(this);
            
            Vector3[] path = new Vector3[] { };
            try
            {
                playCollectSfx?.Invoke(this);
                
                if (targetGoalTransform == null)
                {
                    onCollected?.Invoke(this);
                    Destroy(gameObject);
                    return;
                }
                
                _canvas.enabled = true;
                _canvas.overrideSorting = true;
                
                var endPos = targetGoalTransform.position;
                var position = transform.position;
                endPos.z = position.z;
                //endPos.y -= ;
            
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
                        //onCollected?.Invoke(this);
                        if (gameObject != null)
                        {
                            transform.DOKill();
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
    }
}