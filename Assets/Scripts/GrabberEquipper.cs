using UnityEngine;

public class GrabberEquipper : MonoBehaviour
{
    [SerializeField] private TrashGrabber grabberPrefab;
    [SerializeField] private Transform handSocket;
    [SerializeField] private KeyCode toggleKey = KeyCode.Alpha1;

    private TrashGrabber spawnedGrabber;
    private bool equipped;

    private void Start()
    {
        if (grabberPrefab != null && handSocket != null)
        {
            spawnedGrabber = Instantiate(grabberPrefab, handSocket);
            spawnedGrabber.transform.localPosition = Vector3.zero;
            spawnedGrabber.transform.localRotation = Quaternion.identity;
            spawnedGrabber.SetEquipped(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && spawnedGrabber != null)
        {
            equipped = !equipped;
            spawnedGrabber.SetEquipped(equipped);
        }
    }
}