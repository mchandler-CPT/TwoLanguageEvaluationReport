import sys
import pandas as pd
import time
import tracemalloc

YIELD_THRESHOLD = 7.0

def process_analysis(input_path, output_path):
    """
    Loads, processes, and analyzes property data from a CSV file.
    
    Returns True on success, False on failure.
    """
    tracemalloc.start()

    try:
        # 1. Load Data
        print(f"Loading data from {input_path}...")
        df = pd.read_csv(input_path)
        print(f"Loaded {len(df):,} records.")

        start_time = time.time()

        # 2. Calculate Metrics
        df['RentalYield'] = (df['NetAnnualIncome'] / df['Price']) * 100
        df['PricePerSqM'] = df['Price'] / df['GrossLettableArea']

        # 3. Filter Properties
        high_yield_df = df[df['RentalYield'] > YIELD_THRESHOLD].copy()

        # 4. Aggregate by Suburb
        # We just add the new metrics to the aggregation list.
        suburb_summary = high_yield_df.groupby('Suburb').agg(
            averageYield=('RentalYield', 'mean'),
            medianPricePerSqM=('PricePerSqM', 'median'), # <-- NEW
            stdDevYield=('RentalYield', 'std'),        # <-- NEW
            averagePricePerSqM=('PricePerSqM', 'mean'),
            propertyCount=('ListingId', 'count')
        )

        # 5. Rank Suburbs and Select Top 5
        top_5_suburbs = suburb_summary.sort_values(by='averageYield', ascending=False).head(5)
        
        end_time = time.time()
        elapsed_ms = (end_time - start_time) * 1000
        print(f"✅ Analysis complete in {elapsed_ms:.2f} ms.")

        # 6. Format for JSON Output
        top_5_suburbs_dict = top_5_suburbs.reset_index().rename(columns={'Suburb': 'name'}).to_dict('records')
        
        # --- UPDATE THE ROUNDING LOOP ---
        for suburb in top_5_suburbs_dict:
            suburb['averageYield'] = round(suburb['averageYield'], 2)
            suburb['medianPricePerSqM'] = round(suburb['medianPricePerSqM'], 2) # <-- NEW
            suburb['stdDevYield'] = round(suburb['stdDevYield'], 2)         # <-- NEW
            suburb['averagePricePerSqM'] = round(suburb['averagePricePerSqM'], 2)

        report = {"topSuburbs": top_5_suburbs_dict}

        end_time = time.time()
        elapsed_ms = (end_time - start_time) * 1000
        print(f"✅ Analysis complete in {elapsed_ms:.2f} ms.")

        # Get the peak memory usage from tracemalloc
        current, peak = tracemalloc.get_traced_memory()
        peak_mb = peak / 1024.0 / 1024.0
        print(f"✅ Peak memory usage: {peak_mb:.2f} MB")
        tracemalloc.stop()
        
        pd.Series(report).to_json(output_path, indent=4)
        
        print(f"✅ Successfully wrote report to {output_path}")
        return True

    except FileNotFoundError:
        print(f"Error: The file '{input_path}' was not found.")
        return False
    except Exception as e:
        print(f"An unexpected error occurred: {e}")
        return False

def main():
    """
    Main entry point for the script.
    """
    if len(sys.argv) != 3:
        print("Error: Invalid arguments.")
        print("Usage: python property_analyser.py <input_file_path> <output_file_path>")
        return

    input_file = sys.argv[1]
    output_file = sys.argv[2]
    
    # Call the main processing function
    process_analysis(input_file, output_file)

if __name__ == "__main__":
    main()