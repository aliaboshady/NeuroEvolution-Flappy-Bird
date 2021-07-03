using UnityEngine;
public class NeuralNetwork : MonoBehaviour
{
    int inputCount, hiddenCount, outputCount;
    public Matrix weights_IH, weights_HO;
    public Matrix bias_H, bias_O;

    public NeuralNetwork(int inputCount, int hiddenCount, int outputCount)
    {
        this.inputCount = inputCount;
        this.hiddenCount = hiddenCount;
        this.outputCount = outputCount;

        weights_IH = new Matrix(hiddenCount, inputCount);
        weights_IH.Randomize();

        weights_HO = new Matrix(outputCount, hiddenCount);
        weights_HO.Randomize();

        bias_H = new Matrix(hiddenCount, 1);
        bias_H.Randomize();

        bias_O = new Matrix(outputCount, 1);
        bias_O.Randomize();
    }

    public Matrix FeedForward(Matrix inputVal)
    {
        Matrix hiddenVal = Matrix.Dot(weights_IH, inputVal);
        hiddenVal.Add(bias_H);
        hiddenVal.Map(Sigmoid);

        Matrix outputVal = Matrix.Dot(weights_HO, hiddenVal);
        outputVal.Add(bias_O);
        outputVal.Map(Sigmoid);

        return outputVal;
    }

    public float Sigmoid(float num)
    {
        return (float)(1.0 / (1.0 + Mathf.Exp(-num)));
    }
}

