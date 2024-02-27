# Demo .NET Rate Limit



## Install k6

```bash
winget install k6 --source winget
```

Check the installation
```bash
k6 --version
```


## Run Api

```bash
dotnet run -p ./src
```


## Run tests

### Run get tests

#### `Fixed window`
```pwsh
k6 run ./tests/fixed-window-test.js
```

#### `Sliding window`

```pwsh
k6 run ./tests/sliding-window-test.js
```
[Explanations](#sliding-window-explanation)

#### `token-bucket`

```pwsh
k6 run ./tests/token-bucket-test.js
```

#### `concurrency`

```pwsh
k6 run ./tests/concurrency-test.js
```

#### `limited-by-ip-test`

```pwsh
k6 run ./tests/limited-by-ip-test.js
```

#### `demo` (generic tests)

```pwsh
k6 run ./tests/demo-test.js
```


## Explanations

### Sliding window explanation

* Limit: 50 requests per 20 seconds
* Segment: 10 seconds

***Stage 1***
```
Remaining requests: 50
Started

|         20s       |
| 0 (10s) | 0 (10s) | 0 (10s) | 0 (10s) | 0 (10s) | 0 (10s) |
              |
              |
       current segment
```

***Stage 2***
```
Remaining requests: 50
Received 20 requests

|         20s       |
| 0 (10s) | 20 (10s)| 0 (10s) | 0 (10s) | 0 (10s) | 0 (10s) |
              |
              |
       current segment

Remaining requests: 30
```

***Stage 3***
```
Remaining requests: 30
Slided to the next segment

          |         20s       |
| 0 (10s) | 20 (10s)| 0 (10s) | 0 (10s) | 0 (10s) | 0 (10s) |
                       |
                       |
                current segment

Remaining requests: 30
```

***Stage 4***
```
Remaining requests: 30
Received 25 requests

          |         20s       |
| 0 (10s) | 20 (10s)| 25 (10s)| 0 (10s) | 0 (10s) | 0 (10s) |
                       |
                       |
                current segment

Remaining requests: 5
```

***Stage 5***
```
Remaining requests: 30
Slided to the next segment

                    |         20s       |
| 0 (10s) | 20 (10s)| 25 (10s)| 0 (10s) | 0 (10s) | 0 (10s) |
                                 |
                                 |
                          current segment

Remaining requests: 25
```

***Stage 6***
```
Remaining requests: 25
Received 30 requests

                    |         20s        |
| 0 (10s) | 20 (10s)| 25 (10s)| 30 (10s) | 0 (10s) | 0 (10s) |
                                 |
                                 |
                          current segment

Remaining requests: 0
Exceeded: 25
Rejected: 5
```




## References

- [Offical documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-8.0#rate-limiter-algorithms)
