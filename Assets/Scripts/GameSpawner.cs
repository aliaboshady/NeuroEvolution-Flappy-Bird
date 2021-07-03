using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSpawner : MonoBehaviour
{
    public GameObject pipePrefab;
    public Bird birdObj;
    public float pipesDelay = 1.3f;
    public int birdCount = 150;
    public float mutationRate = 0.1f;
    [Range(1, 4.5f)]
    public float timeScale = 1;

    private Pipe[] foundPipes;
    private Bird[] foundBirds;

    private string bestBirdJsonPath;

    public static int inputNodes = 5;
    public static int hiddenNodes = 8;
    public static int outputNodes = 1;

    public static int birdCountStatic;

    public static int prevGenBirdsListIndex;
    public static DeadBirdHolder[] prevGenBirds;

    void Start()
    {
        prevGenBirdsListIndex = 0;
        prevGenBirds = new DeadBirdHolder[birdCount];

        birdCountStatic = birdCount;

        InvokeRepeating("SpawnPipe", 0, pipesDelay);
        SpawnBirds(birdObj, true);

        prevGenBirds = new DeadBirdHolder[birdCount];
        for (int i = 0; i < prevGenBirds.Length; i++)
        {
            prevGenBirds[i] = new DeadBirdHolder();

            prevGenBirds[i].weights_IH = new Matrix(hiddenNodes, inputNodes);
            prevGenBirds[i].weights_HO = new Matrix(outputNodes, hiddenNodes);

            prevGenBirds[i].bias_H = new Matrix(hiddenNodes, 1);
            prevGenBirds[i].bias_O = new Matrix(outputNodes, 1);
        }
    }

    private void Update()
    {
        Time.timeScale = timeScale;

        if (CountBirds() == 0)
        {
            NormalizeFitness();

            Score.IncreaseGeneration();
            CancelPipes();

            SpawnBirds(birdObj, false);
            InvokeRepeating("SpawnPipe", 0, pipesDelay);

            Score.currentScore = 0;
            Score.currentScoreText.text = "Score: 0";
        }

    }

    //////////////////////////////////////////////////////////////////////////////
    ///////METHODS
    //////////////////////////////////////////////////////////////////////////////

    private void SpawnPipe()
    {
        Instantiate(pipePrefab, new Vector3(2.5f, 0f, 0f), Quaternion.identity);
    }

    private void CancelPipes()
    {
        CancelInvoke("SpawnPipe");
        foundPipes = FindObjectsOfType<Pipe>();

        foreach (Pipe pipe in foundPipes)
        {
            Destroy(pipe.gameObject);
        }
    }

    private void SpawnBirds(Bird birdToMake, bool isFirstTime)
    {

        string path = Application.dataPath + "/Best Bird/BestBird.json";

        if (!isFirstTime)
        {
            for (int k = 0; k < birdCount; k++)
            {
                Bird newBird = Instantiate(birdToMake);

                int bestBirdIndex = PickBestBird();

                newBird.nn = new NeuralNetwork(GameSpawner.inputNodes, GameSpawner.hiddenNodes, GameSpawner.outputNodes);
                newBird.nnInput = new Matrix(GameSpawner.inputNodes, 1);
                newBird.nnOutput = new Matrix(GameSpawner.outputNodes, 1);

                //Weights_IH
                for (int i = 0; i < hiddenNodes; i++)
                {
                    for (int j = 0; j < inputNodes; j++)
                    {
                        newBird.nn.weights_IH.matrix[i, j] = prevGenBirds[bestBirdIndex].weights_IH.matrix[i, j];
                        newBird.nn.weights_IH.Map(Mutate);
                    }
                }

                //Weights_HO
                for (int i = 0; i < outputNodes; i++)
                {
                    for (int j = 0; j < hiddenNodes; j++)
                    {
                        newBird.nn.weights_HO.matrix[i, j] = prevGenBirds[bestBirdIndex].weights_HO.matrix[i, j];
                        newBird.nn.weights_HO.Map(Mutate);
                    }
                }

                //Bias_H
                for (int i = 0; i < hiddenNodes; i++)
                {

                    newBird.nn.bias_H.matrix[i, 0] = prevGenBirds[bestBirdIndex].bias_H.matrix[i, 0];
                    newBird.nn.bias_H.Map(Mutate);

                }

                //Bias_O
                for (int i = 0; i < outputNodes; i++)
                {

                    newBird.nn.bias_O.matrix[i, 0] = prevGenBirds[bestBirdIndex].bias_O.matrix[i, 0];
                    newBird.nn.bias_O.Map(Mutate);

                }

                if (k == 0)
                {
                    //Get best bird
                    BirdJson toSave = new BirdJson(Score.currentScore,
                                                    newBird.nn.weights_IH,
                                                    newBird.nn.weights_HO,
                                                    newBird.nn.bias_H,
                                                    newBird.nn.bias_O);

                    //Convert to Json
                    string toSaveJson = JsonUtility.ToJson(toSave);

                    //If no Json file exists, create file with best bird
                    //Else, compare it with the already created file
                    if (!File.Exists(path))
                    {
                        File.WriteAllText(path, toSaveJson);
                    }
                    else
                    {
                        string loadedJson = File.ReadAllText(path);
                        BirdJson toCompareBird = JsonUtility.FromJson<BirdJson>(loadedJson);

                        if (toSave.fitness > toCompareBird.fitness)
                        {
                            File.WriteAllText(path, toSaveJson);
                        }
                    }
                }

            }

        }

        else
        {
            if (File.Exists(path))
            {
                File.ReadAllText(path);
                string loadedJson = File.ReadAllText(path);
                BirdJson loadedBird = JsonUtility.FromJson<BirdJson>(loadedJson);

                for (int k = 0; k < birdCount; k++)
                {
                    Bird newBird = Instantiate(birdToMake);

                    newBird.nn = new NeuralNetwork(GameSpawner.inputNodes, GameSpawner.hiddenNodes, GameSpawner.outputNodes);
                    newBird.nnInput = new Matrix(GameSpawner.inputNodes, 1);
                    newBird.nnOutput = new Matrix(GameSpawner.outputNodes, 1);

                    //Weights_IH
                    for (int i = 0; i < hiddenNodes; i++)
                    {
                        for (int j = 0; j < inputNodes; j++)
                        {
                            newBird.nn.weights_IH.matrix[i, j] = loadedBird.weights_IH[i * inputNodes + j];
                        }
                    }

                    //Weights_HO
                    for (int i = 0; i < outputNodes; i++)
                    {
                        for (int j = 0; j < hiddenNodes; j++)
                        {
                            newBird.nn.weights_HO.matrix[i, j] = loadedBird.weights_HO[i * hiddenNodes + j];
                        }
                    }

                    //Bias_H
                    for (int i = 0; i < hiddenNodes; i++)
                    {

                        newBird.nn.bias_H.matrix[i, 0] = loadedBird.bias_H[i];

                    }

                    //Bias_O
                    for (int i = 0; i < outputNodes; i++)
                    {

                        newBird.nn.bias_O.matrix[i, 0] = loadedBird.bias_O[i];

                    }
                }
            }
            else
            {
                for (int i = 0; i < birdCount; i++)
                {
                    Instantiate(birdToMake);
                }
            }

        }

        prevGenBirdsListIndex = 0;
        prevGenBirds = new DeadBirdHolder[birdCount];

        for (int i = 0; i < birdCount; i++)
        {
            prevGenBirds[i] = new DeadBirdHolder();

            prevGenBirds[i].weights_IH = new Matrix(hiddenNodes, inputNodes);
            prevGenBirds[i].weights_HO = new Matrix(outputNodes, hiddenNodes);

            prevGenBirds[i].bias_H = new Matrix(hiddenNodes, 1);
            prevGenBirds[i].bias_O = new Matrix(outputNodes, 1);
        }
    }

    private int CountBirds()
    {
        foundBirds = FindObjectsOfType<Bird>();
        return foundBirds.Length;
    }

    private void NormalizeFitness()
    {
        float sum = 0;
        for (int i = 0; i < prevGenBirds.Length; i++)
        {
            sum += prevGenBirds[i].fitness;
        }

        if (sum > 0)
        {
            for (int i = 0; i < prevGenBirds.Length; i++)
            {
                prevGenBirds[i].fitness /= sum;
            }
        }
    }

    private int PickBestBird()
    {
        int select = 0;
        double selector = Random.Range(0f, 1f);

        while (selector > 0)
        {
            selector -= prevGenBirds[select].fitness;
            select++;
            if (select == birdCount) break;
        }
        select--;

        return select;
    }

    private float Mutate(float val)
    {
        float willMutate = Random.Range(0, 1);
        if (willMutate < mutationRate)
        {
            return val += RandomGaussian(0, 0.1f);
        }
        return val;
    }

    private float RandomGaussian(float mean, float deviation)
    {
        float rand1 = Random.Range(0.0f, 1.0f);
        float rand2 = Random.Range(0.0f, 1.0f);

        float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        return (mean + deviation * n);
    }

}


public class BirdJson
{
    public float fitness;
    public float[] weights_IH, weights_HO;
    public float[] bias_H, bias_O;

    public BirdJson(float fitness, Matrix weights_IH2D, Matrix weights_HO2D, Matrix bias_H2D, Matrix bias_O2D)
    {
        weights_IH = new float[weights_IH2D.rows * weights_IH2D.cols];
        weights_HO = new float[weights_HO2D.rows * weights_HO2D.cols];
        bias_H = new float[bias_H2D.rows * bias_H2D.cols];
        bias_O = new float[bias_O2D.rows * bias_O2D.cols];

        this.fitness = fitness;

        for (int i = 0; i < weights_IH2D.rows; i++)
        {
            for (int j = 0; j < weights_IH2D.cols; j++)
            {
                weights_IH[i * weights_IH2D.cols + j] = weights_IH2D.matrix[i, j];
            }
        }

        for (int i = 0; i < weights_HO2D.rows; i++)
        {
            for (int j = 0; j < weights_HO2D.cols; j++)
            {
                weights_HO[i * weights_HO2D.cols + j] = weights_HO2D.matrix[i, j];
            }
        }

        for (int i = 0; i < bias_H2D.rows; i++)
        {
            for (int j = 0; j < bias_H2D.cols; j++)
            {
                bias_H[i * bias_H2D.cols + j] = bias_H2D.matrix[i, j];
            }
        }

        for (int i = 0; i < bias_O2D.rows; i++)
        {
            for (int j = 0; j < bias_O2D.cols; j++)
            {
                bias_O[i * bias_O2D.cols + j] = bias_O2D.matrix[i, j];
            }
        }
    }
}