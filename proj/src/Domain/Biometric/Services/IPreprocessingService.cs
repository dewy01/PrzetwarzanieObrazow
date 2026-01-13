namespace MapEditor.Domain.Biometric.Services;

/// <summary>
/// Service for preprocessing binary matrices (noise reduction and binarization).
/// </summary>
public interface IPreprocessingService
{
    /// <summary>
    /// Applies a median filter to reduce noise in a binary matrix.
    /// The filter replaces each pixel with the median value of its neighborhood.
    /// </summary>
    /// <param name="matrix">The input binary matrix</param>
    /// <param name="kernelSize">The size of the filter kernel (must be odd, e.g., 3, 5, 7)</param>
    /// <returns>The filtered binary matrix</returns>
    int[,] ApplyMedianFilter(int[,] matrix, int kernelSize = 3);

    /// <summary>
    /// Applies Otsu's binarization method to automatically determine the optimal threshold.
    /// For binary matrices, this can be used to optimize the threshold if grayscale data is available.
    /// </summary>
    /// <param name="matrix">The input grayscale or binary matrix</param>
    /// <returns>A tuple containing the binarized matrix and the calculated threshold</returns>
    (int[,] binarized, int threshold) ApplyOtsuBinarization(int[,] matrix);

    /// <summary>
    /// Applies both median filter and Otsu binarization in sequence.
    /// </summary>
    /// <param name="matrix">The input matrix</param>
    /// <param name="kernelSize">The size of the median filter kernel</param>
    /// <returns>The preprocessed binary matrix</returns>
    int[,] Preprocess(int[,] matrix, int kernelSize = 3);
}
