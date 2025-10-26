using UnityEngine;
using UnityEngine.Android;

public class AndroidPermissionsManager : MonoBehaviour
{
    void Start()
    {
        RequestCameraPermission();
        RequestStoragePermission();
    }

    void RequestCameraPermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
    }

    void RequestStoragePermission()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }
    }
}