namespace PropertyAnalyser.Models
{
    // Represents the calculated data for a single suburb in the output.
    public record SuburbReport
    {
        public string Name { get; init; } = string.Empty;
        public double AverageYield { get; init; }
        
        public double MedianPricePerSqM { get; init; }
        public double StdDevYield { get; init; }

        public double AveragePricePerSqM { get; init; }
        public int PropertyCount { get; init; }
    }
}
