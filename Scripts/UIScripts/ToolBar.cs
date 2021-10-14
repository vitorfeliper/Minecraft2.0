using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
    public UIItemSlot[] slots;
    public RectTransform highlight;

    public Player player;

    public int slotIndex = 0;

    private void Start()
    {
        byte index = 1;
        foreach(UIItemSlot s in slots)
        {
            ItemStack stack = new ItemStack(index, Random.Range(2, 65));
            ItemSlot2 slot = new ItemSlot2(s, stack);
            index++;
        }
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll != 0)
        {
            if(scroll > 0)
            {
                slotIndex--;
            }
            else
            {
                slotIndex++;
            }

            if(slotIndex > slots.Length - 1)
            {
                slotIndex = 0;
            }

            if (slotIndex < 0)
            {
                slotIndex = slots.Length - 1;
            }

            highlight.position = slots[slotIndex].slotIcon.transform.position;
            //player.selectedBlockIndex = slots[slotIndex].itemSlot.stack.id; 
        }
    }

}

/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolBar : MonoBehaviour
{
    World world;
    public Player player;

    public RectTransform highlight;
    public ItemSlot[] itemSlots;

    int slotIndex = 0;

    private void Start()
    {
        world = GameObject.Find("World").GetComponent<World>();

        foreach(ItemSlot slot in itemSlots)
        {
            slot.icon.sprite = world.blockTypes[slot.itemID].icon;
            slot.icon.enabled = true;
        }

        player.selectedBlockIndex = itemSlots[slotIndex].itemID;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if(scroll != 0)
        {
            if(scroll > 0)
            {
                slotIndex--;
            }
            else
            {
                slotIndex++;
            }

            if(slotIndex > itemSlots.Length - 1)
            {
                slotIndex = 0;
            }

            if(slotIndex < 0)
            {
                slotIndex = itemSlots.Length - 1;
            }

            highlight.position = itemSlots[slotIndex].icon.transform.position;
            player.selectedBlockIndex = itemSlots[slotIndex].itemID;
        }
    }

}

[System.Serializable]
public class ItemSlot
{
    public byte itemID;
    public Image icon;
}
*/