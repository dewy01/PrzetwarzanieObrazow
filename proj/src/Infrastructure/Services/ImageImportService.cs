using MapEditor.Application.Services;
using MapEditor.Domain.Biometric.Services;
using MapEditor.Domain.Editing.Entities;
using MapEditor.Domain.Editing.Services;
using MapEditor.Domain.Editing.ValueObjects;
using MapEditor.Domain.Shared.Enums;
using System.Drawing;
using System.Drawing.Imaging;

namespace MapEditor.Infrastructure.Services;

/// <summary>
/// Implementacja service do importu obrazów i tworzenia Preset
/// </summary>
public class ImageImportService : IImageImportService
{
    private readonly IFragmentationService _fragmentationService;
    private readonly IPreprocessingService _preprocessingService;

    public ImageImportService(
        IFragmentationService fragmentationService,
        IPreprocessingService preprocessingService)
    {
        _fragmentationService = fragmentationService ?? throw new ArgumentNullException(nameof(fragmentationService));
        _preprocessingService = preprocessingService ?? throw new ArgumentNullException(nameof(preprocessingService));
    }

    public async Task<byte[,]> LoadImageAsync(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Image file not found: {filePath}");

        return await Task.Run(() =>
        {
            using var bitmap = new Bitmap(filePath);
            return ConvertBitmapToGrayscale(bitmap);
        });
    }

    public async Task<Preset> CreatePresetFromImageAsync(
        string imagePath,
        string presetName,
        int threshold = 128)
    {
        // 1. Load image
        var grayMatrix = await LoadImageAsync(imagePath);
        int height = grayMatrix.GetLength(0);
        int width = grayMatrix.GetLength(1);

        // 2. Apply preprocessing (median filter + binarization)
        var intMatrix = ConvertByteToInt(grayMatrix);
        var preprocessed = _preprocessingService.ApplyMedianFilter(intMatrix, 3);
        var binarized = BinarizeMatrix(preprocessed, threshold);

        // 3. Run fragmentation to find Map regions (foreground = 1)
        var fragments = _fragmentationService.DetectFragments(binarized);

        if (fragments.Count == 0)
            throw new InvalidOperationException("No fragments found in image. The image may be empty or all background.");

        // 4. Find the largest fragment (main map)
        var mainFragment = FindLargestFragment(fragments);

        // 5. Convert fragment to Preset
        var preset = ConvertFragmentToPreset(mainFragment, ConvertIntToByte(binarized), presetName);

        return preset;
    }

    private int[,] ConvertByteToInt(byte[,] byteMatrix)
    {
        int height = byteMatrix.GetLength(0);
        int width = byteMatrix.GetLength(1);
        var result = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result[y, x] = byteMatrix[y, x];
            }
        }

        return result;
    }

    private byte[,] ConvertIntToByte(int[,] intMatrix)
    {
        int height = intMatrix.GetLength(0);
        int width = intMatrix.GetLength(1);
        var result = new byte[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                result[y, x] = (byte)intMatrix[y, x];
            }
        }

        return result;
    }

    private byte[,] ConvertBitmapToGrayscale(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;
        var result = new byte[height, width];

        var bitmapData = bitmap.LockBits(
            new Rectangle(0, 0, width, height),
            ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        unsafe
        {
            byte* ptr = (byte*)bitmapData.Scan0;
            int stride = bitmapData.Stride;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = y * stride + x * 3;
                    byte b = ptr[offset];
                    byte g = ptr[offset + 1];
                    byte r = ptr[offset + 2];

                    // Convert to grayscale using standard luminosity formula
                    byte gray = (byte)(0.299 * r + 0.587 * g + 0.114 * b);
                    result[y, x] = gray;
                }
            }
        }

        bitmap.UnlockBits(bitmapData);
        return result;
    }

    private int[,] BinarizeMatrix(int[,] gray, int threshold)
    {
        int height = gray.GetLength(0);
        int width = gray.GetLength(1);
        var binary = new int[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                binary[y, x] = gray[y, x] >= threshold ? 1 : 0;
            }
        }

        return binary;
    }

    private Domain.Biometric.ValueObjects.Fragment FindLargestFragment(
        List<Domain.Biometric.ValueObjects.Fragment> fragments)
    {
        if (fragments.Count == 0)
            throw new InvalidOperationException("No fragments found in image");

        // Return fragment with most pixels
        return fragments.OrderByDescending(f => f.PixelCount).First();
    }

    private Preset ConvertFragmentToPreset(
        Domain.Biometric.ValueObjects.Fragment fragment,
        byte[,] binary,
        string presetName)
    {
        if (fragment.PixelCount == 0)
            throw new InvalidOperationException("Cannot create preset from empty fragment");

        int width = fragment.MaxX - fragment.MinX + 1;
        int height = fragment.MaxY - fragment.MinY + 1;

        // Create square definitions only for foreground pixels (Map area)
        // Preset should contain only the shape itself, not empty background areas
        var squares = new List<SquareDefinition>();
        foreach (var pixel in fragment.Pixels)
        {
            // Only add pixels that belong to the fragment (foreground = 1)
            if (binary[pixel.y, pixel.x] == 1)
            {
                var relativePos = new Domain.Editing.ValueObjects.Point(
                    pixel.x - fragment.MinX,
                    pixel.y - fragment.MinY
                );

                squares.Add(new SquareDefinition(relativePos, SquareType.Grass));
            }
        }

        // Preset size is the minimal bounding box of the fragment
        // This ensures the preset only contains the actual shape, not surrounding empty space
        var size = new Domain.Editing.ValueObjects.Size(width, height);
        var origin = new Domain.Editing.ValueObjects.Point(0, 0);

        return new Preset(presetName, size, squares, origin);
    }
}
