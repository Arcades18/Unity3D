using UnityEngine;

public class ImageZoomPan : MonoBehaviour
{
    public float zoomSpeed = 0.1f;
    public float minZoom = 1f;
    public float maxZoom = 3f;
    public float panSpeed = 1f;

    private RectTransform rect;
    private Vector2 lastPanPosition;
    private bool isPanning;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    void HandleMouse()
    {
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0)
            Zoom(scroll * zoomSpeed);

        if (Input.GetMouseButtonDown(1))
        {
            lastPanPosition = Input.mousePosition;
            isPanning = true;
        }
        if (Input.GetMouseButtonUp(1))
            isPanning = false;

        if (isPanning)
        {
            Vector2 delta = (Vector2)Input.mousePosition - lastPanPosition;
            rect.anchoredPosition += delta * panSpeed;
            lastPanPosition = Input.mousePosition;
        }
    }

    void HandleTouch()
    {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);

            Vector2 prevDist = (t0.position - t0.deltaPosition) - (t1.position - t1.deltaPosition);
            Vector2 currDist = t0.position - t1.position;

            float diff = currDist.magnitude - prevDist.magnitude;
            Zoom(diff * 0.002f);
        }
        else if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
                rect.anchoredPosition += t.deltaPosition * panSpeed;
        }
    }

    void Zoom(float increment)
    {
        float scale = Mathf.Clamp(rect.localScale.x + increment, minZoom, maxZoom);
        rect.localScale = Vector3.one * scale;
    }
}

