using UnityEngine;
using System.Collections.Generic;

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] Animator animator;
    [SerializeField] List<GameObject> lootPrefabs;
    [SerializeField] GameObject miniMapIcon;


    public bool IsOpened { get; private set;}

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        IsOpened = false;
    }

    public void Interact()
    {
        if (!IsInteractable())
            return;

        OpenChest();
    }

    public bool IsInteractable()
    {
        return !IsOpened;
    }

    void OpenChest()
    {
        // set animation to open
        animator.Play("chestOpen");
        IsOpened = true;

        if (miniMapIcon)
            miniMapIcon.SetActive(false);
    }

    void DropLoot()
    {
        if (lootPrefabs == null || lootPrefabs.Count == 0)
            return;

        int prefabIndex = Random.Range(0, lootPrefabs.Count);

        // drop prefab
        GameObject item = Instantiate(lootPrefabs[prefabIndex], transform.position + Vector3.down * 0.2f, Quaternion.identity);
    }
}
