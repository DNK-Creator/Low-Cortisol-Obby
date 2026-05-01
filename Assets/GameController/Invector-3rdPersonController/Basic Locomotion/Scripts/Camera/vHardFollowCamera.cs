using UnityEngine;
using Invector.vCamera;
using Invector;

/// <summary>
/// Hard-follow camera that overrides smoothing behavior of vThirdPersonCamera.
/// </summary>
public class vHardFollowCamera : vThirdPersonCamera
{
    /// <summary>
    /// Overrides default camera movement with immediate, non-smoothed behavior.
    /// </summary>
    /// <param name="forceUpdate">If true, forces camera update regardless of first state flag.</param>
    public override void CameraMovement(bool forceUpdate = false)
    {
        if (currentTarget == null || targetCamera == null || (!firstStateIsInit && !forceUpdate))
            return;

        // Instant state copy without smooth transition
        currentState.CopyState(lerpState);

        // Instant camera settings
        currentZoom = currentState.useZoom
            ? Mathf.Clamp(currentZoom, currentState.minDistance, currentState.maxDistance)
            : currentState.defaultDistance;
        distance = currentZoom;
        targetCamera.fieldOfView = currentState.fov;

        // Calculate direction and position
        Vector3 pivotOffset = currentTarget.up * offSetPlayerPivot;
        Vector3 cameraTargetPosition = currentTarget.position + pivotOffset + currentTarget.up * currentState.height;
        Vector3 cameraDirection = (currentState.forward * currentTarget.forward) +
                                  (currentState.right * switchRight * currentTarget.right);
        cameraDirection.Normalize();

        Vector3 finalCameraPosition = cameraTargetPosition + (cameraDirection * distance);

        // Update camera transform directly
        transform.position = finalCameraPosition;

        float finalMouseY = mouseYStart + offsetMouse.y;
        float finalMouseX = mouseXStart + offsetMouse.x;

        // Compute rotation instantly
        Quaternion rotation = Quaternion.Euler(finalMouseY, finalMouseX, 0f);
        transform.rotation = rotation;

        // Set the look-at target transform instantly
        if (_lookAtTarget != null)
        {
            _lookAtTarget.position = cameraTargetPosition;
            _lookAtTarget.rotation = rotation;
        }
    }

    /// <summary>
    /// Disables Rigidbody motion on awake to avoid smoothing.
    /// </summary>
    protected override void Start()
    {
        base.Start();

        // If Rigidbody is used in base, disable it to prevent physics interpolation
        if (_selfRigidbody != null)
        {
            Destroy(_selfRigidbody);
            _selfRigidbody = null;
        }

        // Disable smoothing
        useSmooth = false;
    }

    /// <summary>
    /// Prevents base class from modifying transform using physics.
    /// </summary>
    public override void FixedUpdate()
    {
        if (!isInit || isFreezed || currentTarget == null || currentState == null || lerpState == null)
            return;

        // Only use our instant movement logic
        if (currentState.cameraMode == TPCameraMode.FreeDirectional || currentState.cameraMode == TPCameraMode.FixedAngle)
        {
            CameraMovement(forceUpdate: true);
        }
        else
        {
            base.FixedUpdate();
        }
    }
}