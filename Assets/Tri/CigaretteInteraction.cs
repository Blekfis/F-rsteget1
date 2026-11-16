using UnityEngine;
using System.Collections;

public class CigaretteInteraction : MonoBehaviour
{
    [Header("Referenser")]
    public Transform playerCamera;          
    public Transform holdPoint;               // Dra hit 'CigaretteHoldPoint' från kameran
    public Transform facePoint;               // Dra hit 'CigaretteFacePoint' från kameran
    public ParticleSystem smokeParticles;   
    public Renderer cigaretteRenderer;      
    public Renderer glowRenderer;           
    public TripEffect tripEffect;             // Koppla in ditt TripEffect-skript här
    
    [Header("Inställningar")]
    public float pickupRange = 3.0f;        
    public float moveDuration = 0.4f;       
    public float throwForce = 3.0f;           

    private Rigidbody rb;
    private Collider col;
    private bool isPickedUp = false;
    private bool isInteracting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        if (smokeParticles != null) smokeParticles.Stop();

        if (cigaretteRenderer == null) cigaretteRenderer = GetComponent<Renderer>();
        if (glowRenderer == null) glowRenderer = cigaretteRenderer; 

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        if (tripEffect == null)
        {
            tripEffect = GetComponent<TripEffect>();
        }

        SetGlow(false);
    }

    void Update()
    {
        if (playerCamera == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerCamera.position);

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isPickedUp && distanceToPlayer <= pickupRange)
            {
                PickUp();
            }
            else if (isPickedUp && !isInteracting)
            {
                StartCoroutine(TakeDragRoutine());
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && isPickedUp && !isInteracting)
        {
            ThrowAway();
        }
    }

    public void PickUp()
    {
        isPickedUp = true;

        // 1. Nollställ hastighet medan fysiken är aktiv
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 2. Gör den kinematisk
        rb.isKinematic = true;                      

        // 3. Stäng av kollideraren så den inte krockar med spelarkroppen
        if (col != null) col.enabled = false;

        Transform parentTarget = holdPoint != null ? holdPoint : playerCamera;
        transform.SetParent(parentTarget);          
        transform.localPosition = Vector3.zero;     
        transform.localRotation = Quaternion.identity;  
    }

    IEnumerator TakeDragRoutine()
    {
        isInteracting = true;

        if (facePoint != null)
        {
            yield return StartCoroutine(MoveToTransform(facePoint, moveDuration));
        }

        SetGlow(true);
        if (smokeParticles != null) smokeParticles.Play();

        yield return new WaitForSeconds(2.5f);

        SetGlow(false);
        if (smokeParticles != null) smokeParticles.Stop();

        if (holdPoint != null)
        {
            yield return StartCoroutine(MoveToTransform(holdPoint, moveDuration));
        }

        if (tripEffect != null)
        {
            tripEffect.StartTrip(120f);
        }

        isInteracting = false;
    }

    IEnumerator MoveToTransform(Transform target, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;
        Quaternion startRot = transform.localRotation;

        Vector3 targetLocalPos = target.localPosition;
        Quaternion targetLocalRot = target.localRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(startPos, targetLocalPos, smoothT);
            transform.localRotation = Quaternion.Slerp(startRot, targetLocalRot, smoothT);
            yield return null;
        }

        transform.localPosition = targetLocalPos;
        transform.localRotation = targetLocalRot;
    }

    void SetGlow(bool active)
    {
        Renderer targetRenderer = glowRenderer != null ? glowRenderer : cigaretteRenderer;
        if (targetRenderer != null)
        {
            Material mat = targetRenderer.material;
            if (active)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", Color.red * 6f); 
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
            }
        }
    }

    public void ThrowAway()
    {
        isPickedUp = false;
        transform.SetParent(null);              

        // 1. Flytta ut cigaretten framför kameran
        transform.position = playerCamera.position + playerCamera.forward * 0.9f;

        // 2. Slå på kollideraren igen
        if (col != null) col.enabled = true;

        // 3. Sätt på fysik och nollställ gamla krafter
        rb.isKinematic = false;                 
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 4. Kasta iväg snyggt och kontrollerat
        rb.AddForce(playerCamera.forward * throwForce + Vector3.up * 0.3f, ForceMode.Impulse);
    }
}