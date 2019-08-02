# Hands-On Lab #1 - Data Storage

The goals for this session are to understand data storage options available to us and to ingest data so that it can be processed.

## Task 1: Create Storage Account
* Log into Microsoft Azure 
* Create a resource group for labs (e.g. rg-bigdata-[name])
* Create an Azure Storage Account (e.g. stgbigdata[name])

**TODO: Put some images here**

* Make sure the storage account is configured for HTTPS only and a Hierarchical Namespace filesystem (i.e. Data Lake Gen2 Storage).

## Task 2: Download and configure Azure Storage Explorer

**TODO: Need the URL for the download link**

* Download and install the software
* Configure connection to Storage Account
* Create a default filesystem in the Storage Account
* Setup default POSIX security settings

## Task 3: Create an Azure Data Factory

**TODO: Need screenshots**

* Open Azure Portal again
* Create an Azure Data Factory service
* View the service and click the *Author & Monitor* link
* Create a pipeline
* Add a copy file shape to the pipeline and configure

**TODO: Need settings and screenshot**

* Point the copy file at **put URL here**
* Execute the pipeline and copy a file to the datalake

## Task 4: Verify the file as been copied

* View the **Monitor** page in Azure Data Factory
* Verify the pipeline has fully executed and was successful
* Open Azure Storage Explorer and browse to output location
* Verify the file has been created

**Now ready for Lab2**