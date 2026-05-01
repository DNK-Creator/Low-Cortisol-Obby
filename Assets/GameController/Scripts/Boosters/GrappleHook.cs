using UnityEngine;
using DG.Tweening;
using Invector.vCharacterController;

/// <summary>
/// Grapple hook that pulls the player to a target point and renders a rope.
/// </summary>
public class GrappleHook : ShootingBooster
{
    [Tooltip("LineRenderer component used to render the grappling rope.")]
    public LineRenderer lineRenderer;

    [Tooltip("Transform of the gun's muzzle where the rope starts.")]
    public Transform muzzlePoint;

    [Tooltip("Transform of the player to move.")]
    public Transform player;

    [Tooltip("Duration of the pull animation.")]
    public float grappleDuration = 0.5f;

    [Tooltip("Max distance the grapple hook can reach.")]
    public float maxDistance = 50f;

    [Tooltip("LayerMask to determine grappleable surfaces.")]
    public LayerMask grappleSurfaceMask;

    public vThirdPersonController vThirdPersonController;
    public vThirdPersonInput vThirdPersonInput;

    // Target point where the grapple hits
    private Vector3 _grapplePoint;

    // Whether currently grappling
    private bool _isGrappling = false;

    private Tween _grappleTween;

    protected override void Update()
    {
        base.Update();
        // Update rope positions while grappling
        if (_isGrappling)
        {
            UpdateRope();
        }

        // Cancel grapple on jump
        if (_isGrappling && Input.GetKeyDown(KeyCode.Space))
        {
            StopGrapple();
        }
    }

    public override void Shoot()
    {
        base.Shoot();

        // Raycast from center of screen
        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        //Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, grappleSurfaceMask))
        {
            _grapplePoint = hit.point;
            StartGrapple();
        }
    }

    /// <summary>
    /// Starts the grapple by moving the player and showing the rope.
    /// </summary>
    private void StartGrapple()
    {
        if (_isGrappling) return;

        _isGrappling = true;
        DisableController();

        // Start rope rendering
        lineRenderer.gameObject.SetActive(true);
        lineRenderer.positionCount = 2;

        // Move the player using DOTween
        _grappleTween = player.DOMove(_grapplePoint, grappleDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(EnableController);

        UpdateRope();
    }

    /// <summary>
    /// Stops the grappling process prematurely.
    /// </summary>
    private void StopGrapple()
    {
        if (!_isGrappling) return;

        _isGrappling = false;

        _grappleTween?.Kill();
        EnableController();

        // Hide rope
        lineRenderer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the rope positions every frame.
    /// </summary>
    private void UpdateRope()
    {
        lineRenderer.SetPosition(0, muzzlePoint.position);
        lineRenderer.SetPosition(1, _grapplePoint);
    }

    public void DisableController()
    {
        vThirdPersonInput.LockInputExceptCamera(true);
        vThirdPersonController._rigidbody.isKinematic = true;
        // TODO: Implement disabling character controller and player input
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        StopGrapple();
    }

    public void EnableController()
    {
        _isGrappling = false;

        // Hide rope
        lineRenderer.gameObject.SetActive(false);

        // TODO: Re-enable character controller and input
        vThirdPersonInput.LockInputExceptCamera(false);
        vThirdPersonController._rigidbody.isKinematic = false;
    }
}
