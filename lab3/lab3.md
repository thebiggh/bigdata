# Hands-On Lab #3 - Stream Processing

This lab will process data from a streaming source

[Reference](https://docs.databricks.com/spark/latest/structured-streaming/foreach.html#reuse-existing-batch-data-sources-with-foreachbatch)

## Task 1: Read streaming data from Azure Event Hub

### Configure Azure Event Hub
```python
import json

conf = {}
conf['eventhubs.connectionString'] = '<this will be provided>' 
conf['eventhubs.startingPosition'] = json.dumps({
  'offset': '-1',
  'seqNo': -1,
  'enqueuedTime': None,
  'isInclusive': True
})
```

### Read streaming data
```python
sdf = spark.readStream.format('eventhubs').options(**conf).load()
sdf = sdf.withColumn('body', sdf.body.cast('string'))
```

## Task 2: Display data stream in Databricks

```python
sdf = sdf.select(
  get_json_object(sdf.body, '$.created_at').cast(TimestampType()).alias('created_at'),
  get_json_object(sdf.body, '$.keyword').alias('keyword'),
  get_json_object(sdf.body, '$.sentiment').alias('sentiment')
).withWatermark('created_at', '60 minutes')
display(sdf)
```

## Task 3: Apply temporal window to data and calculate aggregates

```python
sdf = sdf.groupBy(window(sdf.created_at, '60 minutes'), sdf.keyword).agg({'sentiment' : 'avg'}).withColumnRenamed('avg(sentiment)', 'sentiment')
sdf = sdf.select(sdf.window.start.alias('timestamp'), sdf.keyword, sdf.sentiment)
```

## Task 4: Write data stream to Databricks Delta Table

```python
sdf.writeStream.format('delta').outputMode('append').option('checkpointLocation', '///sentiment_hourly_checkpoint').table('sentiment_hourly')
```

**Now ready for [Lab4](../lab4/lab4.md)**