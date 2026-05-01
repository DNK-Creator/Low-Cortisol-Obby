using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private Portal _exitPoint;
    [SerializeField] private float _exitOffset;

    public float Delay = 0f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && Delay <= 0)
        { 
            other.gameObject.SetActive(false);
            _exitPoint.Delay = 2f;
            other.transform.position = _exitPoint.transform.position + _exitPoint.transform.forward * _exitOffset;
            other.gameObject.SetActive(true);
        }
    }

    public void Update()
    {
        if (Delay > 0)
            Delay -= Time.deltaTime;
    }
}
