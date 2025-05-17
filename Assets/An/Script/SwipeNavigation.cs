using UnityEngine;

namespace An_An
{
    public class NewBehaviourScript : MonoBehaviour
    {
        public BottomNavigationBar navBar;
        private Vector2 endTouchPosition;

        private Vector2 startTouchPosition;

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    startTouchPosition = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    endTouchPosition = touch.position;
                    HandleSwipe();
                }
            }
        }

        private void HandleSwipe()
        {
            var deltaX = endTouchPosition.x - startTouchPosition.x;

            if (Mathf.Abs(deltaX) > 100f) // Vuot du xa
            {
                if (deltaX > 0)
                    navBar.SwipeToPrevious();
                else
                    navBar.SwipeToNext();
            }
        }
    }
}