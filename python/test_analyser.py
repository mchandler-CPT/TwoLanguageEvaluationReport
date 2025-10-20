import pytest
import pandas as pd
import json
from property_analyser import process_analysis # Import the refactored function

@pytest.fixture
def mock_csv_file(tmp_path):
    """
    A pytest fixture. This function runs *before* the test.
    It creates a temporary mock CSV file and returns its path.
    """
    csv_path = tmp_path / "test_listings.csv"
    
    # Use the same test data as the C# unit test for a fair comparison
    test_data = {
        'ListingId': [1, 2, 3, 4],
        'Address': ['1 Main Rd', '2 Main Rd', '1 Ocean View', '1 High St'],
        'Suburb': ['Goodwood', 'Goodwood', 'Clifton', 'Observatory'],
        'Price': [1000000, 1000000, 10000000, 1000000],
        'GrossLettableArea': [100, 100, 300, 100],
        'NetAnnualIncome': [100000, 100000, 500000, 80000] # Yields: 10%, 10%, 5%, 8%
    }
    df = pd.DataFrame(test_data)
    df.to_csv(csv_path, index=False)
    return csv_path

def test_process_analysis_logic(mock_csv_file, tmp_path):
    """
    This test checks the core logic of the 'process_analysis' function.
    It uses the 'mock_csv_file' fixture to get a test file.
    """
    
    # --- ARRANGE ---
    input_path = str(mock_csv_file)
    output_path = str(tmp_path / "test_summary.json")

    # --- ACT ---
    success = process_analysis(input_path, output_path)

    # --- ASSERT ---
    # 1. Check that the function reported success
    assert success == True

    # 2. Read the JSON report that the function created
    with open(output_path, 'r') as f:
        report = json.load(f)
        results = report.get('topSuburbs')

    # 3. Verify the contents of the report
    
    # We expect 2 suburbs. "Clifton" (5% yield) should be filtered out.
    assert len(results) == 2

    # Get the top-ranked result ("Goodwood")
    first_result = results[0]
    assert first_result['name'] == 'Goodwood'
    
    # Check all the calculated values
    assert first_result['averageYield'] == 10.0
    
    # --- NEW ASSERTIONS ---
    # PricePerSqM values are [10000, 10000]. Median is 10000.
    assert first_result['medianPricePerSqM'] == 10000.0 
    
    # Yield values are [10.0, 10.0]. Std Dev is 0.
    assert first_result['stdDevYield'] == 0.0 
    # -------------------------
    
    assert first_result['propertyCount'] == 2

@pytest.fixture
def empty_csv_file(tmp_path):
    """Fixture for an empty CSV file (header only)."""
    csv_path = tmp_path / "empty.csv"
    pd.DataFrame(columns=[
        'ListingId', 'Address', 'Suburb', 'Price', 
        'GrossLettableArea', 'NetAnnualIncome'
    ]).to_csv(csv_path, index=False)
    return csv_path

@pytest.fixture
def zero_price_csv_file(tmp_path):
    """Fixture for a CSV with a Price of 0."""
    csv_path = tmp_path / "zero_price.csv"
    # --- ADD ListingId TO THIS DICTIONARY ---
    test_data = {
        'ListingId': [1], 
        'Suburb': ['Goodwood'], 
        'Price': [0], 
        'NetAnnualIncome': [100000], 
        'GrossLettableArea': [100]
    }
    pd.DataFrame(test_data).to_csv(csv_path, index=False)
    return csv_path

@pytest.fixture
def zero_gla_csv_file(tmp_path):
    """Fixture for a CSV with a GrossLettableArea of 0."""
    csv_path = tmp_path / "zero_gla.csv"
    # --- ADD ListingId TO THIS DICTIONARY ---
    test_data = {
        'ListingId': [1],
        'Suburb': ['Goodwood'], 
        'Price': [1000000], 
        'NetAnnualIncome': [100000], 
        'GrossLettableArea': [0]
    }
    pd.DataFrame(test_data).to_csv(csv_path, index=False)
    return csv_path

def test_with_empty_file(empty_csv_file, tmp_path):
    """Test that an empty input file produces an empty report without crashing."""
    # ARRANGE
    output_path = str(tmp_path / "empty_summary.json")
    
    # ACT
    success = process_analysis(str(empty_csv_file), output_path)

    # ASSERT
    assert success == True
    with open(output_path, 'r') as f:
        report = json.load(f)
    assert len(report['topSuburbs']) == 0

def test_with_zero_price(zero_price_csv_file, tmp_path):
    """Test how pandas handles division by zero for Price."""
    # ARRANGE
    output_path = str(tmp_path / "zero_price_summary.json")

    # ACT
    success = process_analysis(str(zero_price_csv_file), output_path)

    # ASSERT
    assert success == True
    with open(output_path, 'r') as f:
        report = json.load(f)
    # The yield is 'inf', so it passes the filter. The JSON will contain 'Infinity'.
    assert report['topSuburbs'][0]['averageYield'] is None # json.load converts 'Infinity' to None in some versions, or it could be a string 'Infinity'
    
def test_with_zero_gla(zero_gla_csv_file, tmp_path):
    """Test how pandas handles division by zero for GLA."""
    # ARRANGE
    output_path = str(tmp_path / "zero_gla_summary.json")
    
    # ACT
    success = process_analysis(str(zero_gla_csv_file), output_path)

    # ASSERT
    assert success == True
    with open(output_path, 'r') as f:
        report = json.load(f)
    # The PricePerSqM is 'inf'. The JSON will contain 'Infinity'.
    assert report['topSuburbs'][0]['averagePricePerSqM'] is None
    assert report['topSuburbs'][0]['medianPricePerSqM'] is None
