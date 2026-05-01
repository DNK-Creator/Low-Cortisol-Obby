using Invector.vCamera;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchCameraView : MonoBehaviour
{
    private bool _isThirdPersonView = true;
    [SerializeField] private GameObject _headGO;
    [SerializeField] private vThirdPersonCamera _vThirdPersonCamera;
    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (_isThirdPersonView)
            {
                SwitchToFPV();
            }
            else
            {
                SwitchToTPV();
            }
        }
    }

    private void SwitchToFPV()
    {
        _isThirdPersonView = false;
        _headGO.SetActive(false);
        _vThirdPersonCamera.ChangeState("FPV");
    }

    private void SwitchToTPV()
    {
        _isThirdPersonView = true;
        _headGO.SetActive(true);
        _vThirdPersonCamera.ChangeState("TPV");
    }
}
