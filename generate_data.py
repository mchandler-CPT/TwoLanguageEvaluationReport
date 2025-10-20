import csv
import random
import os

# --- Configuration ---
NUM_ROWS = 1_000_000
OUTPUT_FOLDER = 'data'
OUTPUT_FILE = os.path.join(OUTPUT_FOLDER, 'listings.csv')
# ---------------------

SUBURBS = [
    "Woodstock", "Salt River", "Observatory", "Mowbray", "Rondebosch",
    "Claremont", "Kenilworth", "Wynberg", "Plumstead", "Diep River",
    "Constantia", "Sea Point", "Green Point", "Camps Bay", "City Bowl"
]

def generate_row(listing_id):
    """Generates a single row of mock property data."""
    suburb = random.choice(SUBURBS)
    price = random.randint(1_500_000, 15_000_000)
    gla = random.randint(80, 1000)
    # Generate income with a realistic yield range (3% to 12%)
    net_annual_income = int(price * random.uniform(0.03, 0.12))
    address = f"{random.randint(1, 200)} {random.choice(['Main', 'Victoria', 'Long'])} Rd"

    return [listing_id, address, suburb, price, gla, net_annual_income]

def main():
    """Main function to generate the CSV file."""
    print(f"Generating {NUM_ROWS:,} rows of mock data...")
    
    # Ensure the output directory exists
    os.makedirs(OUTPUT_FOLDER, exist_ok=True)

    header = ['ListingId', 'Address', 'Suburb', 'Price', 'GrossLettableArea', 'NetAnnualIncome']

    with open(OUTPUT_FILE, 'w', newline='') as csvfile:
        writer = csv.writer(csvfile)
        writer.writerow(header)
        for i in range(1, NUM_ROWS + 1):
            writer.writerow(generate_row(i))
            if i % 100_000 == 0:
                print(f"  ...wrote {i:,} rows")

    print(f"\n✅ Successfully created '{OUTPUT_FILE}'")

if __name__ == "__main__":
    main()