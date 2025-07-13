using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Mirror;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private string weaponLayerName = "Weapon";

    [SerializeField] private PlayerWeapon primaryWeapon;
    [SerializeField] private Transform weaponHolder;
    private PlayerWeapon currentWeapon;
    private WeaponGraphics currentGraphics;

    private void Start()
    {
        EquipWeapon(primaryWeapon);
    }
    public PlayerWeapon GetCurrentWeapon()
    {
        return currentWeapon;
    }
    public WeaponGraphics GetWeaponGraphics()
    {
        return currentGraphics;
    }
    private void EquipWeapon(PlayerWeapon _weapon)
    {
        currentWeapon = _weapon;
        GameObject weaponIns= (GameObject)Instantiate(_weapon.graphic, weaponHolder.position, weaponHolder.rotation);
        weaponIns.transform.SetParent(weaponHolder);

        currentGraphics = weaponIns.GetComponent<WeaponGraphics>();
        if(currentGraphics == null )
        {
            Debug.Log("No Weapon Graphics" + weaponIns.name);
        }

        if (isLocalPlayer)
        {
            Util.SetLayerRecurively(weaponIns, LayerMask.NameToLayer(weaponLayerName));
        }
    }
}
