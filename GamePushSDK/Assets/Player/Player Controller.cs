using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDead) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            rb.velocity = Vector2.up * jumpForce;
        }
    }

    public void Die()
    {
        isDead = true;
        GameManager.Instance.GameOver();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Die();
    }
}
