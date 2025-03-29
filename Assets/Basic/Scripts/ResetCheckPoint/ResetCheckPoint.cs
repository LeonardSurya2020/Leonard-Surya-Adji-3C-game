using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCheckPoint : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().ResetPositionToCheckPoint();
        }
    }
}
