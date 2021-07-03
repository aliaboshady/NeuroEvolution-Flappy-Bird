using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    [HideInInspector]
    public float fitness = 0;
    public float jumpAmount = 50;

    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public NeuralNetwork nn;

    [HideInInspector]
    public Matrix nnInput;
    [HideInInspector]
    public Matrix nnOutput;
    Pipe[] foundPipes;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        nn = new NeuralNetwork(GameSpawner.inputNodes, GameSpawner.hiddenNodes, GameSpawner.outputNodes);
        nnInput = new Matrix(GameSpawner.inputNodes, 1);
        nnOutput = new Matrix(GameSpawner.outputNodes, 1);
    }

    private void Start()
    {
        InvokeRepeating("IncreaseFitness", 0, 0.25f);
    }

    void Update()
    {
        foundPipes = FindObjectsOfType<Pipe>();
        Pipe closestPipe = ClosestPipe(foundPipes);

        double pipeTop, pipeBottom, pipeX;

        pipeTop = closestPipe.transform.position.y + 0.25;
        pipeBottom = closestPipe.transform.position.y - 0.25;
        pipeX = closestPipe.transform.position.x;

        nnInput.matrix[0, 0] = transform.position.y;
        nnInput.matrix[1, 0] = (float)pipeTop;
        nnInput.matrix[2, 0] = (float)pipeBottom;
        nnInput.matrix[3, 0] = (float)pipeX;
        nnInput.matrix[4, 0] = rb.velocity.y;

        float result = nn.FeedForward(nnInput).matrix[0, 0];

        if (result > 0.5)
        {
            Jump();
        }
        // if (Input.GetKeyDown(KeyCode.Space))
        // {
        //     Jump();
        // }

        if (transform.position.y > 1.5 || transform.position.y < -0.85)
        {
            DestroyThis();
        }
    }

    private void IncreaseFitness()
    {
        fitness++;
    }

    private Pipe ClosestPipe(Pipe[] pipes)
    {
        Pipe closestPipe = null;
        double closestDistance = 100000000;
        for (int i = 0; i < pipes.Length; i++)
        {
            double distance = pipes[i].transform.position.x + 0.2 - transform.position.x;
            if (distance < closestDistance && distance > 0)
            {
                closestDistance = distance;
                closestPipe = pipes[i];
            }
        }
        return closestPipe;
    }

    private void Jump()
    {
        rb.velocity = Vector2.up * jumpAmount;
    }

    private void DestroyThis()
    {
        CancelInvoke("IncreaseFitness");

        if (GameSpawner.prevGenBirdsListIndex == GameSpawner.birdCountStatic)
        {
            Destroy(gameObject);
            return;
        }

        //Fitness
        GameSpawner.prevGenBirds[GameSpawner.prevGenBirdsListIndex].fitness = fitness;

        //Weights_IH
        for (int i = 0; i < GameSpawner.hiddenNodes; i++)
        {
            for (int j = 0; j < GameSpawner.inputNodes; j++)
            {
                GameSpawner.prevGenBirds[GameSpawner.prevGenBirdsListIndex].weights_IH.matrix[i, j] = nn.weights_IH.matrix[i, j];
            }
        }

        //Weights_HO
        for (int i = 0; i < GameSpawner.outputNodes; i++)
        {
            for (int j = 0; j < GameSpawner.hiddenNodes; j++)
            {
                GameSpawner.prevGenBirds[GameSpawner.prevGenBirdsListIndex].weights_HO.matrix[i, j] = nn.weights_HO.matrix[i, j];
            }
        }

        //Bias_H
        for (int i = 0; i < GameSpawner.hiddenNodes; i++)
        {

            GameSpawner.prevGenBirds[GameSpawner.prevGenBirdsListIndex].bias_H.matrix[i, 0] = nn.bias_H.matrix[i, 0];

        }

        //Bias_O
        for (int i = 0; i < GameSpawner.outputNodes; i++)
        {

            GameSpawner.prevGenBirds[GameSpawner.prevGenBirdsListIndex].bias_O.matrix[i, 0] = nn.bias_O.matrix[i, 0];

        }

        GameSpawner.prevGenBirdsListIndex++;

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Floor" || other.tag == "Ceiling" || other.tag == "Lower Pipe" || other.tag == "Upper Pipe")
        {
            DestroyThis();
        }
    }

}
