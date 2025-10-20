
namespace PropertyAnalyser.Models
{
    // Represents the final top-level JSON object.
    public record ReportOutput
    {
        public List<SuburbReport> TopSuburbs { get; init; } = new();
    }
}