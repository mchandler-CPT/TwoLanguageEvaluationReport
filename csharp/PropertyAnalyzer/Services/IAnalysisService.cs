using PropertyAnalyser.Models;
using System.Collections.Generic;

namespace PropertyAnalyser.Services
{
    public interface IAnalysisService
    {
        List<SuburbReport> AnalyzeProperties(List<PropertyListing> listings);
    }
}