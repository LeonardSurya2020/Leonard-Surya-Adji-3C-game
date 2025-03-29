using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCheckPoint : MonoBehaviour
{
    #region Parameters
    [SerializeField]
    private BoxCollider _checkpointBoxCollider;
    #endregion

    #region Main Functions
    private void Awake()
    {
        _checkpointBoxCollider = GetComponent<BoxCollider>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerMovement>().SetCheckPoint(this.transform);
            _checkpointBoxCollider.enabled = false;
        }
    }
    #endregion
}
