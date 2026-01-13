using MapEditor.Domain.Biometric.Services;

namespace MapEditor.Infrastructure.Algorithms;

/// <summary>
/// Implementation of preprocessing algorithms for binary matrices.
/// </summary>
public class PreprocessingService : IPreprocessingService
{
    /// <inheritdoc/>
    public int[,] ApplyMedianFilter(int[,] matrix, int kernelSize = 3)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        if (kernelSize < 1 || kernelSize % 2 == 0)
            throw new ArgumentException("Kernel size must be a positive odd number", nameof(kernelSize));

        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);
        int[,] result = new int[rows, columns];
        int offset = kernelSize / 2;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                // Extract neighborhood values
                List<int> neighborhood = new List<int>();

                for (int ky = -offset; ky <= offset; ky++)
                {
                    for (int kx = -offset; kx <= offset; kx++)
                    {
                        int ny = y + ky;
                        int nx = x + kx;

                        // Check bounds
                        if (ny >= 0 && ny < rows && nx >= 0 && nx < columns)
                        {
                            neighborhood.Add(matrix[ny, nx]);
                        }
                    }
                }

                // Calculate median
                neighborhood.Sort();
                int median = neighborhood[neighborhood.Count / 2];
                result[y, x] = median;
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public (int[,] binarized, int threshold) ApplyOtsuBinarization(int[,] matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        // Build histogram
        int maxValue = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (matrix[y, x] > maxValue)
                    maxValue = matrix[y, x];
            }
        }

        // For binary matrices (0 and 1), Otsu will find optimal threshold
        // For grayscale, it will find the best separation
        int[] histogram = new int[maxValue + 1];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                histogram[matrix[y, x]]++;
            }
        }

        int totalPixels = rows * columns;

        // Calculate Otsu threshold
        int optimalThreshold = CalculateOtsuThreshold(histogram, totalPixels);

        // Apply threshold
        int[,] binarized = new int[rows, columns];
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                binarized[y, x] = matrix[y, x] > optimalThreshold ? 1 : 0;
            }
        }

        return (binarized, optimalThreshold);
    }

    /// <inheritdoc/>
    public int[,] Preprocess(int[,] matrix, int kernelSize = 3)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        // Step 1: Apply median filter to reduce noise
        var filtered = ApplyMedianFilter(matrix, kernelSize);

        // Step 2: Apply Otsu binarization
        var (binarized, _) = ApplyOtsuBinarization(filtered);

        return binarized;
    }

    private int CalculateOtsuThreshold(int[] histogram, int totalPixels)
    {
        double sum = 0;
        for (int i = 0; i < histogram.Length; i++)
        {
            sum += i * histogram[i];
        }

        double sumBackground = 0;
        int weightBackground = 0;
        int weightForeground;

        double maxVariance = 0;
        int threshold = 0;

        for (int t = 0; t < histogram.Length; t++)
        {
            weightBackground += histogram[t];
            if (weightBackground == 0)
                continue;

            weightForeground = totalPixels - weightBackground;
            if (weightForeground == 0)
                break;

            sumBackground += t * histogram[t];

            double meanBackground = sumBackground / weightBackground;
            double meanForeground = (sum - sumBackground) / weightForeground;

            // Calculate between-class variance
            double variance = weightBackground * weightForeground *
                            Math.Pow(meanBackground - meanForeground, 2);

            if (variance > maxVariance)
            {
                maxVariance = variance;
                threshold = t;
            }
        }

        return threshold;
    }
}
