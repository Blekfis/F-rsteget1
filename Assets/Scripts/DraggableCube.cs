using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class DraggableCube : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private KeyCode dragKey = KeyCode.E;
    [SerializeField] private float maxGrabDistance = 5f;
    [SerializeField] private float minHoldDistance = 1.5f;
    [SerializeField] private float maxHoldDistance = 6f;
    [SerializeField] private float followSpeed = 18f;
    [SerializeField] private Camera playerCamera;

    private Rigidbody rb;
    private Collider col;
    private bool isDragging;
    private float currentHoldDistance;
    private bool previousUseGravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (!isDragging)
        {
            if (Input.GetKeyDown(dragKey))
            {
                TryStartDrag();
            }
        }
        else
        {
            if (!Input.GetKey(dragKey))
            {
                StopDrag();
                return;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                currentHoldDistance = Mathf.Clamp(
                    currentHoldDistance + scroll * 3f,
                    minHoldDistance,
                    maxHoldDistance
                );
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isDragging || playerCamera == null) return;

        Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * currentHoldDistance;
        Vector3 nextPos = Vector3.Lerp(rb.position, targetPos, Time.fixedDeltaTime * followSpeed);
        rb.MovePosition(nextPos);
    }

    private void TryStartDrag()
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == col || hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                isDragging = true;
                currentHoldDistance = Mathf.Clamp(hit.distance, minHoldDistance, maxHoldDistance);

                previousUseGravity = rb.useGravity;
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = true;
                col.enabled = false;
            }
        }
    }

    private void StopDrag()
    {
        isDragging = false;
        col.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = previousUseGravity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}