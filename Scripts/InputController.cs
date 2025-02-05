using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MatchTwo.Client.Gameplay
{
    public class InputController : MonoBehaviour
    {
        public bool EnabledControls => _isEnabled;
        
        #region Fields

        private bool _isInitialized, _isEnabled;
        private BoardManager _boardManager;
        private EventSystem _eventSystem;
        private PointerEventData _pointerEventData;
        private List<RaycastResult> _raycastResults;
        private BoardItem _selectedBoardItem;
        
        #endregion

        #region Public Methods
        public void Initialize(BoardManager boardManager)
        {
            _boardManager = boardManager;
            _raycastResults = new List<RaycastResult>();
            _eventSystem = EventSystem.current;
            _isInitialized = true;
        }
        
        public void EnableControls(bool enabled)
        {
            _isEnabled = enabled;
        }
        #endregion

        #region Unity Methods

        private void Update()
        {
            if(!_isInitialized)
                return;

            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {   _selectedBoardItem = GetBoardItemAtPosition(touch.position);
                    if (_selectedBoardItem != null)
                    {
                        _ = _boardManager.PerformMatch(_selectedBoardItem);
                    }
                }
            }
        }
        #endregion

        #region Private Methods
        
        private BoardItem GetBoardItemAtPosition(Vector2 position)
        {
            _pointerEventData = new PointerEventData(_eventSystem)
            {
                position = position
            };

            _raycastResults.Clear();
            _eventSystem.RaycastAll(_pointerEventData, _raycastResults);

            foreach (var result in _raycastResults)
            {
                var hitObject = result.gameObject;
                var boardItem = hitObject.GetComponent<BoardItem>();

                if (boardItem != null)
                {
                    return boardItem;
                }
            }

            return null;
        }
        #endregion
    }
}