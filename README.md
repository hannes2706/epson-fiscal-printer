# Epson Fiscal Printer

This epson fiscal printer is a small library to print fiscal invoices and non fiscal documents using C#.  
I have tested this library with the Epson FP81.    


> Currently the api only works with the default port: 9100

# Print invoice

```c#
EpsonFiscalPrinter epsonFiscalPrinter = new EpsonFiscalPrinter("<ip_address>");

epsonFiscalPrinter.BeginInvoice();
epsonFiscalPrinter.AddProduct("Your product", 21.9m);
epsonFiscalPrinter.AddProduct("Another product", 5m);
epsonFiscalPrinter.EndInvoice(PaymentType.CASH);

epsonFiscalPrinter.Print();
```

# Print document

```c#
EpsonFiscalPrinter epsonFiscalPrinter = new EpsonFiscalPrinter("<ip_address>");
epsonFiscalPrinter.BeginDocument();
epsonFiscalPrinter.AddTextToDocument("Hello world!");
epsonFiscalPrinter.AddTextToDocument("Here you can write whatever you want");
epsonFiscalPrinter.EndDocument();

epsonFiscalPrinter.Print();        
```

# Payment types
```c#
PaymentType.CASH  
PaymentType.CREDIT_CARD

epsonFiscalPrinter.Print();        
```
