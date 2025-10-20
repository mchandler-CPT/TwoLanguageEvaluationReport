using System;
using System.Collections.Generic;
using System.Linq;
using PropertyAnalyser.Models;

namespace PropertyAnalyser.Services
{
    public class AnalysisService: IAnalysisService
    {
        private const double YIELD_THRESHOLD = 7.0;

        public List<SuburbReport> AnalyzeProperties(List<PropertyListing> listings)
        {
            var highYieldProperties = listings
                .Select(p => new
                {
                    Listing = p,
                    // We need these raw calculated values for our new metrics
                    RentalYield = (p.Price > 0) ? (double)(p.NetAnnualIncome / p.Price) * 100 : 0.0,
                    PricePerSqM = (p.GrossLettableArea > 0) ? (double)(p.Price / (decimal)p.GrossLettableArea) : 0.0
                })
                .Where(p => p.RentalYield > YIELD_THRESHOLD)
                .ToList(); // We materialize the list to use it multiple times

            var topSuburbs = highYieldProperties
                .AsParallel()
                .GroupBy(p => p.Listing.Suburb)
                .Select(group => new SuburbReport
                {
                    Name = group.Key,
                    PropertyCount = group.Count(),
                    
                    AverageYield = Math.Round(group.Average(p => p.RentalYield), 2),
                    AveragePricePerSqM = Math.Round(group.Average(p => p.PricePerSqM), 2),
                    
                    // Pass the list of values for each group to our new helper methods
                    MedianPricePerSqM = Math.Round(CalculateMedian(group.Select(p => p.PricePerSqM)), 2),
                    StdDevYield = Math.Round(CalculateStdDev(group.Select(p => p.RentalYield)), 2)
                })
                .OrderByDescending(s => s.AverageYield)
                .Take(5)
                .ToList();

            return topSuburbs;
        }

        // --- NEW HELPER METHOD ---
        private double CalculateMedian(IEnumerable<double> values)
        {
            if (!values.Any()) return 0;

            var sortedValues = values.OrderBy(n => n).ToList();
            var count = sortedValues.Count;
            var mid = count / 2;

            if (count % 2 == 0)
            {
                // Even number of items, get average of the two middle ones
                return (sortedValues[mid - 1] + sortedValues[mid]) / 2;
            }
            
            // Odd number of items, just return the middle one
            return sortedValues[mid];
        }

        private double CalculateStdDev(IEnumerable<double> values)
        {
            if (values.Count() <= 1) return 0;

            var avg = values.Average();
            var sumOfSquares = values.Sum(val => Math.Pow(val - avg, 2));
            
            // We use (n-1) for sample standard deviation, which is standard practice.
            return Math.Sqrt(sumOfSquares / (values.Count() - 1));
        }
    }
}