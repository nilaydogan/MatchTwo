using MatchTwo.Client.Data;
using TMPro;
using UnityEngine;

namespace MatchTwo.Client
{
    public class GoalItem : MonoBehaviour
    {
        public BoardItemType Type => _type;
        [SerializeField] private TextMeshProUGUI _goalCountText;
        [SerializeField] private BoardItemType _type;
        [SerializeField] private ParticleSystem _hitEffect;
        
        private int _count;

        public void Initialize(int count)
        {
            gameObject.SetActive(true);
            UpdateCount(count);
        }
        
        public void UpdateCount(int count)
        {
            //todo: check for completion
            if (_count != count)
            {
                _hitEffect.Play();
            }
            _count = count;
            _goalCountText.text = _count.ToString();
        }
    }
}