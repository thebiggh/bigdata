# Hands-On Lab #2 - Batch Processing

This lab will create an **Azure Databricks Workspace** that can be used for batch processing.

## Task 1: Create Databricks Workspace

**TODO: need setup screenshot**

* Login to Azure Portal
* Browse to resource group from Lab 1
* Create a new Databricks service. Ensure Notebooks are available.

**Screenshot**

* Once deployed, browse to the resource and click the **Open Databricks** button
* Login with Azure AD account
* In Databricks, create a new development cluster. Use the smallest size for most efficient price point.

**Screenshot**


## Task 2: Create a notebook and connect to Storage Account

* Open the workspace section in Databricks and create a new Notebook
* Open the new Notebook and begin editing
* Set configuration settings and update hadoopConfiguration
* Load the data source **(file type TBD)**
* Convert to a Data Frame and display

## Task 3: Perform some calculations on the data

* Load a reference file from the Data Lake
* Create a query to join data with the reference data
* Select the columns to display
* MapReduce **TODO: create example code**
* Reduce the Data Frame to an aggregated value
* Visualise the data by selecting a graphing option
