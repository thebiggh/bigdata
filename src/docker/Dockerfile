FROM ubuntu:18.04

ENV SPARK_VERSION spark-2.4.4
ENV SPARK_FILE ${SPARK_VERSION}-bin-without-hadoop
ENV HADOOP_VERSION hadoop-3.2.1

RUN apt update \
 && apt install -y -qq curl openjdk-8-jdk-headless python3 python3-pip \
 && pip3 install -q jupyter \
 && pip3 install -q pandas \
 && pip3 install -q scikit-learn \
 && pip3 install -q matplotlib \
 && pip3 install -q sql_magic

 RUN curl -o ${SPARK_VERSION}.tgz https://www-eu.apache.org/dist/spark/${SPARK_VERSION}/${SPARK_FILE}.tgz \
 && tar xzf ${SPARK_VERSION}.tgz \
 && rm ${SPARK_VERSION}.tgz

 RUN curl -o ${HADOOP_VERSION}.tar.gz https://www-eu.apache.org/dist/hadoop/common/${HADOOP_VERSION}/${HADOOP_VERSION}.tar.gz \
 && tar xzf ${HADOOP_VERSION}.tar.gz \
 && rm ${HADOOP_VERSION}.tar.gz 

ENV JAVA_HOME /usr/lib/jvm/java-8-openjdk-amd64
ENV SPARK_HOME /${SPARK_FILE}
ENV HADOOP_HOME /${HADOOP_VERSION}
ENV PATH ${HADOOP_HOME}/bin:${SPARK_HOME}/bin:${PATH}

ENV PYSPARK_DRIVER_PYTHON jupyter
ENV PYSPARK_DRIVER_PYTHON_OPTS 'notebook --ip=0.0.0.0 --port=8080 --allow-root --no-browser'
ENV PYTHON_DIR_PATH=$SPARK_HOME/python/
ENV PY4J_PATH=$SPARK_HOME/python/lib/py4j-0.10.7-src.zip
ENV PYTHONPATH=$PYTHON_DIR_PATH:$PY4J_PATH

RUN ln -s /usr/bin/python3 /usr/bin/python 

EXPOSE 8080

COPY start.sh .

WORKDIR /notebooks

CMD ["/bin/bash", "/start.sh"]
