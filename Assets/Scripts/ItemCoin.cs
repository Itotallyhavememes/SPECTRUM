using System.Collections;
using UnityEngine;

public class ItemCoin : ItemObject
{
    protected override void Start()
    {
        base.Start();

    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

    }

    protected override IEnumerator Pickup()
    {
        GameManager.instance?.onPickupCurrency.Invoke(quantity);

        return base.Pickup();
    }
}
