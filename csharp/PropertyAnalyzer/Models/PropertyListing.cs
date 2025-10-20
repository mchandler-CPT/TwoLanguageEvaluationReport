namespace PropertyAnalyser.Models
{
    // Represents a single row from the input CSV file.
    public record PropertyListing
    {
        public int ListingId { get; init; }
        public string Address { get; init; } = string.Empty;
        public string Suburb { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public int GrossLettableArea { get; init; }
        public decimal NetAnnualIncome { get; init; }
    }
}