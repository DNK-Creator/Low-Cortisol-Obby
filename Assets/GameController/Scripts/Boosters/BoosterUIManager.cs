using Invector.vCamera;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoosterUIManager : MonoBehaviour
{
    public static BoosterUIManager Instance;

    [SerializeField] private GameObject Crosshair;
    [SerializeField] private Button ShootButton;
    [SerializeField] private vThirdPersonCamera Camera;
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShootButton.onClick.AddListener(() => MMGameEvent.Trigger("Shoot"));
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            MMGameEvent.Trigger("Shoot");
    }
    public void ShootingBoosterSelected()
    {
        StartCoroutine(showCrosshairWithDelay());
    }

    public IEnumerator showCrosshairWithDelay()
    {
        yield return null;

        Camera.ChangeState("Aiming");
        Crosshair.SetActive(true);
        ShootButton.gameObject.SetActive(true);
    }

    public void ShootingBoosterDeselected()
    {

        Camera.ChangeState("Default");
        Crosshair.SetActive(false);
        ShootButton.gameObject.SetActive(false);
    }
}
