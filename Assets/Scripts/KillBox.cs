using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    [SerializeField] bool kills;
    [SerializeField] Transform returnLocation;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision?.gameObject.GetComponent<PlayerController>())
        {
            if (!kills)
                collision.gameObject.transform.position = returnLocation.position;
            else
            {
                //TODO: kill player
            }
        }
    }
}
