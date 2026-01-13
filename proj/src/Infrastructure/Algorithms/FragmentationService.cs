using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Biometric.ValueObjects;

namespace MapEditor.Infrastructure.Algorithms;

/// <summary>
/// Implementation of connected components labeling algorithm.
/// Uses flood-fill approach with 8-connectivity.
/// </summary>
public class FragmentationService : IFragmentationService
{
    /// <inheritdoc/>
    public List<Fragment> DetectFragments(int[,] matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);
        bool[,] visited = new bool[rows, columns];
        List<Fragment> fragments = new();
        int fragmentId = 1;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                // Start flood-fill from unvisited foreground pixels
                if (matrix[y, x] == 1 && !visited[y, x])
                {
                    var pixels = new List<(int x, int y)>();
                    FloodFill(matrix, visited, x, y, pixels);

                    if (pixels.Count > 0)
                    {
                        // Calculate bounding box
                        int minX = pixels.Min(p => p.x);
                        int maxX = pixels.Max(p => p.x);
                        int minY = pixels.Min(p => p.y);
                        int maxY = pixels.Max(p => p.y);

                        fragments.Add(new Fragment
                        {
                            Id = fragmentId++,
                            PixelCount = pixels.Count,
                            Pixels = pixels,
                            MinX = minX,
                            MaxX = maxX,
                            MinY = minY,
                            MaxY = maxY
                        });
                    }
                }
            }
        }

        return fragments;
    }

    /// <inheritdoc/>
    public int[,] CreateLabeledMatrix(int[,] matrix)
    {
        if (matrix == null)
            throw new ArgumentNullException(nameof(matrix));

        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);
        int[,] labeled = new int[rows, columns];
        bool[,] visited = new bool[rows, columns];
        int fragmentId = 1;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (matrix[y, x] == 1 && !visited[y, x])
                {
                    var pixels = new List<(int x, int y)>();
                    FloodFill(matrix, visited, x, y, pixels);

                    // Label all pixels in this fragment
                    foreach (var (px, py) in pixels)
                    {
                        labeled[py, px] = fragmentId;
                    }

                    fragmentId++;
                }
            }
        }

        return labeled;
    }

    /// <inheritdoc/>
    public Dictionary<string, double> CalculateStatistics(List<Fragment> fragments)
    {
        if (fragments == null)
            throw new ArgumentNullException(nameof(fragments));

        var stats = new Dictionary<string, double>();

        if (fragments.Count == 0)
        {
            stats["FragmentCount"] = 0;
            stats["AverageSize"] = 0;
            stats["LargestSize"] = 0;
            stats["SmallestSize"] = 0;
            return stats;
        }

        stats["FragmentCount"] = fragments.Count;
        stats["AverageSize"] = fragments.Average(f => f.PixelCount);
        stats["LargestSize"] = fragments.Max(f => f.PixelCount);
        stats["SmallestSize"] = fragments.Min(f => f.PixelCount);
        stats["TotalPixels"] = fragments.Sum(f => f.PixelCount);

        return stats;
    }

    private void FloodFill(int[,] matrix, bool[,] visited, int startX, int startY, List<(int x, int y)> pixels)
    {
        int rows = matrix.GetLength(0);
        int columns = matrix.GetLength(1);

        // Use stack for iterative flood-fill to avoid stack overflow
        Stack<(int x, int y)> stack = new();
        stack.Push((startX, startY));

        // 8-connectivity: all 8 neighbors
        int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
        int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();

            // Check bounds and if already visited
            if (x < 0 || x >= columns || y < 0 || y >= rows || visited[y, x] || matrix[y, x] == 0)
                continue;

            visited[y, x] = true;
            pixels.Add((x, y));

            // Check all 8 neighbors
            for (int i = 0; i < 8; i++)
            {
                int nx = x + dx[i];
                int ny = y + dy[i];
                stack.Push((nx, ny));
            }
        }
    }
}
