using UnityEngine;

namespace MatchTwo.Client.Utils
{
    //for the devices that have a notch or a cutout, this script will adjust the UI to fit the screen
    //but in the game scene our top ui image is not a good fit for these kind of devices
    public class SafeArea : MonoBehaviour
    {
        #region Fields
        
        private Canvas _canvas;
        private RectTransform _safeAreaRectTransform;
        private const float _multiplier = 0.66f;

        #endregion

        #region Unity Methods
        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();

            _safeAreaRectTransform = GetComponent<RectTransform>();

            ApplySafeArea();
        }
        #endregion

        #region Private Methods

        private void ApplySafeArea()
        {
            if (_safeAreaRectTransform == null)
                return;

            var safeArea = Screen.safeArea;

            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            var canvasRect = _canvas.pixelRect;
            anchorMin.x /= canvasRect.width;
            anchorMin.y /= canvasRect.height;
            anchorMax.x /= canvasRect.width;
            anchorMax.y /= canvasRect.height;

            anchorMin.y = UpdateMin(anchorMin.y);
            anchorMax.y = UpdateMax(anchorMax.y);

            _safeAreaRectTransform.anchorMin = anchorMin;
            _safeAreaRectTransform.anchorMax = anchorMax;
        }

        private float UpdateMin(float min)
        {
            return min * _multiplier;
        }

        private float UpdateMax(float max)
        {
            var dif = 1 - max;
            dif *= _multiplier;
            var result = 1 - dif;
            return result;
        }

        #endregion
    }
}