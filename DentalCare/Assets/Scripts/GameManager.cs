using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject cameraPanel;
    public GameObject resultsPanel;

    [Header("Camera References")]
    public RawImage cameraDisplay;
    public AspectRatioFitter aspectFitter;

    private WebCamTexture webcamTexture;
    private bool isCameraAvailable;
    private Texture2D capturedPhoto;

    [Header("Manual Selection")]
    public GameObject manualSelectionPanel;
    public ManualSelectionManager manualSelectionManager;

    void Start()
    {
        ShowMainMenu();
        InitializeCamera();
    }

    void InitializeCamera()
    {
        // Check if device has camera
        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogError("No camera found on device!");
            return;
        }

        // Get back camera (usually better quality for face photos)
        WebCamDevice backCamera = WebCamTexture.devices[0];
        for (int i = 0; i < WebCamTexture.devices.Length; i++)
        {
            if (!WebCamTexture.devices[i].isFrontFacing)
            {
                backCamera = WebCamTexture.devices[i];
                break;
            }
        }

        // Create webcam texture
        webcamTexture = new WebCamTexture(backCamera.name, 1920, 1080);
        cameraDisplay.texture = webcamTexture;

        isCameraAvailable = true;
    }

    void Update()
    {
        // Update camera display rotation and aspect ratio
        if (isCameraAvailable)
        {
            // Adjust for device rotation
            int rotation = -webcamTexture.videoRotationAngle;
            cameraDisplay.rectTransform.localEulerAngles = new Vector3(0, 0, rotation);

            // Adjust aspect ratio
            float ratio = (float)webcamTexture.width / (float)webcamTexture.height;
            aspectFitter.aspectRatio = ratio;
        }
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        cameraPanel.SetActive(false);
        resultsPanel.SetActive(false);

        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }

    public void ShowCamera()
    {
        mainMenuPanel.SetActive(false);
        cameraPanel.SetActive(true);
        resultsPanel.SetActive(false);

        if (isCameraAvailable && !webcamTexture.isPlaying)
        {
            webcamTexture.Play();
        }
    }

    public void ShowResults()
    {
        mainMenuPanel.SetActive(false);
        cameraPanel.SetActive(false);
        resultsPanel.SetActive(true);

        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }
    }

    public void CapturePhoto()
    {
        if (!isCameraAvailable || !webcamTexture.isPlaying)
        {
            Debug.LogError("Camera not available!");
            return;
        }

        StartCoroutine(TakePhoto());
    }

    [Header("Image Processing")]
    public SimpleImageProcessor imageProcessor;
    public Text resultsText; // Assign this in Inspector

    // Add these variables to existing GameManager class
    private DentalData currentDentalData;

    // Update the TakePhoto coroutine:
    private IEnumerator TakePhoto()
    {
        yield return new WaitForEndOfFrame();

        capturedPhoto = new Texture2D(webcamTexture.width, webcamTexture.height);
        capturedPhoto.SetPixels(webcamTexture.GetPixels());
        capturedPhoto.Apply();

        Debug.Log("Photo captured! Processing...");

        // Process the image
        currentDentalData = imageProcessor.ProcessImage(capturedPhoto);

        ShowResults();
        UpdateResultsUI();
    }

    // Update the PickImageFromGallery method:
    public void PickImageFromGallery()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
            {
                Texture2D texture = NativeGallery.LoadImageAtPath(path, 1024, false);
                if (texture != null)
                {
                    capturedPhoto = texture;
                    Debug.Log("Image loaded from gallery! Processing...");

                    // Process the image
                    currentDentalData = imageProcessor.ProcessImage(capturedPhoto);

                    ShowResults();
                    UpdateResultsUI();
                }
            }
        }, "Select a teeth photo", "image/*");
    }


    private void UpdateResultsUI()
    {
        if (currentDentalData == null) return;

        string resultText = "";

        if (!string.IsNullOrEmpty(currentDentalData.errorMessage))
        {
            resultText = "Automatic detection failed!\n\n";
            resultText += currentDentalData.errorMessage;
            resultText += "\n\nClick 'Try Manual Selection' below to measure manually.";

            // DON'T automatically start manual selection - just show the option
            // Remove this line: StartManualSelection();
        }
        else
        {
            resultText = $"Teeth Analysis Results:\n\n";
            resultText += $"Teeth Detected: {currentDentalData.teethDetected}\n";
            resultText += $"Total Width: {currentDentalData.totalWidthCM:F2} cm\n";

            if (currentDentalData.hasCoinReference)
            {
                resultText += "✓ Measurement with coin reference\n";
            }
            else
            {
                resultText += "⚠ Estimated measurement\n";
            }

            resultText += $"\nProcessing completed!\n(Step 4 will add brush recommendations)";
        }

        if (resultsText != null)
        {
            resultsText.text = resultText;
        }
    }

    private void ShowManualSelectionOption()
    {
        // Add a manual selection button to your ResultsPanel
        // Or you can automatically switch to manual mode
        StartManualSelection();
    }

    private void HideManualSelectionOption()
    {
        // Hide any manual selection UI elements
    }
    public void StartManualSelection()
    {
        if (capturedPhoto != null)
        {
            manualSelectionPanel.SetActive(true);
            manualSelectionManager.StartManualSelection(capturedPhoto);
        }
    }

    public void ReceiveManualSelections(DentalData manualData)
    {
        if (manualSelectionPanel != null)
            manualSelectionPanel.SetActive(false);

        currentDentalData = manualData;

        // Force update the UI with manual results
        if (string.IsNullOrEmpty(currentDentalData.errorMessage))
        {
            // Success case - show manual results
            string resultText = $"Manual Measurement Complete!\n\n";
            resultText += $"Teeth Selected: {currentDentalData.teethDetected}\n";
            resultText += $"Estimated Width: {currentDentalData.totalWidthCM:F2} cm\n";
            resultText += $"\nReady for brush recommendations!";

            if (resultsText != null)
            {
                resultsText.text = resultText;
            }
        }
        else
        {
            // Error in manual selection
            if (resultsText != null)
            {
                resultsText.text = $"Manual Selection Error: {currentDentalData.errorMessage}";
            }
        }

        // Make sure we're on results panel
        ShowResults();
    }

    private void UpdateManualResultsUI()
    {
        string resultText = $"Manual Measurement Results:\n\n";
        resultText += $"Teeth Selected: {currentDentalData.teethDetected}\n";
        resultText += $"Estimated Total Width: {currentDentalData.totalWidthCM:F2} cm\n";
        resultText += $"\nBrush recommendations coming in Step 4!";

        if (resultsText != null)
        {
            resultsText.text = resultText;
        }
    }
}