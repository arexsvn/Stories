using UnityEngine;
using System.Collections;

public class InputController
{
    private Vector2 _gestureStartPosition;
    private float _gestureStartTime;

    public RaycastHit2D checkInput()
    {
        RaycastHit2D hit = default(RaycastHit2D);
        Vector2 startPosition = getInputPosition(true);

        if (startPosition != Vector2.zero)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        }

        return hit;
    }

    public Vector2 checkGestures()
    {
        Vector2 startPosition = getInputPosition(true);
        if (startPosition != default(Vector2))
        {
            _gestureStartPosition = startPosition;
            _gestureStartTime = Time.time;
            return Vector2.zero;
        }

        Vector2 endPosition = getInputPosition(false);
        if (endPosition == default(Vector2) || _gestureStartPosition == default(Vector2))
        {
            return Vector2.zero;
        }

        Vector2 delta = endPosition - _gestureStartPosition;
        bool verticalSwipe = false;
        bool horizontalSwipe = false;
        const float MIN_SWIPE_DISTANCE = 300f;
        const float MIN_SWIPE_SPEED = 2000f;
        const float MAX_HORIZONTAL_ANGLE = 20f;
        const float MIN_VERTICAL_ANGLE = 80f;
        //const float MAX_VERTICAL_ANGLE = 100f;

        if (Mathf.Abs(delta.y) > MIN_SWIPE_DISTANCE || Mathf.Abs(delta.x) > MIN_SWIPE_DISTANCE)
        {
            float angle = Mathf.Atan(delta.y / delta.x) * (180.0f / Mathf.PI);
            float duration = Time.time - _gestureStartTime;

            //Debug.Log ("angle : " + angle);

            if (angle < 0f)
                angle = angle * -1.0f;

            if (Mathf.Abs(delta.y) > MIN_SWIPE_DISTANCE && (angle > MIN_VERTICAL_ANGLE/* && angle < MAX_VERTICAL_ANGLE*/))
            {
                verticalSwipe = true;
            }
            else if (Mathf.Abs(delta.x) > MIN_SWIPE_DISTANCE && angle < MAX_HORIZONTAL_ANGLE)
            {
                horizontalSwipe = true;
            }

            if (horizontalSwipe || verticalSwipe)
            {
                float distance = Mathf.Sqrt(Mathf.Pow(delta.x, 2f) + Mathf.Pow(delta.y, 2f));
                float speed = distance / duration;

                if (speed > MIN_SWIPE_SPEED)
                {
                    if (verticalSwipe)
                    {
                        if (delta.y > 0)
                        {
                            //Debug.Log("Vertical Swipe Up : distance : " + dist + " speed : " + speed + " angle : " + angle);
                            return new Vector2(0, 1);
                        }
                        else
                        {
                            //Debug.Log("Vertical Swipe Down  : distance : " + dist + " speed : " + speed + " angle : " + angle);
                            return new Vector2(0, -1);
                        }
                    }
                    else if (horizontalSwipe)
                    {
                        if (delta.x > 0)
                        {
                            //Debug.Log("Horizontal Swipe Right : distance : " + dist + " speed : " + speed + " angle : " + angle);
                            return new Vector2(1, 0);
                        }
                        else
                        {
                            //Debug.Log("Horizontal Swipe Left : distance : " + dist + " speed : " + speed + " angle : " + angle);
                            return new Vector2(-1, 0);
                        }
                    }
                }
                else
                {
                    Debug.Log("TOO SLOW : distance : " + distance + " speed : " + speed + " angle : " + angle);
                }
            }
        }

        return Vector2.zero;
    }

    private Vector2 getInputPosition(bool start)
    {
        Vector2 position = Vector2.zero;

        if (Application.isMobilePlatform)
        {
            if (Input.touchCount > 0)
            {
                Touch currentTouch = Input.GetTouch(0);

                if ((currentTouch.phase == TouchPhase.Began && start) || (currentTouch.phase == TouchPhase.Ended && !start))
                {
                    position = currentTouch.position;
                }
            }
        }
        else
        {
            if ((Input.GetMouseButtonDown(0) && start) || (Input.GetMouseButtonUp(0) && !start))
            {
                position = Input.mousePosition;
            }
        }

        return position;
    }
}
