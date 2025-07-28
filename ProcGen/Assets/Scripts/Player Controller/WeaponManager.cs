using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    public int currentWeaponIndex;
    [SerializeField] List<BaseWeapon> weaponList;

    private void Start()
    {
        //weaponList = new List<BaseWeapon>();
    }

    public void Fire()
    {
        if (weaponList == null || weaponList.Count == 0)
            return;

        if (currentWeaponIndex < 0 || currentWeaponIndex >= weaponList.Count)
            return;

        BaseWeapon weapon = weaponList[currentWeaponIndex];
        if (weapon != null)
        {
            weapon.Fire();
        }
    }
    
    public bool CurrentWeaponIsSword()
    {
        return currentWeaponIndex == 0;
    }

    private void SwitchWeapon(int index)
    {
        if (index >= 0 && index < weaponList.Count)
        {
            currentWeaponIndex = index;

            for (int i = 0; i < weaponList.Count; ++i)
            {
                BaseWeapon weapon = weaponList[i];
                if (i == index)
                {
                    weapon.gameObject.SetActive(true);
                    continue;
                }
                weapon.gameObject.SetActive(false);
            }
        }
    }
    public void SwitchWeapon1(InputAction.CallbackContext context) 
    {
        SwitchWeapon(0);
        Debug.Log("Switch to weapon 0");
    }

    public void SwitchWeapon2(InputAction.CallbackContext context) 
    {
        SwitchWeapon(1);
        Debug.Log("Switch to weapon 1");
    }
}
