using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float xGap;
    [SerializeField]
    private float xRatio;
    [SerializeField]
    private float zRatio;
    [SerializeField]
    private float boardHeight;
    [SerializeField]
    private Vector2 moveLimitAt;
    [SerializeField]
    private Vector2 moveLimitTo;


    private Vector3 mousePosition;
    [HideInInspector]
    public Vector3 currentVelocity;
    private float mag;
    private bool isLeftClick;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        mousePosition = Input.mousePosition;
        mousePosition.z = 10.0f;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        mousePosition.y = boardHeight;
        mousePosition.x += xGap;
        mousePosition.x /= xRatio;
        mousePosition.z /= zRatio;
        mousePosition.x = Mathf.Clamp(mousePosition.x, moveLimitAt.x, moveLimitTo.x);
        mousePosition.z = Mathf.Clamp(mousePosition.z, moveLimitAt.y * GameManager.fieldScale.z, moveLimitTo.y * GameManager.fieldScale.z);
        isLeftClick = Input.GetMouseButton(0);

        /* MovePositionを使わない方法
        float sqrMag = (mousePosition - transform.position).sqrMagnitude;
        float distance = Vector3.Distance(mousePosition, transform.position) * speed;
        desiredVelocity = (mousePosition - transform.position).normalized * distance;
        // check this against the lastSqrMag
        // if this is greater than the last,
        // rigidbody has reached target and is now moving past it
        if (sqrMag > lastSqrMag)
        {
            // rigidbody has reached target and is now moving past it
            // stop the rigidbody by setting the velocity to zero
            desiredVelocity = Vector3.zero;
        }
        // make sure you update the lastSqrMag
        lastSqrMag = sqrMag;
		*/
    }

    void FixedUpdate()
    {
        if (isLeftClick)
        { }

        //マウスとの距離がある程度離れている場合速度を落とす
        currentVelocity = mousePosition - transform.position;
        mag = currentVelocity.magnitude;
        if (mag > maxSpeed)
        {
            currentVelocity *= maxSpeed / mag;
            rb.MovePosition(transform.position + currentVelocity);
        }
        else
            rb.MovePosition(mousePosition);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(mousePosition, 0.2f);
    }
}
