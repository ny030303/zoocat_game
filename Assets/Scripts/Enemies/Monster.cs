using UnityEngine;
using System.Collections.Generic;

public class Monster : MonoBehaviour
{
    public float speed = 1f;
    public int health = 10;
    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;
    private int roundMultiplier;
    private Animator animator;

    public void Initialize(List<Transform> waypoints, int round)
    {
        this.waypoints = waypoints;
        roundMultiplier = round;
        health += round * 10; // Increase health based on round
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
    }

    void Move()
    {
        if (waypoints == null || waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
        {
            Destroy(gameObject); // Destroy if no waypoints or reached the end
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = target.position - transform.position;
        float step = speed * Time.deltaTime;

        // Update speed parameter for the Animator
        animator.SetFloat("Speed", speed);

        if (direction.magnitude <= step)
        {
            transform.position = target.position;
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
            {
                FindObjectOfType<GameManager>().TakeDamage(true, 1); // Reduce player health
                Destroy(gameObject);
                //Die();
            }
        }
        else
        {
            transform.position += direction.normalized * step;
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Update isDead parameter for the Animator
        animator.SetBool("IsDead", true);

        //FindObjectOfType<GameManager>().AddGold(10); // Reward gold
        Destroy(gameObject, 3); // Destroy after 1 second to allow death animation to play
    }
}
