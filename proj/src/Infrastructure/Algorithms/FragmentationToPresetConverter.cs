using System;
using System.Collections.Generic;
using System.Linq;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;

namespace MapEditor.Infrastructure.Algorithms;

/// <summary>
/// Converts fragmentation results (connected components) into reusable Preset structures.
/// Implements the algorithmic conversion from Map regions to Preset layouts.
/// </summary>
public class FragmentationToPresetConverter
{
    /// <summary>
    /// Converts a boolean matrix representing a fragmented region into a Preset structure.
    /// </summary>
    /// <param name="fragmentMatrix">Boolean matrix where true = region, false = background</param>
    /// <param name="regionName">Name for the generated preset</param>
    /// <param name="squareType">Type of squares to use in preset (default: Stone)</param>
    /// <returns>Preset with squares positioned to match the fragment pattern</returns>
    public Preset ConvertFragmentToPreset(
        bool[,] fragmentMatrix,
        string regionName,
        SquareType squareType = SquareType.Stone)
    {
        if (fragmentMatrix == null)
            throw new ArgumentNullException(nameof(fragmentMatrix));

        if (fragmentMatrix.GetLength(0) == 0 || fragmentMatrix.GetLength(1) == 0)
            throw new ArgumentException("Fragment matrix cannot be empty", nameof(fragmentMatrix));

        // Step 1: Get bounding box of the region
        var (minX, maxX, minY, maxY) = GetBoundingBox(fragmentMatrix);

        if (minX == -1)
            throw new InvalidOperationException("Fragment matrix contains no marked regions");

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        // Step 2: Determine optimal square size
        int optimalSquareSize = FindOptimalSquareSize(fragmentMatrix, minX, maxX, minY, maxY);

        // Step 3: Tile region into squares
        var squareDefinitions = TileRegionIntoSquares(
            fragmentMatrix,
            minX, maxX, minY, maxY,
            optimalSquareSize,
            squareType
        );

        // Step 4: Create Preset with calculated dimensions
        int presetWidth = (width + optimalSquareSize - 1) / optimalSquareSize;
        int presetHeight = (height + optimalSquareSize - 1) / optimalSquareSize;

        var preset = new Preset(
            name: $"{regionName}_Preset",
            size: new Size(presetWidth, presetHeight),
            squares: squareDefinitions,
            entities: null,
            originPoint: new Point(0, 0)
        );

        return preset;
    }

    /// <summary>
    /// Gets the bounding box (min/max coordinates) of all true values in the matrix.
    /// </summary>
    private (int minX, int maxX, int minY, int maxY) GetBoundingBox(bool[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        int minX = -1, maxX = -1, minY = -1, maxY = -1;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (matrix[y, x])
                {
                    if (minX == -1 || x < minX) minX = x;
                    if (maxX == -1 || x > maxX) maxX = x;
                    if (minY == -1 || y < minY) minY = y;
                    if (maxY == -1 || y > maxY) maxY = y;
                }
            }
        }

        return (minX, maxX, minY, maxY);
    }

    /// <summary>
    /// Determines the optimal square size for tiling the region.
    /// Strategy: Find the size that minimizes total squares while maximizing coverage.
    /// </summary>
    private int FindOptimalSquareSize(
        bool[,] matrix,
        int minX, int maxX, int minY, int maxY)
    {
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        int filledPixels = CountFilledPixels(matrix, minX, maxX, minY, maxY);

        // Available square sizes to test
        int[] candidateSizes = { 1, 2, 4, 8, 16 };

        double bestScore = double.MaxValue;
        int optimalSize = 1;

        foreach (int size in candidateSizes)
        {
            // Calculate how many squares needed
            int squaresX = (width + size - 1) / size;
            int squaresY = (height + size - 1) / size;
            int totalSquares = squaresX * squaresY;

            // Count how well this size covers the region
            int coveredPixels = CountCoveredPixels(
                matrix, minX, maxX, minY, maxY, size
            );

            // Coverage ratio (higher is better)
            double coverage = (double)coveredPixels / (squaresX * squaresY * size * size);

            // Score: minimize squares, maximize coverage
            // Lower score is better
            double score = totalSquares / (coverage + 0.1);

            if (score < bestScore)
            {
                bestScore = score;
                optimalSize = size;
            }
        }

        return optimalSize;
    }

    /// <summary>
    /// Counts the number of filled pixels in the region.
    /// </summary>
    private int CountFilledPixels(bool[,] matrix, int minX, int maxX, int minY, int maxY)
    {
        int count = 0;
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (matrix[y, x])
                    count++;
            }
        }
        return count;
    }

    /// <summary>
    /// Counts how many pixels are covered by squares of the given size.
    /// </summary>
    private int CountCoveredPixels(
        bool[,] matrix,
        int minX, int maxX, int minY, int maxY,
        int squareSize)
    {
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;
        int count = 0;

        // Iterate through square grid
        for (int sy = 0; sy < (height + squareSize - 1) / squareSize; sy++)
        {
            for (int sx = 0; sx < (width + squareSize - 1) / squareSize; sx++)
            {
                int startY = minY + sy * squareSize;
                int startX = minX + sx * squareSize;

                // Check if this square contains any filled pixels
                for (int py = startY; py < startY + squareSize && py <= maxY; py++)
                {
                    for (int px = startX; px < startX + squareSize && px <= maxX; px++)
                    {
                        if (matrix[py, px])
                        {
                            count++;
                        }
                    }
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Tiles the region into squares, creating a list of square definitions.
    /// </summary>
    private List<SquareDefinition> TileRegionIntoSquares(
        bool[,] matrix,
        int minX, int maxX, int minY, int maxY,
        int squareSize,
        SquareType squareType)
    {
        var squareDefinitions = new List<SquareDefinition>();

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        // Threshold: if more than 50% of square is filled, include it
        int threshold = (squareSize * squareSize) / 2;

        for (int sy = 0; sy < (height + squareSize - 1) / squareSize; sy++)
        {
            for (int sx = 0; sx < (width + squareSize - 1) / squareSize; sx++)
            {
                int startY = minY + sy * squareSize;
                int startX = minX + sx * squareSize;

                int filledCount = 0;

                // Count filled pixels in this square
                for (int py = startY; py < startY + squareSize && py <= maxY; py++)
                {
                    for (int px = startX; px < startX + squareSize && px <= maxX; px++)
                    {
                        if (matrix[py, px])
                            filledCount++;
                    }
                }

                // If more than threshold is filled, include this square
                if (filledCount >= threshold)
                {
                    // Normalize to (0,0) origin
                    var relativePosition = new Point(sx, sy);
                    var squareDef = new SquareDefinition(relativePosition, squareType);
                    squareDefinitions.Add(squareDef);
                }
            }
        }

        return squareDefinitions;
    }
}
