using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public SpriteRenderer pickItem;
    public float speed = 5f;
    Vector2 direction;
    Rigidbody2D rigidBody;
    Animator animator;
    SpriteRenderer spriteRenderer;

    bool isAttacking;
    bool invincible;

    float invincibilityTime = 1.2f;
    float blinkTime = 0.1f;

    bool uncontrollable;
    public float knockbackStrength = 10f;
    float knockbackTime = 0.4f;

    GameManager gameManager;

    BasicInteraction basicInteraction;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = FindObjectOfType<GameManager>();

        // Posicionar al jugador en las coordenadas guardadas o en su posición predeterminada
        if (DataInstance.Instance.HasSavedPosition())
        {
            transform.position = DataInstance.Instance.GetPlayerPosition();
        }
        else
        {
            // La posición predeterminada del jugador en la escena
            transform.position = transform.position;
        }
    }

    private void FixedUpdate()
    {
        if (!uncontrollable)
        {
            rigidBody.velocity = direction * speed;
        }
    }

    void Update()
    {
        Inputs();
        Animations();
    }

    private void Inputs()
    {
        if (isAttacking || uncontrollable) return;

        direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (basicInteraction != null)
            {
                Vector2 playerFacing = new Vector2(animator.GetFloat("Horizontal"), animator.GetFloat("Vertical"));
                if (!basicInteraction.Interact(playerFacing, transform.position))
                {
                    Attack();
                }
            }
            else
            {
                Attack();
            }
        }
    }

    private void Attack()
    {
        animator.Play("Attack");
        isAttacking = true;
        direction = Vector2.zero;
        AttackAnimDirection();
    }

    private void Animations()
    {
        if (isAttacking || uncontrollable || Time.timeScale == 0) return;

        if (direction.magnitude != 0)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
            animator.Play("Run");
        }
        else
        {
            animator.Play("Idle");
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    private void AttackAnimDirection()
    {
        direction.x = animator.GetFloat("Horizontal");
        direction.y = animator.GetFloat("Vertical");

        if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
        {
            direction.x = 0;
        }
        else
        {
            direction.y = 0;
        }
        direction = direction.normalized;

        animator.SetFloat("Horizontal", direction.x);
        animator.SetFloat("Vertical", direction.y);

        direction = Vector2.zero;
    }

    IEnumerator Invincibility()
    {
        invincible = true;
        float auxTime = invincibilityTime;

        while (auxTime > 0)
        {
            yield return new WaitForSeconds(blinkTime);
            auxTime -= blinkTime;
            spriteRenderer.enabled = !spriteRenderer.enabled;
        }

        spriteRenderer.enabled = true;
        invincible = false;
    }

    IEnumerator Knockback(Vector3 hitPosition)
    {
        uncontrollable = true;
        direction = Vector2.zero;
        rigidBody.velocity = (transform.position - hitPosition).normalized * knockbackStrength;
        yield return new WaitForSeconds(knockbackTime);
        rigidBody.velocity = Vector3.zero;
        uncontrollable = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("MaxHpUp"))
        {
            Destroy(collision.gameObject);
            gameManager.IncreaseMaxHP();
        }
        else if (collision.CompareTag("Heal") && gameManager.CanHeal())
        {
            Destroy(collision.gameObject);
            gameManager.UpdateCurrentHP(4);
        }
        else if (collision.CompareTag("Interaction"))
        {
            basicInteraction = collision.GetComponent<BasicInteraction>();
        }
        else if (collision.CompareTag("Key"))
        {
            Destroy(collision.gameObject);
            gameManager.UpdateCurrentKeys(1);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Interaction"))
        {
            basicInteraction = null;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            if (!invincible)
            {
                gameManager.UpdateCurrentHP(-2);
                if (gameManager.hp <= 0)
                {
                    RestartGame();
                }
                StartCoroutine(Invincibility());
                StartCoroutine(Knockback(collision.transform.position));
            }
        }
    }

    IEnumerator PickItem()
    {
        animator.Play("Pick_Item");
        uncontrollable = true;
        direction = rigidBody.velocity = Vector2.zero;
        Camera.main.GetComponent<CameraController>().PauseEnemies();

        yield return new WaitForSeconds(1f);

        uncontrollable = false;
        Camera.main.GetComponent<CameraController>().ResumeEnemies();
    }

    private void RestartGame()
    {
        DataInstance.Instance.SetPlayerPosition(gameManager.playerStartCoordinates);
        SceneManager.LoadScene(0);
    }
}
