using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Pipe : MonoBehaviour
{
    bool entered = false;
    public float speed = 3;

    void Start()
    {
        transform.position = transform.position + (Vector3.up * Random.Range(-0.4f, 1));
    }

    void Update()
    {
        transform.position = transform.position + (Vector3.left * speed * Time.deltaTime);

        if (transform.position.x < -2.5f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!entered)
        {
            Score.IncreaseCurrentScore();
            entered = true;


        }

        if (other.tag == "Bird")
        {
            Bird bird = other.gameObject.GetComponent<Bird>();
            bird.fitness += 50;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        entered = false;
    }
}
