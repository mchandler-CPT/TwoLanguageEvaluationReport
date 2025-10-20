using CsvHelper;
using PropertyAnalyser.Models;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PropertyAnalyser.Infrastructure
{
    /// <summary>
    /// Handles loading property listings from a CSV file.
    /// </summary>
    public class CsvLoader: ICsvLoader
    {
        /// <summary>
        /// Reads a CSV file from the given path and returns a list of listings.
        /// </summary>
        /// <param name="filePath">The full path to the input CSV file.</param>
        /// <returns>A list of PropertyListing objects.</returns>
        public List<PropertyListing> LoadListings(string filePath)
        {
            // Using statements ensure that the file streams are properly closed and disposed of.
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                // CsvHelper automatically maps columns to the properties of the PropertyListing record.
                var records = csv.GetRecords<PropertyListing>().ToList();
                return records;
            }
        }
    }
}