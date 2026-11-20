using UnityEngine;

public class TrashGrabber : MonoBehaviour
{
    [Header("Equip & Setup")]
    [SerializeField] private bool isEquipped = true;
    [SerializeField] private Transform upperJawPivot;
    [SerializeField] private Transform lowerJawPivot;
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Camera playerCamera;

    [Header("Grab Settings")]
    [SerializeField] private float grabRange = 3.5f;
    [SerializeField] private float holdFollowSpeed = 20f;
    [SerializeField] private float grabCooldown = 0.15f;
    [SerializeField] private string trashTag = "Trash";

    [Header("Jaw Angles (degrees)")]
    [SerializeField] private float openAngle = 0f;
    [SerializeField] private float upperClosedAngle = 25f;
    [SerializeField] private float lowerClosedAngle = -25f;
    [SerializeField] private float jawSpeed = 14f;

    private Rigidbody heldRb;
    private Transform heldTransform;
    private bool previousUseGravity = true;
    private float grabTime;
    private float nextGrabTime;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        SetEquipped(isEquipped);
    }

    private void Update()
    {
        if (!isEquipped) return;

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        bool isHoldingMouse = Input.GetMouseButton(0);

        float targetUpper = isHoldingMouse ? upperClosedAngle : openAngle;
        float targetLower = isHoldingMouse ? lowerClosedAngle : openAngle;

        if (upperJawPivot != null)
        {
            Quaternion targetUpperRot = Quaternion.Euler(targetUpper, 0f, 0f);
            upperJawPivot.localRotation = Quaternion.Slerp(
                upperJawPivot.localRotation,
                targetUpperRot,
                Time.deltaTime * jawSpeed
            );
        }

        if (lowerJawPivot != null)
        {
            Quaternion targetLowerRot = Quaternion.Euler(targetLower, 0f, 0f);
            lowerJawPivot.localRotation = Quaternion.Slerp(
                lowerJawPivot.localRotation,
                targetLowerRot,
                Time.deltaTime * jawSpeed
            );
        }

        if (Input.GetMouseButtonDown(0) && heldTransform == null && Time.time >= nextGrabTime)
        {
            TryGrabTrash();
            if (heldTransform != null)
            {
                grabTime = Time.time;
            }
        }
        else if (Input.GetMouseButtonUp(0) && heldTransform != null && Time.time >= grabTime + grabCooldown)
        {
            ReleaseTrash();
            nextGrabTime = Time.time + grabCooldown;
        }
    }

    private void FixedUpdate()
    {
        if (!isEquipped || heldTransform == null || holdPoint == null) return;

        heldTransform.position = Vector3.Lerp(
            heldTransform.position,
            holdPoint.position,
            Time.fixedDeltaTime * holdFollowSpeed
        );
        heldTransform.rotation = Quaternion.Slerp(
            heldTransform.rotation,
            holdPoint.rotation,
            Time.fixedDeltaTime * holdFollowSpeed
        );
    }

    private void TryGrabTrash()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, grabRange, ~0, QueryTriggerInteraction.Ignore))
        {
            bool hasTrashTag = hit.collider.CompareTag(trashTag) ||
                               hit.collider.transform.root.CompareTag(trashTag);
            if (!hasTrashTag) return;

            heldTransform = hit.collider.attachedRigidbody != null
                ? hit.collider.attachedRigidbody.transform
                : hit.transform;

            heldRb = heldTransform.GetComponent<Rigidbody>();
            if (heldRb != null)
            {
                previousUseGravity = heldRb.useGravity;

                if (!heldRb.isKinematic)
                {
                    heldRb.linearVelocity = Vector3.zero;
                    heldRb.angularVelocity = Vector3.zero;
                }

                heldRb.isKinematic = true;
            }
        }
    }

    private void ReleaseTrash()
    {
        if (heldRb != null)
        {
            heldRb.isKinematic = false;
            heldRb.useGravity = previousUseGravity;
        }

        heldRb = null;
        heldTransform = null;
    }

    public void SetEquipped(bool equipped)
    {
        isEquipped = equipped;

        if (!isEquipped && heldTransform != null)
        {
            ReleaseTrash();
        }

        gameObject.SetActive(isEquipped);
    }
}