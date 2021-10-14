using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItemSlot : MonoBehaviour
{
    public bool isLinked = false;
    public ItemSlot2 itemSlot;
    public Image slotImage;
    public Image slotIcon;
    public Text slotAmouont;

    World world;

    private void Awake()
    {
        world = GameObject.Find("World").GetComponent<World>();
    }

    public bool HasItem
    {
        get
        {
            if(itemSlot == null)
            {
                return false;
            }
            else
            {
                return itemSlot.HasItem;
            }
        }
    }

    public void Link(ItemSlot2 _itemSlot)
    {
        itemSlot = _itemSlot;
        isLinked = true;

        itemSlot.LinkUISlot(this);
        UpdateSlot();
    }

    public void UnLink()
    {
        itemSlot.UnLinkUISlot();
        itemSlot = null;
        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if(itemSlot != null && itemSlot.HasItem)
        {
            slotIcon.sprite = world.blockTypes[itemSlot.stack.id].icon;
            slotAmouont.text = itemSlot.stack.amount.ToString();
            slotIcon.enabled = true;
            slotAmouont.enabled = true;
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        slotIcon.sprite = null;
        slotAmouont.text = "";
        slotIcon.enabled = false;
        slotAmouont.enabled = false;
    }


    public void OnDestroy()
    {
        if (itemSlot != null)
        {
            itemSlot.UnLinkUISlot();
        }
    }
}

public class ItemSlot2
{
    public ItemStack stack = null;
    private UIItemSlot uiItemSlot = null;

    public bool isCreative;

    public ItemSlot2(UIItemSlot _uiItemSlot)
    {
        stack = null;
        uiItemSlot = _uiItemSlot;

        uiItemSlot.Link(this);
    }

    public ItemSlot2(UIItemSlot _uiItemSlot, ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot = _uiItemSlot;

        uiItemSlot.Link(this);
    }

    public  void LinkUISlot(UIItemSlot uiSlot)
    {
        uiItemSlot = uiSlot;
    }

    public void UnLinkUISlot()
    {
        uiItemSlot = null;
    }


    public void EmptySlot()
    {
        stack = null;
        if(uiItemSlot != null)
        {
            uiItemSlot.UpdateSlot();
        }
    }

    public int Take(int amt)
    {
        if(amt > stack.amount)
        {
            int _amt = stack.amount;
            EmptySlot();

            return _amt;
        }
        else if (amt < stack.amount)
        {
            stack.amount -= amt;
            uiItemSlot.UpdateSlot();
            return amt;
        }
        else
        {
            EmptySlot();
            return amt;
        }
    }

    public ItemStack TakeAll()
    {
        ItemStack handOver = new ItemStack(stack.id, stack.amount);
        EmptySlot();
        return handOver;
    }

    public void InsertStack(ItemStack _stack)
    {
        stack = _stack;
        uiItemSlot.UpdateSlot();
    }

    public bool HasItem
    {
        get
        {
            if(stack != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
