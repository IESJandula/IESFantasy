using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("EnemyHP parameters")]
    public int hp = 5;
    public int maxHp = 5;

    protected bool invincible;
    protected float invincibilityTime = 0.6f;
    protected float blinkTime = 0.1f;

    public float knockbackStrength = 2f;
    protected float knockbackTime = 0.3f;

    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rigidBody;
    
    protected Vector3 initialPosition;

    public virtual void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hp = maxHp;

        initialPosition = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Weapon")
        {
            hp--;
            if (hp <= 0)
            {
                Destroy(gameObject);
            }
            StopBehaviour();
            StartCoroutine(Invincibility());
            StartCoroutine(Knockback(collision.transform.position));
        }
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
        if (knockbackStrength <= 0)
        {
            if (hp > 0) ContinueBehaviour();
            yield break;
        }

        rigidBody.velocity = (transform.position - hitPosition).normalized * knockbackStrength;
        yield return new WaitForSeconds(knockbackTime);
        rigidBody.velocity = Vector3.zero;
        yield return new WaitForSeconds(knockbackTime);
        if (hp > 0) ContinueBehaviour();
    }
    public virtual void StopBehaviour() { }

    public virtual void ContinueBehaviour() { }

    public virtual void ResetPosition()
    {
        transform.position = initialPosition;
        hp = maxHp;
    }
}
