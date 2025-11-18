using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemObject : MonoBehaviour
{
    [SerializeField] private string itemName;
    [SerializeField] private string description;
    [SerializeField] private Image icon;
    [SerializeField] private int quantity;
    [SerializeField] private AudioClip pickupSound;

    private AudioSource audSrc;
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D pCollider2D;

    private void Start()
    {
        audSrc = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pCollider2D = GetComponent<PolygonCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController ctrl = collision.gameObject.GetComponent<PlayerController>();

        if (ctrl != null && ctrl.IsOwner)
        {
            pCollider2D.enabled = false;
            spriteRenderer.enabled = false;

            GameManager.instance.playerData.inventorySystem.Set(itemName, quantity);

            StartCoroutine(Pickup());
        }
    }

    private IEnumerator Pickup()
    {
        if (audSrc != null && pickupSound != null)
        {
            audSrc.PlayOneShot(pickupSound);
            yield return new WaitForSeconds(pickupSound.length);

            Destroy(this.gameObject); //may swap out for placing back into object pool
        }
    }
}
