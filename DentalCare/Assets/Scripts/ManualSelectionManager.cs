using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ManualSelectionManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject manualSelectionPanel;
    public RawImage photoDisplay;
    public Text instructionText;
    public Button confirmButton;
    public Button resetButton;

    [Header("Markers")]
    public GameObject selectionMarkerPrefab;
    public GameObject coinMarkerPrefab;

    [Header("Accuracy Settings")]
    public float coinDiameterCM = 2.3f; // ₹5 coin
    public bool requireCoinForAccuracy = true;

    [Header("Input Mode")]
    public bool coinSelectionMode = true; // TRUE = Coin, FALSE = Teeth

    private Texture2D currentPhoto;
    private List<GameObject> selectionMarkers = new List<GameObject>();
    private List<Vector2> selectedPositions = new List<Vector2>();
    private List<Vector2> coinEdgePoints = new List<Vector2>();
    private float coinDiameterPixels = 0f;
    private GameObject currentCoinMarker;

    void Start()
    {
        confirmButton.onClick.AddListener(ConfirmSelections);
        resetButton.onClick.AddListener(ResetSelections);
    }

    void Update()
    {
        if (!manualSelectionPanel.activeInHierarchy)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleTap(Input.mousePosition);
        }
        else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            HandleTap(Input.GetTouch(0).position);
        }
    }

    public void StartManualSelection(Texture2D photo)
    {
        currentPhoto = photo;
        photoDisplay.texture = photo;
        manualSelectionPanel.SetActive(true);
        ResetSelections();
    }

    // ---------------- MODE TOGGLE (CALL FROM UI) ----------------

    public void ToggleSelectionMode(bool isCoinMode)
    {
        coinSelectionMode = isCoinMode;

        if (coinSelectionMode)
        {
            instructionText.text =
                "Coin Mode ON\nTap two opposite edges of the ₹5 coin";
        }
        else
        {
            instructionText.text =
                "Teeth Mode ON\nTap on the centers of your front teeth";
        }
    }

    // ---------------- INPUT HANDLING ----------------

    private void HandleTap(Vector2 screenPosition)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            photoDisplay.rectTransform, screenPosition, null, out localPoint);

        Rect rect = photoDisplay.rectTransform.rect;

        Vector2 normalizedPos = new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height
        );

        if (normalizedPos.x < 0 || normalizedPos.x > 1 ||
            normalizedPos.y < 0 || normalizedPos.y > 1)
            return;

        if (coinSelectionMode)
        {
            AddCoinEdgePoint(normalizedPos);
        }
        else
        {
            if (!requireCoinForAccuracy || coinEdgePoints.Count == 2)
            {
                AddSelectionMarker(normalizedPos);
            }
            else
            {
                instructionText.text =
                    "Please measure the coin first for accurate results.";
            }
        }
    }

    // ---------------- COIN MEASUREMENT ----------------

    private void AddCoinEdgePoint(Vector2 normalizedPos)
    {
        if (coinEdgePoints.Count >= 2)
        {
            coinEdgePoints.Clear();
            if (currentCoinMarker != null)
                Destroy(currentCoinMarker);
        }

        coinEdgePoints.Add(normalizedPos);

        GameObject marker = Instantiate(coinMarkerPrefab, photoDisplay.transform);
        RectTransform markerRect = marker.GetComponent<RectTransform>();

        markerRect.anchorMin = Vector2.zero;
        markerRect.anchorMax = Vector2.zero;
        markerRect.pivot = new Vector2(0.5f, 0.5f);

        markerRect.anchoredPosition = new Vector2(
            normalizedPos.x * photoDisplay.rectTransform.rect.width,
            normalizedPos.y * photoDisplay.rectTransform.rect.height
        );

        markerRect.sizeDelta = new Vector2(20, 20);

        Image img = marker.GetComponent<Image>();
        if (img != null)
            img.color = coinEdgePoints.Count == 1 ? Color.yellow : Color.red;

        if (coinEdgePoints.Count == 1)
            currentCoinMarker = marker;

        UpdateCoinMeasurement();
    }

    private void UpdateCoinMeasurement()
    {
        if (coinEdgePoints.Count == 2)
        {
            Vector2 p1 = coinEdgePoints[0] * new Vector2(currentPhoto.width, currentPhoto.height);
            Vector2 p2 = coinEdgePoints[1] * new Vector2(currentPhoto.width, currentPhoto.height);

            coinDiameterPixels = Vector2.Distance(p1, p2);

            instructionText.text =
                $"✓ Coin measured\nNow switch to Teeth Mode";
        }
    }

    // ---------------- TEETH SELECTION ----------------

    private void AddSelectionMarker(Vector2 normalizedPos)
    {
        if (selectedPositions.Count >= 4) return;

        GameObject marker = Instantiate(selectionMarkerPrefab, photoDisplay.transform);
        RectTransform rt = marker.GetComponent<RectTransform>();

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition = new Vector2(
            normalizedPos.x * photoDisplay.rectTransform.rect.width,
            normalizedPos.y * photoDisplay.rectTransform.rect.height
        );

        rt.sizeDelta = new Vector2(20, 20);

        selectionMarkers.Add(marker);
        selectedPositions.Add(normalizedPos);

        UpdateTeethMeasurement();
    }

    private void UpdateTeethMeasurement()
    {
        float cm = CalculateHighAccuracyMeasurement();
        instructionText.text =
            $"{selectedPositions.Count}/4 teeth selected\n" +
            $"Estimated width: {cm:F2} cm";
    }

    private float CalculateHighAccuracyMeasurement()
    {
        if (selectedPositions.Count < 2) return 0f;

        selectedPositions.Sort((a, b) => a.x.CompareTo(b.x));

        float left = selectedPositions[0].x * currentPhoto.width;
        float right = selectedPositions[selectedPositions.Count - 1].x * currentPhoto.width;

        float teethWidthPixels = (right - left) * 1.15f;

        if (coinEdgePoints.Count == 2 && coinDiameterPixels > 0)
        {
            float pixelsPerCM = coinDiameterPixels / coinDiameterCM;
            return teethWidthPixels / pixelsPerCM;
        }

        return teethWidthPixels / (currentPhoto.width / 10f);
    }

    // ---------------- RESET & CONFIRM ----------------

    private void ResetSelections()
    {
        foreach (var m in selectionMarkers)
            Destroy(m);

        selectionMarkers.Clear();
        selectedPositions.Clear();
        coinEdgePoints.Clear();
        coinDiameterPixels = 0f;

        foreach (Transform t in photoDisplay.transform)
            if (t.name.Contains("Clone"))
                Destroy(t.gameObject);

        instructionText.text =
            "Start with Coin Mode → Measure ₹5 coin";
    }

    private void ConfirmSelections()
    {
        if (selectedPositions.Count < 2)
        {
            instructionText.text = "Select at least 2 teeth!";
            return;
        }

        DentalData data = new DentalData
        {
            teethDetected = selectedPositions.Count,
            totalWidthCM = CalculateHighAccuracyMeasurement() + 1f,
            hasCoinReference = coinEdgePoints.Count == 2,
            accuracyEstimate = coinEdgePoints.Count == 2 ? "High" : "Low"
        };

        GameManager gm = GetComponentInParent<GameManager>();
        if (gm != null)
            gm.ReceiveManualSelections(data);

        manualSelectionPanel.SetActive(false);
    }

}
