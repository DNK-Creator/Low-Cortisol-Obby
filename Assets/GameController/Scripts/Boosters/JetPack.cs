using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetPack : Booster
{
    public GameObject Fire;
    public float VerticalSpeed;
    public Rigidbody rigidbody;
    public ForceMode forceMode;

    private bool _isActive = false;

    private void Update()
    {
        if (!isSelected)
            return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartJetpack();
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            StopJetpack();
        }

        if (_isActive)
        {
            ApplyJetpackVerticalSpeed();
        }
    }

    /// <summary>
    /// Starts the jetpack effect and lifting.
    /// </summary>
    public void StartJetpack()
    {
        _isActive = true;
        Fire.SetActive(true);
    }

    /// <summary>
    /// Stops the jetpack effect.
    /// </summary>
    public void StopJetpack()
    {
        _isActive = false;
        Fire.SetActive(false);
    }

    /// <summary>
    /// Directly sets the upward velocity to maintain constant lift speed.
    /// </summary>
    private void ApplyJetpackVerticalSpeed()
    {
        Vector3 velocity = rigidbody.linearVelocity;
        velocity.y = VerticalSpeed;
        rigidbody.linearVelocity = velocity;
    }
}
