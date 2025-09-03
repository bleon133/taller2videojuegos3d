using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CharacterController))]
public class RocketBooster : MonoBehaviour
{
    [Header("Bindings")]
    public Transform cam;                         // Shoulder camera transform
    public KeyCode boostKey = KeyCode.Mouse1;     // Right mouse to boost
    public RectTransform crosshairRect;           // REQUIRED: UI crosshair

    [Header("Rules")]
    public bool requireAirToBoost = true;         // Only boost while airborne
    public bool singleBoostPerAir = true;         // One boost until grounded
    public bool faceBoostDirection = true;        // Rotate player to face boost dir (horizontal)

    [Header("Aiming")]
    [Tooltip("Layers considered aimable (exclude Player).")]
    public LayerMask aimMask = ~0;
    public float aimMaxDistance = 500f;

    [Header("Boost Physics")]
    [Tooltip("Max boost speed (units/s).")]
    public float boostMaxSpeed = 40f;
    [Tooltip("Thrust duration (seconds) for reduced gravity window.")]
    public float boostDuration = 0.35f;
    [Tooltip("Linear drag when not thrusting (units/s^2).")]
    public float boostDrag = 10f;
    [Tooltip("Gravity scale during thrust (0 = off, 1 = normal).")]
    [Range(0f, 1f)] public float gravityWhileBoosting = 0.35f;

    // Public API
    public Vector3 CurrentBoostVelocity => boostVelocity;
    public float CurrentGravityScale => (boostTimeLeft > 0f ? gravityWhileBoosting : 1f);

    // Internals
    private CharacterController cc;
    private Transform self;
    private Camera camComp;
    private Vector3 boostVelocity;
    private Vector3 currentBoostDir;
    private float boostTimeLeft;
    private bool boostConsumedThisAir;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        self = transform;

        if (cam == null && Camera.main != null) cam = Camera.main.transform;
        if (cam != null) camComp = cam.GetComponent<Camera>();
        if (camComp == null && Camera.main != null) camComp = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(boostKey))
        {
            bool canBoost = (!requireAirToBoost || !cc.isGrounded) &&
                            (!singleBoostPerAir || !boostConsumedThisAir);
            if (canBoost) BeginBoost();
        }

        bool thrusting = boostTimeLeft > 0f;

        // No acceleration phase; we start at max speed.
        if (!thrusting)
        {
            // Drag taper after thrust window
            float mag = boostVelocity.magnitude;
            if (mag > 0f)
            {
                float newMag = Mathf.Max(0f, mag - boostDrag * Time.deltaTime);
                boostVelocity *= (newMag / Mathf.Max(mag, 0.0001f));
            }
        }
        else
        {
            boostTimeLeft -= Time.deltaTime;
        }
    }

    /// Call from PlayerController to reset per-air boost when grounded.
    public void NotifyGrounded(bool grounded)
    {
        if (grounded) boostConsumedThisAir = false;
    }

    private void BeginBoost()
    {
        if (camComp == null || crosshairRect == null)
        {
            Debug.LogWarning("[RocketBooster] cam/crosshair missing. Assign both for crosshair aiming.");
            return;
        }

        // 1) Ray from crosshair
        Ray ray = GetRayFromCrosshairCenter();

        // 2) Aim point along ray
        Vector3 aimPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, aimMaxDistance, aimMask, QueryTriggerInteraction.Ignore))
            aimPoint = hit.point;
        else
            aimPoint = ray.origin + ray.direction * aimMaxDistance;

        // 3) Direction from PLAYER chest to aim point
        Vector3 origin = self.position + Vector3.up * (cc != null ? cc.height * 0.5f : 1.0f);
        Vector3 dir = (aimPoint - origin).normalized;

        // --- 4) Boost vertical lift (double y) ---
        Vector3 adjustedDir = new Vector3(dir.x, dir.y * 2f, dir.z).normalized;

        currentBoostDir = adjustedDir;
        boostTimeLeft = boostDuration;

        // --- 5) Start at max speed instantly ---
        boostVelocity = currentBoostDir * boostMaxSpeed;

        if (singleBoostPerAir) boostConsumedThisAir = true;

        if (faceBoostDirection)
        {
            Vector3 flat = new Vector3(adjustedDir.x, 0f, adjustedDir.z);
            if (flat.sqrMagnitude > 0.0001f)
                self.rotation = Quaternion.LookRotation(flat, Vector3.up);
        }
    }

    private Ray GetRayFromCrosshairCenter()
    {
        // Compute the world position of the rect's visual center,
        // then convert to screen point and build a ray.
        // Using TransformPoint on rect.center avoids pivot/scale issues.
        Vector3 worldCenter = crosshairRect.TransformPoint(crosshairRect.rect.center);

        Vector2 screenPoint;
        var canvas = crosshairRect.GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // In Overlay, rect world position is already in screen pixels
            screenPoint = (Vector2)worldCenter;
        }
        else
        {
            // ScreenSpace-Camera or World Space canvas
            screenPoint = RectTransformUtility.WorldToScreenPoint(camComp, worldCenter);
        }

        return camComp.ScreenPointToRay(screenPoint);
    }
}
