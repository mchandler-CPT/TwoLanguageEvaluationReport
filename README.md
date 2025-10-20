# C# vs. Python: A Comparative Analysis for Data Processing

This repository is a submission for the MSc in AI (University of Essex Online) computing assignment, "Evaluating the Development Models of Two Programming Languages."

The project consists of two identical console applications, one in C# and one in Python, that analyze a 1,000,000-row CSV file of property data. The accompanying 1000-word report analyzes the differences in performance, maintainability, and security.

*(Note: The 1M-row `listings.csv` is not committed to this repository due to file size limits and is included in the .gitignore).*

## How to Run the Applications

### 1. C# Application

The C# project is located in the `csharp/` directory and is managed by `PropertyAnalyserAssignment.sln`.

1.  **Navigate to the C# solution folder:**
    ```bash
    cd csharp
    ```
2.  **Run the application (from the solution folder):**
    ```bash
    dotnet run --project PropertyAnalyzer/PropertyAnalyser.csproj ../data/listings.csv ../data/csharp_summary.json
    ```

### 2. Python Application

The Python script is located in the `python/` folder.

1.  **Navigate to the project folder:**
    ```bash
    cd python
    ```
2.  **Install dependencies:**
    ```bash
    python -m pip install -r requirements.txt 
    ```
3.  **Run the application:**
    ```bash
    python property_analyser.py ../data/listings.csv ../data/python_summary.json
    ```

## Demonstration & Test Results

### C# Application & Test Results

Screenshot of the C# application running successfully:

`![C# App Results](./screenshots/csharp_app_run.png)`

Screenshot of the C# unit tests passing:

`![C# Test Results](./screenshots/csharp_test_results.png)`


### Python Application & Test Results

Screenshot of the Python application running successfully:

`![Python App Results](./screenshots/python_app_run.png)`

Screenshot of the Python unit tests passing:

`![Python Test Results](./screenshots/python_test_results.png)`