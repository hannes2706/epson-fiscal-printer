# Epson Fiscal Printer

A C# library for printing fiscal receipts, refunds, and non-fiscal documents with Epson fiscal printers. Originally tested with the Epson FP81. The library targets .NET 6.

## Connect to a printer

Import `epson_fiscal_printer` and pass the printer's IP address or hostname to `EpsonFiscalPrinter`. The library sends SOAP XML using HTTP POST to:

```text
http://<host>/cgi-bin/fpmate.cgi?devid=local_printer
```

Pass a host without a URL scheme or path, for example `192.168.1.100`. HTTP uses port 80 by default; an explicit port can be included as `192.168.1.100:8080` if your printer is configured for it. The library does not use raw TCP port 9100.

Each example below can be used in a console application that references the library. Replace the example IP address with your printer's address. `Print()` is asynchronous and must be awaited.

## Print a fiscal receipt

Start a receipt, add products, and finish it with a payment type and the amount tendered. `AddProduct` prints one unit per call; its optional `department` argument defaults to `1`. Choose the department configured on your printer for the product.

```csharp
using epson_fiscal_printer;

var printer = new EpsonFiscalPrinter("192.168.1.100");

printer.BeginInvoice();
printer.AddProduct("Your product", 21.90m, department: 1);
printer.AddProduct("Another product", 5.00m, department: 1);
printer.EndInvoice(PaymentType.CASH, givenMoney: 26.90m, message: "Thank you!");

var response = await printer.Print();

if (response.IsSuccess)
{
    Console.WriteLine($"Receipt: {response.FiscalReceiptNumber}");
    Console.WriteLine($"Amount: {response.FiscalReceiptAmount}");
}
else
{
    Console.WriteLine($"Printer error: {response.Code}, status: {response.Status}");
    foreach (var entry in response.AdditionalInfo)
    {
        Console.WriteLine($"{entry.Key}: {entry.Value}");
    }
}
```

`EndInvoice` requires `givenMoney`; its optional message defaults to `"Thank you"`. For a card payment, use `PaymentType.CREDIT_CARD` with the payment amount.

## Print a refund

Supply the original receipt's Z report number, document number, and date. Z report and document numbers are padded to at least four characters. The optional `fiscalSerialNo` selects the `REFUND` reference format; without it, the reference uses `RESO MERCE`.

```csharp
using epson_fiscal_printer;

var printer = new EpsonFiscalPrinter("192.168.1.100");

printer.BeginRefundInvoice(
    zReportNumber: "12",
    docNumber: "345",
    docDate: new DateTime(2026, 10, 14));
printer.AddRefundItem("Returned product", 21.90m, department: 1);
printer.AddRefundItem("Another returned product", 5.00m, department: 1);
printer.EndRefundInvoice(PaymentType.CASH, message: "RIMBORSO");

var response = await printer.Print();
Console.WriteLine($"Success: {response.IsSuccess}, code: {response.Code}");
```

`AddRefundItem` requires a department. `EndRefundInvoice` uses the sum of the refund item prices as the payment amount; its message defaults to `"RIMBORSO"`.

## Print a non-fiscal document

```csharp
using epson_fiscal_printer;

var printer = new EpsonFiscalPrinter("192.168.1.100");

printer.BeginDocument();
printer.AddTextToDocument("Hello world!", FontType.LARGE);
printer.AddTextToDocument("Thank you for visiting");
printer.EndDocument();

var response = await printer.Print();
Console.WriteLine($"Success: {response.IsSuccess}, code: {response.Code}");
```

`AddTextToDocument` defaults to `FontType.NORMAL`. Document text is inserted directly into an XML attribute: escape XML special characters in dynamic text, for example with `System.Security.SecurityElement.Escape(text)`. Product names and invoice/refund closing messages are escaped by the library.

## Payment types and fonts

| Payment type | Value |
| --- | --- |
| `PaymentType.CASH` | `0` |
| `PaymentType.CREDIT_CARD` | `2` |

| Font | Value |
| --- | --- |
| `FontType.NORMAL` | `1` |
| `FontType.BOLD` | `2` |
| `FontType.LARGE` | `3` |
| `FontType.BOLD_AND_LARGE` | `4` |

## Responses and errors

`Print()` returns a `Task<EpsonFiscalPrinterResponse>`. Check `IsSuccess` before treating a print as successful.

| Property | Meaning |
| --- | --- |
| `IsSuccess` | Success flag from the printer response. |
| `Code`, `Status` | Printer response code and status. |
| `AdditionalInfo` | Fields named in the response's `elementList`, plus concatenated additional-info text under `raw` when present. |
| `RawResponse` | Original response body for normal printer responses and HTTP error responses. |
| `FiscalReceiptNumber` | Receipt number as a nullable string. |
| `FiscalReceiptAmount` | Nullable decimal; accepts the printer's comma decimal separator. |
| `FiscalReceiptDateRaw`, `FiscalReceiptDate` | Original date text and nullable `DateOnly`, parsed using the current culture. |
| `FiscalReceiptTimeRaw`, `FiscalReceiptTime` | Original time text and nullable `TimeOnly`, parsed from hour/minute formats. |
| `ReceiptISODateTimeRaw`, `FiscalReceiptDateTime` | Original `receiptISODateTime` and nullable `DateTime`, parsed from `yyyyMMdd'T'HHmmss`. |
| `ZRepNumber`, `ZRepNumberInt` | Z report number as a nullable string or integer. |

Typed values are `null` when absent or unparseable. `FiscalReceiptDateTime` uses `receiptISODateTime`; it does not combine the separate date and time fields.

For an unsuccessful HTTP status, `Print()` returns `IsSuccess = false`, `Status = "NETWORK_ERROR"`, and the HTTP status name in `Code`. Connection failures and timeouts throw exceptions; the HTTP timeout is 50 seconds. Invalid XML also throws. Well-formed XML without a `response` element returns `IsSuccess = false`, `Status = "INVALID RESPONSE"`, and `Code = "-1"`.

You can parse an existing XML response without contacting a printer using `printer.ParseResponse(xml)`. Inspect the generated request before sending it with `printer.Data` or `printer.ToString()` after the corresponding `End...` call. Each `Begin...` call starts a new document and replaces the previous request data.

## Build and test

From the repository root, with the .NET 6 SDK installed:

```sh
dotnet restore epson-fiscal-printer.sln
dotnet build epson-fiscal-printer.sln --configuration Release --no-restore
dotnet test EpsonFiscalPrinterTest/EpsonFiscalPrinterTest.csproj --configuration Release --no-build --no-restore
```

Target the test project explicitly: the library's xUnit reference also causes solution-wide test discovery to attempt a test run against the library. The existing three tests exercise response parsing without a physical printer; they do not verify hardware printing.
