# Testing

## Running tests

Run all tests with:

```powershell
dotnet test .\CodyNET.Tests\CodyNET.Tests.csproj
```

## Single-step CPU tests

CodyNET uses the [65x02 SingleStepTests](https://github.com/SingleStepTests/65x02) by Thomas Harte et al., licensed under MIT.

To run these tests, download the WDC65C02 definitions and unpack them under `CodyNET.Tests/testdata`, so that this path exists:

```text
CodyNET.Tests/testdata/wdc65c02/v1/*.json
```

The project copies everything below `wdc65c02/` into the test output directory so the tests can run from the compiled `bin` folder.

Helper scripts are available:

```powershell
.\CodyNET.Tests\testdata\download-testdata.ps1
```

```bash
./CodyNET.Tests/testdata/download-testdata.sh
```

```bat
.\CodyNET.Tests\testdata\download-testdata.bat
```
