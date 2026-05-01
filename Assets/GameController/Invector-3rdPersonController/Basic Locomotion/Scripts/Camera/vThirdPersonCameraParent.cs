using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vThirdPersonCameraParent : MonoBehaviour
{
    public Transform Player;
    public UpdateType updateType;

    public void Update()
    {
        if(updateType == UpdateType.Normal)
            transform.position = Player.position;
    }

    public void FixedUpdate()
    {
        if (updateType == UpdateType.Fixed)
            transform.position = Player.position;
    }
    public void LateUpdate()
    {
        if (updateType == UpdateType.Late)
            transform.position = Player.position;
    }
}
