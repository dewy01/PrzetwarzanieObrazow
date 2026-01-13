using MapEditor.Domain.Biometric.Services;

namespace MapEditor.Infrastructure.Algorithms;

/// <summary>
/// Implementation of Zhang-Suen thinning algorithm for skeletonization.
/// This is a parallel thinning algorithm that preserves connectivity and removes
/// pixels from object boundaries iteratively.
/// </summary>
public class SkeletonizationService : ISkeletonizationService
{
    /// <inheritdoc/>
    public int[,] ZhangSuenThinning(int[,] matrix)
    {
        var (skeleton, _) = ZhangSuenWithIterations(matrix);
        return skeleton;
    }

    /// <inheritdoc/>
    public (int[,] skeleton, int iterations) ZhangSuenWithIterations(int[,] matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        // Create working copy
        int[,] result = (int[,])matrix.Clone();
        bool pixelsChanged;
        int iterations = 0;

        do
        {
            pixelsChanged = false;

            // Subiteration 1
            var toDelete1 = new List<(int x, int y)>();
            for (int y = 1; y < rows - 1; y++)
            {
                for (int x = 1; x < columns - 1; x++)
                {
                    if (result[y, x] == 1 && ShouldDeleteStep1(result, x, y))
                    {
                        toDelete1.Add((x, y));
                    }
                }
            }

            foreach (var (x, y) in toDelete1)
            {
                result[y, x] = 0;
                pixelsChanged = true;
            }

            // Subiteration 2
            var toDelete2 = new List<(int x, int y)>();
            for (int y = 1; y < rows - 1; y++)
            {
                for (int x = 1; x < columns - 1; x++)
                {
                    if (result[y, x] == 1 && ShouldDeleteStep2(result, x, y))
                    {
                        toDelete2.Add((x, y));
                    }
                }
            }

            foreach (var (x, y) in toDelete2)
            {
                result[y, x] = 0;
                pixelsChanged = true;
            }

            iterations++;

        } while (pixelsChanged && iterations < 1000); // Safety limit

        return (result, iterations);
    }

    /// <inheritdoc/>
    public Dictionary<string, int> CalculateSkeletonMetrics(int[,] skeleton)
    {
        if (skeleton == null)
            throw new ArgumentNullException(nameof(skeleton));

        int rows = skeleton.GetLength(0);
        int columns = skeleton.GetLength(1);

        int totalPixels = 0;
        int endpoints = 0;
        int junctions = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (skeleton[y, x] == 1)
                {
                    totalPixels++;

                    // Count neighbors
                    int neighbors = CountNeighbors(skeleton, x, y);

                    if (neighbors == 1)
                        endpoints++; // Endpoint has exactly 1 neighbor
                    else if (neighbors >= 3)
                        junctions++; // Junction has 3 or more neighbors
                }
            }
        }

        return new Dictionary<string, int>
        {
            ["SkeletonPixels"] = totalPixels,
            ["Endpoints"] = endpoints,
            ["Junctions"] = junctions,
            ["Branches"] = junctions > 0 ? junctions : 0
        };
    }

    private bool ShouldDeleteStep1(int[,] matrix, int x, int y)
    {
        // Get 8 neighbors (P2-P9 in clockwise order starting from top)
        int p2 = GetPixel(matrix, x, y - 1);     // North
        int p3 = GetPixel(matrix, x + 1, y - 1); // Northeast
        int p4 = GetPixel(matrix, x + 1, y);     // East
        int p5 = GetPixel(matrix, x + 1, y + 1); // Southeast
        int p6 = GetPixel(matrix, x, y + 1);     // South
        int p7 = GetPixel(matrix, x - 1, y + 1); // Southwest
        int p8 = GetPixel(matrix, x - 1, y);     // West
        int p9 = GetPixel(matrix, x - 1, y - 1); // Northwest

        // Condition 1: 2 <= B(P1) <= 6 (number of non-zero neighbors)
        int b = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
        if (b < 2 || b > 6)
            return false;

        // Condition 2: A(P1) = 1 (number of 0-1 transitions in ordered sequence)
        int a = CountTransitions(p2, p3, p4, p5, p6, p7, p8, p9);
        if (a != 1)
            return false;

        // Condition 3: P2 * P4 * P6 = 0
        if (p2 * p4 * p6 != 0)
            return false;

        // Condition 4: P4 * P6 * P8 = 0
        if (p4 * p6 * p8 != 0)
            return false;

        return true;
    }

    private bool ShouldDeleteStep2(int[,] matrix, int x, int y)
    {
        // Get 8 neighbors
        int p2 = GetPixel(matrix, x, y - 1);
        int p3 = GetPixel(matrix, x + 1, y - 1);
        int p4 = GetPixel(matrix, x + 1, y);
        int p5 = GetPixel(matrix, x + 1, y + 1);
        int p6 = GetPixel(matrix, x, y + 1);
        int p7 = GetPixel(matrix, x - 1, y + 1);
        int p8 = GetPixel(matrix, x - 1, y);
        int p9 = GetPixel(matrix, x - 1, y - 1);

        // Conditions 1 and 2 are same as step 1
        int b = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;
        if (b < 2 || b > 6)
            return false;

        int a = CountTransitions(p2, p3, p4, p5, p6, p7, p8, p9);
        if (a != 1)
            return false;

        // Condition 3: P2 * P4 * P8 = 0
        if (p2 * p4 * p8 != 0)
            return false;

        // Condition 4: P2 * P6 * P8 = 0
        if (p2 * p6 * p8 != 0)
            return false;

        return true;
    }

    private int GetPixel(int[,] matrix, int x, int y)
    {
        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        if (x < 0 || x >= columns || y < 0 || y >= rows)
            return 0;

        return matrix[y, x];
    }

    private int CountTransitions(int p2, int p3, int p4, int p5, int p6, int p7, int p8, int p9)
    {
        // Count 0->1 transitions in the ordered sequence
        int[] pixels = { p2, p3, p4, p5, p6, p7, p8, p9, p2 }; // Wrap around
        int transitions = 0;

        for (int i = 0; i < 8; i++)
        {
            if (pixels[i] == 0 && pixels[i + 1] == 1)
                transitions++;
        }

        return transitions;
    }

    private int CountNeighbors(int[,] matrix, int x, int y)
    {
        int count = 0;
        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        // 8-connectivity
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < columns && ny >= 0 && ny < rows)
                {
                    if (matrix[ny, nx] == 1)
                        count++;
                }
            }
        }

        return count;
    }

    /// <inheritdoc/>
    public int[,] K3MThinning(int[,] matrix)
    {
        var (skeleton, _) = K3MWithIterations(matrix);
        return skeleton;
    }

    /// <inheritdoc/>
    public (int[,] skeleton, int iterations) K3MWithIterations(int[,] matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        // Create working copy
        int[,] result = (int[,])matrix.Clone();
        bool pixelsChanged;
        int iterations = 0;

        do
        {
            pixelsChanged = false;

            // Phase 0: Remove pixels with specific border patterns
            var toDelete0 = new List<(int x, int y)>();
            for (int y = 1; y < rows - 1; y++)
            {
                for (int x = 1; x < columns - 1; x++)
                {
                    if (result[y, x] == 1 && ShouldDeleteK3MPhase0(result, x, y))
                    {
                        toDelete0.Add((x, y));
                    }
                }
            }

            foreach (var (x, y) in toDelete0)
            {
                result[y, x] = 0;
                pixelsChanged = true;
            }

            // Phase 1-5: Directional thinning
            for (int phase = 1; phase <= 5; phase++)
            {
                var toDelete = new List<(int x, int y)>();
                for (int y = 1; y < rows - 1; y++)
                {
                    for (int x = 1; x < columns - 1; x++)
                    {
                        if (result[y, x] == 1 && ShouldDeleteK3MPhase(result, x, y, phase))
                        {
                            toDelete.Add((x, y));
                        }
                    }
                }

                foreach (var (x, y) in toDelete)
                {
                    result[y, x] = 0;
                    pixelsChanged = true;
                }
            }

            iterations++;

        } while (pixelsChanged && iterations < 1000); // Safety limit

        return (result, iterations);
    }

    private bool ShouldDeleteK3MPhase0(int[,] matrix, int x, int y)
    {
        // Get 8 neighbors (clockwise from top)
        int p2 = GetPixel(matrix, x, y - 1);
        int p3 = GetPixel(matrix, x + 1, y - 1);
        int p4 = GetPixel(matrix, x + 1, y);
        int p5 = GetPixel(matrix, x + 1, y + 1);
        int p6 = GetPixel(matrix, x, y + 1);
        int p7 = GetPixel(matrix, x - 1, y + 1);
        int p8 = GetPixel(matrix, x - 1, y);
        int p9 = GetPixel(matrix, x - 1, y - 1);

        // Count number of non-zero neighbors
        int neighborCount = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;

        // Phase 0: Remove border pixels with 1 neighbor (endpoints not on main structure)
        if (neighborCount == 1)
            return false; // Keep single-neighbor pixels (endpoints)

        // Count connectivity (0->1 transitions)
        int connectivity = CountTransitions(p2, p3, p4, p5, p6, p7, p8, p9);

        // Remove pixels that don't affect connectivity
        return neighborCount >= 2 && neighborCount <= 6 && connectivity == 1;
    }

    private bool ShouldDeleteK3MPhase(int[,] matrix, int x, int y, int phase)
    {
        // Get 8 neighbors
        int p2 = GetPixel(matrix, x, y - 1);
        int p3 = GetPixel(matrix, x + 1, y - 1);
        int p4 = GetPixel(matrix, x + 1, y);
        int p5 = GetPixel(matrix, x + 1, y + 1);
        int p6 = GetPixel(matrix, x, y + 1);
        int p7 = GetPixel(matrix, x - 1, y + 1);
        int p8 = GetPixel(matrix, x - 1, y);
        int p9 = GetPixel(matrix, x - 1, y - 1);

        int neighborCount = p2 + p3 + p4 + p5 + p6 + p7 + p8 + p9;

        // Don't remove if only 1 or 2 neighbors (preserve thin lines and endpoints)
        if (neighborCount < 2 || neighborCount > 6)
            return false;

        // Check connectivity
        int connectivity = CountTransitions(p2, p3, p4, p5, p6, p7, p8, p9);
        if (connectivity != 1)
            return false;

        // Directional removal based on phase
        switch (phase)
        {
            case 1: // North border
                return p2 == 0 && (p4 == 0 || p6 == 0 || p8 == 0);
            case 2: // East border
                return p4 == 0 && (p2 == 0 || p6 == 0 || p8 == 0);
            case 3: // South border
                return p6 == 0 && (p2 == 0 || p4 == 0 || p8 == 0);
            case 4: // West border
                return p8 == 0 && (p2 == 0 || p4 == 0 || p6 == 0);
            case 5: // Diagonal refinement
                return (p2 * p4 * p6 == 0) && (p4 * p6 * p8 == 0);
            default:
                return false;
        }
    }
}
