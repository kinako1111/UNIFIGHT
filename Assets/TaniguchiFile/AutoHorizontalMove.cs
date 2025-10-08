using UnityEngine;

public class AutoHorizontalMove : MonoBehaviour
{
    [SerializeField] float moveSpeed = 3f;
    [SerializeField] float moveDistance = 5f;

    private Vector3 startPosition;
    private int direction = 1;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

        // ˆÚ“®‹——£‚ð’´‚¦‚½‚ç•ûŒü‚ð”½“]
        if (Mathf.Abs(transform.position.x - startPosition.x) >= moveDistance)
        {
            direction *= -1;
        }
    }
}