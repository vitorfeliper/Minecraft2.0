using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeInventory : MonoBehaviour
{
    public GameObject slotPrefab;
    World world;

    List<ItemSlot2> slots = new List<ItemSlot2>();

    public void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

        for(int i = 1; i < world.blockTypes.Length; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, transform);

            ItemStack stack = new ItemStack((byte)i, 64);

            ItemSlot2 slot = new ItemSlot2(newSlot.GetComponent<UIItemSlot>(), stack);

            slot.isCreative = true;
        }
    }
}
