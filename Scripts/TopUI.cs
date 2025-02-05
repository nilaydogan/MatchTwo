using System.Collections.Generic;
using MatchTwo.Client.Data;
using TMPro;
using UnityEngine;

namespace MatchTwo.Client.Gameplay
{
    public class TopUI : MonoBehaviour
    {
        #region Fields
        
        [SerializeField] private TextMeshProUGUI _movesLeftText;
        [SerializeField] private Transform _goalItemsHolder;

        private List<GoalItem> _goalItems;
        
        #endregion

        #region Public Methods
        
        public void Initialize(int movesLeft, Dictionary<BoardItemType,Target> targets)
        {
            _movesLeftText.text = movesLeft.ToString();
            _goalItems = new List<GoalItem>();

            for (var i = 0; i < _goalItemsHolder.childCount; i++)
            {
                var goalItem = _goalItemsHolder.GetChild(i).GetComponent<GoalItem>();
                if (targets.ContainsKey(goalItem.Type))
                {
                    goalItem.Initialize(targets[goalItem.Type].amount);
                    _goalItems.Add(goalItem);
                }
                else
                    goalItem.gameObject.SetActive(false);
            }
        }
        
        public void UpdateTopUI(int movesLeft, Dictionary<BoardItemType,Target> targets)
        {
            _movesLeftText.text = movesLeft.ToString();
            foreach (var goalItem in _goalItems)
            {
                if (targets.ContainsKey(goalItem.Type))
                {
                    goalItem.UpdateCount(targets[goalItem.Type].amount);
                }
            }
        }
        
        public Transform GetGoalItemTransform(BoardItemType type)
        {
            foreach (var goalItem in _goalItems)
            {
                if (goalItem.Type == type)
                    return goalItem.transform;
            }

            return null;
        }
        #endregion
    }
}