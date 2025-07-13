using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;
[RequireComponent(typeof(Player))]
[RequireComponent(typeof(PlayerController))]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private Behaviour[] componentToDisble;

    [SerializeField] private string remoteLayerName = "RemotePlayer";

    [SerializeField] private string dontDrawLayerName = "DontDraw";
    [SerializeField] private GameObject playerGraphics;

    [SerializeField] private GameObject playerUIPrefab;
    
    [HideInInspector]public GameObject playerUIInstance;

    private void Start()
    {
        if (!isLocalPlayer)
        {
            DisableComponents();
            AssignRemoteLayer();
        }
        else
        {
            SetLayerRecursively(playerGraphics, LayerMask.NameToLayer(dontDrawLayerName));
            playerUIInstance = Instantiate(playerUIPrefab);
            playerUIInstance.name = playerUIPrefab.name;

            PlayerUI ui = playerUIInstance.GetComponent<PlayerUI>();
            if(ui == null)
            {
                Debug.Log("No PlayerUI comment on playerUI prefab");
            }
            ui.SetController(GetComponent<PlayerController>());

            GetComponent<Player>().SetupPlayer();
        }
    }
    private void SetLayerRecursively(GameObject obj ,int newLayer)
    {
        obj.layer = newLayer;
        foreach(Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
    public override void OnStartClient()
    {
        base.OnStartClient();

        string _netID = GetComponent<NetworkIdentity>().netId.ToString();
        Player _player = GetComponent<Player>();

        GameManager.RegisterPlayer(_netID,_player);
    }
    private void AssignRemoteLayer()
    {
        gameObject.layer = LayerMask.NameToLayer(remoteLayerName);
    }
    private void DisableComponents()
    {
        for (int i = 0; i < componentToDisble.Length; i++)
        {
            componentToDisble[i].enabled = false;
        }
    }
    private void OnDisable()
    {
        Destroy(playerUIInstance);
        
        if(isLocalPlayer)
            GameManager.instance.SetSceneCameraActive(true);

        GameManager.UnregisterPlayer(transform.name);
    }
}
