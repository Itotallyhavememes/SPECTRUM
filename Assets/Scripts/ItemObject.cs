using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ItemObject : MonoBehaviour
{
    [SerializeField] protected string itemName;
    [SerializeField] protected string description;
    [SerializeField] protected Image icon;
    [SerializeField] protected int quantity;
    [SerializeField] protected AudioClip pickupSound;

    protected AudioSource audSrc;
    protected SpriteRenderer spriteRenderer;
    protected PolygonCollider2D pCollider2D;

    public string GetName() => itemName;
    public string GetDesc() => description;
    public Image GetImg() => icon;
    public int GetQuantity() => quantity;

    protected virtual void Start()
    {
        audSrc = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pCollider2D = GetComponent<PolygonCollider2D>();
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController ctrl = collision.gameObject.GetComponent<PlayerController>();

        if (ctrl != null)
        {
            pCollider2D.enabled = false;
            spriteRenderer.enabled = false;

            if (ctrl.IsOwner)
                StartCoroutine(Pickup());

        }
    }

    /// <summary>
    /// Called when a valid player picks up an item.
    /// </summary>
    /// <returns></returns>
    protected virtual IEnumerator Pickup()
    {
        GameManager.instance.playerData.inventorySystem.Set(itemName, quantity);

        if (audSrc != null && pickupSound != null)
        {
            audSrc.PlayOneShot(pickupSound);
            yield return new WaitForSeconds(pickupSound.length);

            Destroy(this.gameObject); //may swap out for placing back into object pool
        }
    }
}
