using UnityEngine;

public class Matrix
{
    public int rows, cols;
    public float[,] matrix;

    public Matrix(int rows, int cols)
    {
        this.rows = rows;
        this.cols = cols;
        matrix = new float[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = 0;
            }
        }
    }

    public void Randomize(float minVal = -1, float maxVal = 1)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] = Random.Range(minVal, maxVal);
            }
        }
    }

    public void Multiply(float num)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] *= num;
            }
        }
    }

    public void Add(float num)
    {

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] += num;
            }
        }

    }

    public void Add(Matrix mat)
    {
        if (mat.rows != rows || mat.cols != cols)
        {
            Debug.Log("ADD: Matrices' dimensions don't match!!!");
            return;
        }
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                matrix[i, j] += mat.matrix[i, j];
            }
        }
    }

    public delegate float InnerMethod(float x);
    public void Map(InnerMethod func)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                float val = matrix[i, j];
                matrix[i, j] = func(val);
            }
        }
    }

    public void PrintMatrix()
    {
        string matStr = "\n";
        for (int i = 0; i < rows; i++)
        {
            matStr += "    ";

            for (int j = 0; j < cols; j++)
            {
                matStr += matrix[i, j].ToString();
                if (j < cols - 1) matStr += "   ";
            }
            matStr += "\n\n";

        }
        Debug.Log(matStr);
    }

    ////////////////////////////////////////////////////////
    //Static Functions
    ////////////////////////////////////////////////////////

    public static Matrix Dot(Matrix mat1, Matrix mat2)
    {
        if (mat1.cols != mat2.rows)
        {
            Debug.Log("DOT: Matrices' dimensions don't match!!!");
            return new Matrix(0, 0);
        }

        Matrix outputMatrix = new Matrix(mat1.rows, mat2.cols);
        for (int i = 0; i < mat1.rows; i++)
        {
            for (int j = 0; j < mat2.cols; j++)
            {
                float product = 0;
                for (int k = 0; k < mat1.cols; k++)
                {
                    product += mat1.matrix[i, k] * mat2.matrix[k, j];

                }
                outputMatrix.matrix[i, j] = product;
            }
        }

        return outputMatrix;
    }

    public static Matrix Transpose(Matrix mat)
    {
        Matrix outputMatrix = new Matrix(mat.cols, mat.rows);
        for (int i = 0; i < mat.rows; i++)
        {
            for (int j = 0; j < mat.cols; j++)
            {
                outputMatrix.matrix[j, i] = mat.matrix[i, j];
            }
        }
        return outputMatrix;
    }

    public static Matrix Clone(Matrix mat)
    {
        Matrix outputMatrix = new Matrix(mat.rows, mat.cols);
        for (int i = 0; i < mat.rows; i++)
        {
            for (int j = 0; j < mat.cols; j++)
            {
                outputMatrix.matrix[i, j] = mat.matrix[i, j];
            }
        }
        return outputMatrix;
    }

}
