using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

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

    private Texture2D currentPhoto;
    private List<GameObject> selectionMarkers = new List<GameObject>();
    private List<Vector2> selectedPositions = new List<Vector2>();
    private List<Vector2> coinEdgePoints = new List<Vector2>(); // Two points for diameter
    private float coinDiameterPixels = 0f;
    private GameObject currentCoinMarker;

    void Start()
    {
        confirmButton.onClick.AddListener(ConfirmSelections);
        resetButton.onClick.AddListener(ResetSelections);
    }

    void Update()
    {
        if (manualSelectionPanel.activeInHierarchy && Input.GetMouseButtonDown(0))
        {
            HandleTap(Input.mousePosition);
        }
    }

    public void StartManualSelection(Texture2D photo)
    {
        currentPhoto = photo;
        photoDisplay.texture = photo;
        manualSelectionPanel.SetActive(true);
        ResetSelections();
    }

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

        if (normalizedPos.x >= 0 && normalizedPos.x <= 1 && normalizedPos.y >= 0 && normalizedPos.y <= 1)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Coin measurement mode
                AddCoinEdgePoint(normalizedPos);
            }
            else
            {
                // Teeth selection mode
                if (!requireCoinForAccuracy || coinEdgePoints.Count == 2)
                {
                    AddSelectionMarker(normalizedPos);
                }
                else
                {
                    instructionText.text = "Please measure the coin first!\nHold SHIFT and click on two opposite edges of the coin.";
                }
            }
        }
    }

    private void AddCoinEdgePoint(Vector2 normalizedPos)
    {
        if (coinEdgePoints.Count >= 2)
        {
            // Reset if already have 2 points
            coinEdgePoints.Clear();
            if (currentCoinMarker != null)
                Destroy(currentCoinMarker);
        }

        coinEdgePoints.Add(normalizedPos);

        // Add visual marker
        GameObject marker = Instantiate(coinMarkerPrefab, photoDisplay.transform);
        RectTransform markerRect = marker.GetComponent<RectTransform>();

        markerRect.anchorMin = new Vector2(0, 0);
        markerRect.anchorMax = new Vector2(0, 0);
        markerRect.pivot = new Vector2(0.5f, 0.5f);

        float xPos = normalizedPos.x * photoDisplay.rectTransform.rect.width;
        float yPos = normalizedPos.y * photoDisplay.rectTransform.rect.height;

        markerRect.anchoredPosition = new Vector2(xPos, yPos);
        markerRect.sizeDelta = new Vector2(30, 30);

        // Style for coin points
        Image markerImage = marker.GetComponent<Image>();
        if (markerImage != null)
        {
            markerImage.color = coinEdgePoints.Count == 1 ? Color.yellow : Color.red;
        }

        Text markerText = marker.GetComponentInChildren<Text>();
        if (markerText != null)
        {
            markerText.text = coinEdgePoints.Count.ToString();
            markerText.color = Color.white;
        }

        // Store first marker for line drawing
        if (coinEdgePoints.Count == 1)
        {
            currentCoinMarker = marker;
        }

        UpdateCoinMeasurement();
    }

    private void UpdateCoinMeasurement()
    {
        if (coinEdgePoints.Count == 2)
        {
            // Calculate actual coin diameter in pixels
            float point1X = coinEdgePoints[0].x * currentPhoto.width;
            float point1Y = coinEdgePoints[0].y * currentPhoto.height;
            float point2X = coinEdgePoints[1].x * currentPhoto.width;
            float point2Y = coinEdgePoints[1].y * currentPhoto.height;

            coinDiameterPixels = Mathf.Sqrt(Mathf.Pow(point2X - point1X, 2) + Mathf.Pow(point2Y - point1Y, 2));

            instructionText.text = $"✓ Coin measured: {coinDiameterPixels:F0} pixels\nNow select your 4 front teeth";

            Debug.Log($"Coin diameter: {coinDiameterPixels} pixels = {coinDiameterCM} cm");
        }
        else if (coinEdgePoints.Count == 1)
        {
            instructionText.text = "Click on the opposite edge of the coin\n(Hold SHIFT + Click)";
        }
        else
        {
            instructionText.text = "Hold SHIFT and click on two opposite edges of the ₹5 coin\nThen select your 4 front teeth";
        }
    }

    private void AddSelectionMarker(Vector2 normalizedPos)
    {
        if (selectedPositions.Count >= 4) return;

        GameObject marker = Instantiate(selectionMarkerPrefab, photoDisplay.transform);
        RectTransform markerRect = marker.GetComponent<RectTransform>();

        markerRect.anchorMin = new Vector2(0, 0);
        markerRect.anchorMax = new Vector2(0, 0);
        markerRect.pivot = new Vector2(0.5f, 0.5f);

        float xPos = normalizedPos.x * photoDisplay.rectTransform.rect.width;
        float yPos = normalizedPos.y * photoDisplay.rectTransform.rect.height;

        markerRect.anchoredPosition = new Vector2(xPos, yPos);
        markerRect.sizeDelta = new Vector2(40, 40);

        Text markerText = marker.GetComponentInChildren<Text>();
        if (markerText != null)
        {
            markerText.text = (selectedPositions.Count + 1).ToString();
        }

        selectionMarkers.Add(marker);
        selectedPositions.Add(normalizedPos);

        UpdateTeethMeasurement();
    }

    private void UpdateTeethMeasurement()
    {
        if (selectedPositions.Count >= 2)
        {
            float estimatedCM = CalculateHighAccuracyMeasurement();
            string accuracy = coinEdgePoints.Count == 2 ? "High (±0.2 cm)" : "Low (±1.0 cm)";

            instructionText.text = $"{selectedPositions.Count}/4 teeth selected\n" +
                                  $"Estimated width: {estimatedCM:F2} cm\n" +
                                  $"Accuracy: {accuracy}\n" +
                                  "Tap to select more teeth";
        }
        else
        {
            string coinStatus = coinEdgePoints.Count == 2 ? "✓ Coin measured" : "ⓘ Measure coin for accuracy";
            instructionText.text = $"Tap on the centers of your 4 front teeth\n" +
                                  $"{selectedPositions.Count}/4 selected\n" +
                                  $"{coinStatus}";
        }
    }

    private float CalculateHighAccuracyMeasurement()
    {
        if (selectedPositions.Count < 2) return 0f;

        // Sort positions left to right
        selectedPositions.Sort((a, b) => a.x.CompareTo(b.x));

        // Calculate teeth width in pixels
        float leftMost = selectedPositions[0].x * currentPhoto.width;
        float rightMost = selectedPositions[selectedPositions.Count - 1].x * currentPhoto.width;
        float teethWidthPixels = rightMost - leftMost;

        // Add margin for tooth edges (15% total)
        teethWidthPixels *= 1.15f;

        // Convert to CM
        if (coinEdgePoints.Count == 2 && coinDiameterPixels > 0)
        {
            // High accuracy: use actual coin measurement
            float pixelsPerCM = coinDiameterPixels / coinDiameterCM;
            return teethWidthPixels / pixelsPerCM;
        }
        else
        {
            // Low accuracy: estimation
            float estimatedPixelsPerCM = currentPhoto.width / 10f;
            return teethWidthPixels / estimatedPixelsPerCM;
        }
    }

    private void ResetSelections()
    {
        // Clear teeth markers
        foreach (GameObject marker in selectionMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        selectionMarkers.Clear();
        selectedPositions.Clear();

        // Clear coin markers
        coinEdgePoints.Clear();
        coinDiameterPixels = 0f;
        if (currentCoinMarker != null)
        {
            Destroy(currentCoinMarker);
            currentCoinMarker = null;
        }

        // Clear all markers from photo display
        foreach (Transform child in photoDisplay.transform)
        {
            if (child.name.Contains("Clone"))
                Destroy(child.gameObject);
        }

        instructionText.text = "Hold SHIFT and click on two opposite edges of the ₹5 coin\nThen select your 4 front teeth";
    }

    private void ConfirmSelections()
    {
        if (requireCoinForAccuracy && coinEdgePoints.Count != 2)
        {
            instructionText.text = "⚠ Please measure the coin first for accurate results!\nHold SHIFT and click on two opposite edges.";
            return;
        }

        if (selectedPositions.Count >= 2)
        {
            DentalData data = new DentalData();
            data.teethDetected = selectedPositions.Count;
            data.totalWidthCM = CalculateHighAccuracyMeasurement();
            data.hasCoinReference = (coinEdgePoints.Count == 2);
            data.accuracyEstimate = data.hasCoinReference ? "High (±0.2 cm)" : "Low (±1.0 cm)";

            GameManager gameManager = GetComponentInParent<GameManager>();
            if (gameManager != null)
            {
                gameManager.ReceiveManualSelections(data);
            }

            manualSelectionPanel.SetActive(false);
        }
        else
        {
            instructionText.text = "Please select at least 2 teeth!";
        }
    }
}