using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;
[RequireComponent(typeof(WeaponManager))]
public class PlayerShoot : NetworkBehaviour
{
    private const string PLAYER_TAG = "Player";

    private PlayerWeapon currentWeapon;

    [SerializeField] private LayerMask mask;
    [SerializeField] private Camera _camera;

    private WeaponManager weaponManager;
    private void Awake()
    {
        if( _camera == null)
        {
            Debug.Log("PlayerShoot : No Camera Reference");
            this.enabled = false;
        }
        weaponManager = GetComponent<WeaponManager>();
    }
    private void Update()
    {
        currentWeapon = weaponManager.GetCurrentWeapon();

        if(currentWeapon.fireRate <= 0)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot();
            }
        }
        else
        {
            if (Input.GetButtonDown("Fire1"))
            {
                InvokeRepeating("Shoot",0f,1f/currentWeapon.fireRate);
            }
            else if (Input.GetButtonUp("Fire1"))
            {
                CancelInvoke("Shoot");
            }
        }
    }

    [Command]
    private void CmdOnShoot()
    {
        RpcDoShootEffect();
    }
    [ClientRpc]
    private void RpcDoShootEffect()
    {
        weaponManager.GetWeaponGraphics().muzzelFlash.Play();
    }

    [Command]
    private void CmdOnHit(Vector3 _pos,Vector3 _normal)
    {
        RpcDoHitEffect(_pos,_normal);
    }
    [ClientRpc]
    private void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    {
       GameObject _hitEffect =  Instantiate(weaponManager.GetWeaponGraphics().hitEffectPrefab,_pos,Quaternion.LookRotation(_normal));
        Destroy(_hitEffect, 2f);
    }

    [Client]
    private void Shoot()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        CmdOnShoot();

        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit _hit;
        if (Physics.Raycast(ray,out _hit ,currentWeapon.range,mask))
        {
            if(_hit.collider.tag == PLAYER_TAG)
            {
                CmdPlayerShot(_hit.collider.name,currentWeapon.damage);
            }
            CmdOnHit(_hit.point, _hit.normal);
        }
    }
    [Command]
    private void CmdPlayerShot(string _playerID,int _damage)
    {
        Debug.Log(_playerID + "has been shot.");
        Player _player = GameManager.GetPlayer(_playerID);
        _player.RpcTakeDamage(_damage);
    }
}
