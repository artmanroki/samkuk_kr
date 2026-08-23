using UnityEngine;

public class WorldCollider : MonoBehaviour
{
    Rigidbody rb;
    public float speed = 3f;
    public Vector3 targetPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector3 nextPos = Vector3.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);
    }
}
