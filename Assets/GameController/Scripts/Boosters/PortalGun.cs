using UnityEngine;

/// <summary>
/// Portal gun that alternates between spawning Portal A and Portal B at raycast hit positions.
/// </summary>
public class PortalGun : ShootingBooster
{
    [Tooltip("Prefab or instance of the first portal.")]
    public GameObject PortalA;

    [Tooltip("Prefab or instance of the second portal.")]
    public GameObject PortalB;

    [Tooltip("Maximum distance the portal gun can shoot.")]
    public float MaxDistance = 100f;

    [Tooltip("LayerMask to determine what surfaces the portal can be placed on.")]
    public LayerMask PortalSurfaceMask;

    // Tracks whether the next portal to place is A or B
    private bool _placePortalA = true;

    public override void Shoot()
    {
        
        base.Shoot();

        // Raycast from the center of the screen
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        //Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, MaxDistance, PortalSurfaceMask))
        {
            PlacePortal(hitInfo);
        }
    }

    /// <summary>
    /// Places the appropriate portal at the hit location.
    /// </summary>
    /// <param name="hitInfo">Raycast hit information.</param>
    private void PlacePortal(RaycastHit hitInfo)
    {
        GameObject selectedPortal = _placePortalA ? PortalA : PortalB;

        // Move and rotate portal to hit position
        selectedPortal.transform.position = hitInfo.point;
        selectedPortal.transform.rotation = Quaternion.LookRotation(hitInfo.normal);

        // Ensure portal is active
        if (!selectedPortal.activeInHierarchy)
            selectedPortal.SetActive(true);

        // Alternate portal type for next shot
        _placePortalA = !_placePortalA;
    }
}