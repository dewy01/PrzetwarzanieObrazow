namespace MapEditor.Domain.Biometric.ValueObjects;

/// <summary>
/// Grid Features - Numeryczna reprezentacja wszystkich Structural features z danego obrazu lub 2D grid.
/// Contains metrics that describe the complexity and structure of a workspace.
/// </summary>
public class GridFeatures
{
  public int EndpointCount { get; set; }
  public int BifurcationCount { get; set; }
  public int CrossingCount { get; set; }
  public int TotalBranchPoints { get; set; }
  public int TotalSquareCount { get; set; }
  public int TotalEntityCount { get; set; }
  public int GridWidth { get; set; }
  public int GridHeight { get; set; }
  public int TotalSkeletonPixels { get; set; }
  public double BranchDensity { get; set; }
  public double BranchComplexity { get; set; }
  public double AverageEndpointDistance { get; set; }
  public double AverageBifurcationDistance { get; set; }

  /// <summary>
  /// Complexity Score: A single metric combining multiple features for quick comparison.
  /// Higher score = more complex structure.
  /// Formula: (BranchPoints * 10 + TotalSquares * 2 + Density * 5) / (Width * Height)
  /// </summary>
  public double ComplexityScore { get; set; }

  public override string ToString()
  {
    return $@"Grid Features Analysis
==========================================
Workspace Size: {GridWidth} x {GridHeight}
Total Squares: {TotalSquareCount}
Total Entities: {TotalEntityCount}

Branch Structure:
  Endpoints: {EndpointCount}
  Bifurcations: {BifurcationCount}
  Crossings: {CrossingCount}
  Total Branch Points: {TotalBranchPoints}

Skeleton Properties:
  Total Skeleton Pixels: {TotalSkeletonPixels}
  Branch Density: {BranchDensity:F2} per 100 pixels
  Branch Complexity: {BranchComplexity:F3}
  Avg Endpoint Distance: {AverageEndpointDistance:F2} pixels
  Avg Bifurcation Distance: {AverageBifurcationDistance:F2} pixels

Complexity Score: {ComplexityScore:F2}
==========================================";
  }
}
