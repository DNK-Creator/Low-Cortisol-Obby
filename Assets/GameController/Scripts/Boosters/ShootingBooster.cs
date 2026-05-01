using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShootingBooster : Booster, MMEventListener<MMGameEvent>
{
    public UnityEvent OnShoot;

    [HideInInspector] public Camera mainCamera;

    public void Start()
    {
        mainCamera = Camera.main;
    }

    public override void OnActivate()
    {
        base.OnActivate();
        BoosterUIManager.Instance.ShootingBoosterSelected();
        this.MMEventStartListening();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        BoosterUIManager.Instance.ShootingBoosterDeselected();
        this.MMEventStopListening();
    }

    protected virtual void Update()
    {
        if (isSelected && Input.GetKeyDown(KeyCode.R))
        {
            Shoot();
        }
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "Shoot" && isSelected)
        {
            Shoot();
        }
    }

    public virtual void Shoot()
    {
        OnShoot.Invoke();
    }
}
