using UnityEngine;

public class SimplePatrol : MonoBehaviour
{
    public Transform[] points;
    public float speed = 2f;
    public float waitTime = 1f;

    private Animator animator;
    private int currentPoint = 0;
    private float waitTimer = 0f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (points.Length == 0) return;

        Transform target = points[currentPoint];

        // Flytta gubben rakt mot punkten (behåll samma höjd Y)
        Vector3 targetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance > 0.1f)
        {
            // Gå och vrid gubben mot punkten
            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
            transform.LookAt(targetPos);

            if (animator != null) animator.SetBool("isWalking", true);
        }
        else
        {
            // Vänta vid punkten
            if (animator != null) animator.SetBool("isWalking", false);

            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                currentPoint = (currentPoint + 1) % points.Length;
                waitTimer = 0f;
            }
        }
    }
}