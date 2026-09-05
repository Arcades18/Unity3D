using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject homePanel;
    public GameObject mainMenu;
    public GameObject cameraPanel;
    public GameObject modeSelectionPanel;
    public GameObject manualPanel;
    public GameObject resultPanel;
    public GameObject instructionPopup;
    public GameObject referencePopup;
    public GameObject suggestionPanel;

    [Header("UI")]
    public RawImage cameraView;
    public Text resultsText;
    public Text suggestionText;

    [Header("Logic")]
    public SimpleImageProcessor imageProcessor;
    public ManualSelectionManager manualManager;

    private WebCamTexture camTex;
    private Texture2D captured;
    private DentalData currentData;

    void Start()
    {
        ShowHome();
        InitCamera();
    }

    void InitCamera()
    {
        if (WebCamTexture.devices.Length == 0) return;
        camTex = new WebCamTexture();
        cameraView.texture = camTex;
    }

    // ---------------- UI FLOW ----------------

    public void ShowHome()
    {
        homePanel.SetActive(true);
        mainMenu.SetActive(false);
        cameraPanel.SetActive(false);
        modeSelectionPanel.SetActive(false);
        manualPanel.SetActive(false);
        resultPanel.SetActive(false);

        if (camTex != null && camTex.isPlaying)
            camTex.Stop();
    }

    public void StartButton()
    {
        homePanel.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void OpenCamera()
    {
        mainMenu.SetActive(false);
        cameraPanel.SetActive(true);

        if (!camTex.isPlaying)
            camTex.Play();
    }

    public void CameraBackButton()
    {
        cameraPanel.SetActive(false);
        mainMenu.SetActive(true);
    }

    // ---------------- IMAGE CAPTURE ----------------

    public void Capture()
    {
        StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        yield return new WaitForEndOfFrame();

        captured = new Texture2D(camTex.width, camTex.height);
        captured.SetPixels(camTex.GetPixels());
        captured.Apply();

        currentData = imageProcessor.ProcessImage(captured);
        OpenModeSelection();
    }

    public void PickImageFromGallery()
    {
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path == null) return;

            captured = NativeGallery.LoadImageAtPath(path, 2048, false);
            if (captured == null) return;

            currentData = imageProcessor.ProcessImage(captured);
            OpenModeSelection();
        }, "Select a clear teeth photo");
    }

    void OpenModeSelection()
    {
        mainMenu.SetActive(false);
        cameraPanel.SetActive(false);
        modeSelectionPanel.SetActive(true);
    }

    // ---------------- AUTO / MANUAL ----------------

    public void AutoButton()
    {
        modeSelectionPanel.SetActive(false);
        UpdateResultsUI();
        ShowResultsPanel();
    }

    public void StartManual()
    {
        modeSelectionPanel.SetActive(false);
        manualPanel.SetActive(true);
        manualManager.StartManualSelection(captured);
    }

    public void ReceiveManualSelections(DentalData manualData)
    {
        manualPanel.SetActive(false);
        currentData = manualData;

        UpdateResultsUI();
        ShowResultsPanel();
    }

    // ---------------- RESULTS ----------------

    void ShowResultsPanel()
    {
        cameraPanel.SetActive(false);
        resultPanel.SetActive(true);
    }

    void UpdateResultsUI()
    {
        if (currentData == null)
        {
            resultsText.text = "No data available.";
            return;
        }

        if (!string.IsNullOrEmpty(currentData.errorMessage))
        {
            resultsText.text =
                "Analysis Failed\n\n" +
                currentData.errorMessage +
                "\n\nTry Manual Selection.";
            return;
        }

        resultsText.text =
            "Dental Analysis Result\n\n" +
            $"Mode: {currentData.analysisMode}\n" +
            $"Teeth Detected: {currentData.teethDetected}\n" +
            $"Total Width: {currentData.totalWidthCM:F2} cm\n\n" +
            (currentData.hasCoinReference
                ? "✓ Coin reference used"
                : "⚠ Estimated measurement") +
            "\n\nBrush recommendations coming next.";
    }
    public void UpdateSuggestionUI()
    {
        if (currentData == null)
        {
            suggestionText.text = "No data available.";
            return;
        }

        if (!string.IsNullOrEmpty(currentData.errorMessage))
        {
            suggestionText.text =
                "Analysis Failed\n\n" +
                currentData.errorMessage +
                "\n\nTry Manual Selection.";
            return;
        }
        if(currentData.totalWidthCM < 2.6)
        {
            suggestionText.text =

            "Detected Teeth Category: SMALL SIZE\n\n" +

            "Tooth Brush:\n" +
            "Small or compact head with soft bristles\n" +
            "Allows precise cleaning without irritating gums.\n\n" +

            "Toothpaste Amount:\n" +
            "Rice grain-sized amount\n" +
            "Especially suitable for smaller teeth and sensitive gums.\n\n" +

            "Brushing Technique:\n" +
            "Modified Bass technique, 2 minutes\n" +
            "Use gentle circular motions.\n\n" +

            "Note:\n" +
            "This app provides visual estimation and general oral care guidance.\n" +
            "It is not a substitute for professional dental consultation.";


        }
        else if(2.6 < currentData.totalWidthCM && currentData.totalWidthCM > 3.2)
        {
            suggestionText.text =
                   
                        "Detected Teeth Category: MEDIUM SIZE\n\n" +

                        "Tooth Brush:\n" +
                        "Medium or large head with soft bristles\n" +
                        "Covers 2–3 teeth at a time for effective cleaning.\n\n" +

                        "Toothpaste Amount:\n" +
                        "Pea to peanut-sized amount\n" +
                        "Using more paste does not improve cleaning.\n\n" +

                        "Brushing Technique:\n" +
                        "Modified Bass technique, 2 minutes\n" +
                        "Brush twice daily and floss regularly.\n\n" +

                        "Note:\n" +
                        "This app provides visual estimation and general oral care guidance.\n" +
                        "It is not a substitute for professional dental consultation.";

        }
        else if(currentData.totalWidthCM > 3.2)
        {
            suggestionText.text =

            "Detected Teeth Category: LARGE SIZE\n\n" +

            "Tooth Brush:\n" +
            "Large or wide head with medium bristles\n" +
            "Improves coverage for wider teeth surfaces.\n\n" +

            "Toothpaste Amount:\n" +
            "Pea-sized amount or slightly more\n" +
            "Ensure even distribution across all teeth.\n\n" +

            "Brushing Technique:\n" +
            "Modified Bass technique, 2–3 minutes\n" +
            "Spend extra time on molars.\n\n" +

            "Note:\n" +
            "This app provides visual estimation and general oral care guidance.\n" +
            "It is not a substitute for professional dental consultation.";

        }
    }

    // ---------------- POPUPS ----------------

    public void InstructionOkButton()
    {
        instructionPopup.SetActive(false);
    }


    public void ReferenceOkButton()
    {
        referencePopup.SetActive(false);
    }
    public void HomeButton()
    {
        suggestionPanel.SetActive(false);
        homePanel.SetActive(true);
    }
    public void SuggestionButton()
    {
        UpdateSuggestionUI();
        resultPanel.SetActive(false);
        suggestionPanel.SetActive(true);
    }
}
