using Xunit;
using System.Collections.Generic;
using PropertyAnalyser.Models;
using PropertyAnalyser.Services;
using System.Linq;

namespace PropertyAnalyser.Tests
{
    public class AnalysisServiceTests
    {
        [Fact]
        public void AnalyzeProperties_Should_ReturnCorrectlyRankedSuburb()
        {
            // --- ARRANGE ---
            var testListings = new List<PropertyListing>
            {
                // High-Yield Suburb: "Goodwood" (10% yield)
                // PricePerSqM = 1,000,000 / 100 = 10,000
                new PropertyListing { Suburb = "Goodwood", Price = 1000000, NetAnnualIncome = 100000, GrossLettableArea = 100 },
                new PropertyListing { Suburb = "Goodwood", Price = 1000000, NetAnnualIncome = 100000, GrossLettableArea = 100 },

                // Low-Yield Suburb: "Clifton" (5% yield, below our 7% threshold)
                new PropertyListing { Suburb = "Clifton", Price = 10000000, NetAnnualIncome = 500000, GrossLettableArea = 300 },
                
                // High-Yield Suburb: "Observatory" (8% yield)
                // PricePerSqM = 1,000,000 / 100 = 10,000
                new PropertyListing { Suburb = "Observatory", Price = 1000000, NetAnnualIncome = 80000, GrossLettableArea = 100 }
            };

            var analysisService = new AnalysisService();

            // --- ACT ---
            var results = analysisService.AnalyzeProperties(testListings);
            var firstResult = results.First(); // Get the top-ranked suburb

            // --- ASSERT ---

            // Check that Clifton was filtered out
            Assert.Equal(2, results.Count);

            // Check that Goodwood is #1
            Assert.Equal("Goodwood", firstResult.Name);

            // Check the existing average calculation
            Assert.Equal(10.0, firstResult.AverageYield);

            // --- ADD THESE NEW ASSERTIONS ---

            // Check the Median calculation. 
            // The PricePerSqM values for Goodwood are [10000, 10000]. The median is 10000.
            Assert.Equal(10000.0, firstResult.MedianPricePerSqM);

            // Check the Standard Deviation calculation.
            // The Yield values for Goodwood are [10.0, 10.0]. The deviation is 0.
            Assert.Equal(0.0, firstResult.StdDevYield);
        }
    
        [Fact]
        public void AnalyzeProperties_WithEmptyList_ReturnsEmptyReport()
        {
            // --- ARRANGE ---
            var emptyListings = new List<PropertyListing>();
            var analysisService = new AnalysisService();

            // --- ACT ---
            var results = analysisService.AnalyzeProperties(emptyListings);

            // --- ASSERT ---
            // The service should not crash and should return an empty list.
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public void AnalyzeProperties_WithZeroPrice_ShouldNotCrash()
        {
            // --- ARRANGE ---
            // A Price of 0 would cause a DivideByZeroException if not handled.
            var testListings = new List<PropertyListing>
            {
                new PropertyListing { Suburb = "Goodwood", Price = 0, NetAnnualIncome = 100000, GrossLettableArea = 100 }
            };
            var analysisService = new AnalysisService();

            // --- ACT ---
            var results = analysisService.AnalyzeProperties(testListings);

            // --- ASSERT ---
            // Our logic checks for Price > 0, so this property's yield is 0.
            // It gets filtered out by the 7% threshold, resulting in an empty report.
            Assert.NotNull(results);
            Assert.Empty(results);
        }

        [Fact]
        public void AnalyzeProperties_WithZeroGLA_ShouldNotCrash()
        {
            // --- ARRANGE ---
            // A GLA of 0 would also cause a DivideByZeroException.
            var testListings = new List<PropertyListing>
            {
                new PropertyListing { Suburb = "Goodwood", Price = 1000000, NetAnnualIncome = 100000, GrossLettableArea = 0 }
            };
            var analysisService = new AnalysisService();

            // --- ACT ---
            var results = analysisService.AnalyzeProperties(testListings);
            var firstResult = results.First();

            // --- ASSERT ---
            // The service should run, and the calculated PricePerSqM should be 0.
            Assert.Single(results);
            Assert.Equal("Goodwood", firstResult.Name);
            Assert.Equal(0, firstResult.MedianPricePerSqM);
            Assert.Equal(0, firstResult.AveragePricePerSqM);
        }
    }
}