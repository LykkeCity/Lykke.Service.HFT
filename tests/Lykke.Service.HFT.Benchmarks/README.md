# HFT Client Benchmarks

## Settings file

Add the following json file ```appsettings.bm.json``` to the root of the benchmark test project or to the output folder.

```json
{
  "HftUrl": "http://localhost:5000",
  "ApiKey": "<MY-API-KEY>"
}
```

## Remarks

- Benchmarks will only run in Release build
- Outcome is dependant on local latency so the results can only be used for relative checks. For the real performance check Microsoft Azure appinsights.