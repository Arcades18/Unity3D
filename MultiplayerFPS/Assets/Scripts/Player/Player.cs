using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;
[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour
{
    private bool _isDead = false;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField] private int maxHealth = 100;
    [SyncVar] private int currentHealth;

    [SerializeField] private Behaviour[] diableOnDeath;
    private bool[] wasEnabled;

    [SerializeField] private GameObject[] disbleGameObjectOnDeath;

    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject SpawnEffect;

    private bool firstSetup = true;

    //private void Update()
    //{
    //    if (!isLocalPlayer)
    //    {
    //        return;
    //    }
    //    if (Input.GetKeyDown(KeyCode.K))
    //    {
    //        RpcTakeDamage(9999);
    //    }
    //}
    public void SetupPlayer()
    {
        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(true);
        }

        CmdBroadCastNewPlayerSetup();
    }
    [Command]
    private void CmdBroadCastNewPlayerSetup()
    {
        RpcSetupPlayerOnClient();
    }
    [ClientRpc]
    private void RpcSetupPlayerOnClient()
    {
        if (firstSetup)
        {
            wasEnabled = new bool[diableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = diableOnDeath[i].enabled;
            }
            firstSetup = false;
        }
        SetDefaults();
    }

    [ClientRpc]
    public void RpcTakeDamage(int amount)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= amount;
        Debug.Log(transform.name + "now has" +  currentHealth + "Health");
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        for(int i = 0; i < diableOnDeath.Length; i++)
        {
            diableOnDeath[i].enabled = false;
        }
        for(int i = 0;i < disbleGameObjectOnDeath.Length; i++)
        {
            disbleGameObjectOnDeath[i].gameObject.SetActive(false);
        }
        Collider _collider = GetComponent<Collider>();
        if(_collider != null)
        {
            _collider.enabled = false;
        }

        GameObject _gfxIns = Instantiate(deathEffect,this.transform.position,Quaternion.identity);
        Destroy(_gfxIns, 3f);

        if (isLocalPlayer)
        {
            GameManager.instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
        }

        Debug.Log(transform.name + " is Dead!");

        StartCoroutine(Respawn());
    }
    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(GameManager.instance.matchSettings.respawnTime);

        Transform spawnPoint = NetworkManager.singleton.GetStartPosition();
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        yield return new WaitForSeconds(0.1f);

        SetupPlayer();

        Debug.Log(transform.name + " is Respawned");
    }
    public void SetDefaults()
    {
        isDead = false;

        currentHealth = maxHealth;

        for(int i = 0; i < diableOnDeath.Length; i++)
        {
            diableOnDeath[i].enabled = wasEnabled[i];
        }

        for (int i = 0; i < disbleGameObjectOnDeath.Length; i++)
        {
            disbleGameObjectOnDeath[i].gameObject.SetActive(true);
        }

        Collider _collider = GetComponent<Collider>();
        if(_collider != null)
        {
            _collider.enabled = true;
        }


        GameObject gfxIns = Instantiate(SpawnEffect, this.transform.position, Quaternion.identity);
        Destroy(gfxIns, 3f);
    }
}
