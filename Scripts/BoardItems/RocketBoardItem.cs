using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using MatchTwo.Client.Gameplay;
using UnityEngine;

namespace MatchTwo.Client
{
    public class RocketBoardItem : BoardItem
    {
        public Vector3 StartPosition { get; private set; }
        public Vector2 RocketDirection{ get; private set; }
        
        private float moveSpeed = 5f;
        private bool isMoving = false;

        // Initialize the Rocket with start position
        public void Initialize(Vector3 startPosition, Vector2 rocketDirection)
        {
            StartPosition = startPosition;
            RocketDirection = rocketDirection;
            transform.position = startPosition;
            isMoving = true;
        }
        public override Task CollectBoardItem(Transform targetGoalTransform, Action<BoardItem> onCollected, Action<BoardItem> playCollectSfx)
        {
            return Task.CompletedTask;
        }
        
        public void LaunchRocket(int direction, Action<List<Cell>> onRocketReachedTarget, float movementDistance)
        {
            Vector3 targetPosition = transform.position + new Vector3(RocketDirection.x, RocketDirection.y, 0) * movementDistance;
            
            transform.DOMove(targetPosition, 2f).OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }
    }
}