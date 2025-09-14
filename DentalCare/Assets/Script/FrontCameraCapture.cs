using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FrontCameraCapture : MonoBehaviour
{
    public RawImage cameraPreview;
    public Button scanButton;
    public Button takePhotoButton;
    public GameObject arCamera;
    public GameObject startPanel;
    public GameObject cameraPreviewPanel;

    private WebCamTexture frontCameraTexture;
    private Texture2D capturedPhoto;
    private bool isCameraRunning = false;

    void Start()
    {
        scanButton.onClick.AddListener(StartFrontCamera);
        takePhotoButton.onClick.AddListener(CapturePhoto);

        takePhotoButton.gameObject.SetActive(false);
        Screen.orientation = ScreenOrientation.Portrait;  // Force portrait mode
    }

    public void StartFrontCamera()
    {
        cameraPreviewPanel.SetActive(true);

        if (WebCamTexture.devices.Length == 0)
        {
            Debug.LogWarning("No camera detected");
            return;
        }

        WebCamDevice? frontCamDevice = null;
        foreach (var device in WebCamTexture.devices)
        {
            if (device.isFrontFacing)
            {
                frontCamDevice = device;
                break;
            }
        }

        if (frontCamDevice == null)
        {
            Debug.LogError("Front camera not found");
            return;
        }

        frontCameraTexture = new WebCamTexture(frontCamDevice.Value.name, Screen.width, Screen.height);
        frontCameraTexture.requestedFPS = 30;

        cameraPreview.texture = frontCameraTexture;
        frontCameraTexture.Play();
        isCameraRunning = true;

        startPanel.SetActive(false);
        takePhotoButton.gameObject.SetActive(true);

        StartCoroutine(AdjustCameraPreview());
    }

    private IEnumerator AdjustCameraPreview()
    {
        yield return new WaitUntil(() => frontCameraTexture.width > 100);

        float ratio = (float)frontCameraTexture.width / frontCameraTexture.height;
        cameraPreview.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.width / ratio);
    }

    public void CapturePhoto()
    {
        if (frontCameraTexture == null || !frontCameraTexture.isPlaying)
        {
            Debug.LogWarning("Front camera not running");
            return;
        }

        capturedPhoto = new Texture2D(frontCameraTexture.width, frontCameraTexture.height);
        capturedPhoto.SetPixels(frontCameraTexture.GetPixels());
        capturedPhoto.Apply();

        cameraPreview.texture = capturedPhoto;

        frontCameraTexture.Stop();
        isCameraRunning = false;

        cameraPreviewPanel.SetActive(false);
        arCamera.SetActive(true);

        Debug.Log("Photo captured and camera preview closed");

        // Continue with teeth size detection logic using 'capturedPhoto'
    }

    private void OnDisable()
    {
        if (frontCameraTexture != null && frontCameraTexture.isPlaying)
        {
            frontCameraTexture.Stop();
        }
    }
}
