# Open Telemetry Extensions


## Run Jaeger trace collector

``` docker
docker run --name jaeger-otel  --rm -it -e COLLECTOR_OTLP_ENABLED=true -p 16686:16686 -p 4317:4317 -p 4318:4318  jaegertracing/all-in-one:latest
```

Check it on: `http://localhost:16686/search`
