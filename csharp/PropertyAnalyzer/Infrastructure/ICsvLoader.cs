using PropertyAnalyser.Models;
using System.Collections.Generic;

namespace PropertyAnalyser.Infrastructure
{
    public interface ICsvLoader
    {
        List<PropertyListing> LoadListings(string filePath);
    }
}