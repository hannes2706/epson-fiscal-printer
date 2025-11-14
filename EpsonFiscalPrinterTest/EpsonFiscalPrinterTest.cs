using epson_fiscal_printer;

namespace EpsonFiscalPrinterTest
{
    public class EpsonFiscalPrinterTest
    {
        [Fact]
        public void ParseResponse_ShouldReturnSuccess()
        {
            string response = @"<?xml version=""1.0"" encoding=""utf-8""?> 
                                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""> 
                                <s:Body> 
                                <response success=""true"" code=""SUCCESS"" status=""2"">
                                   <addInfo> 
                                    <elementList>lastCommand,printerStatus,fiscalReceiptNumber,fiscalReceiptAmount,fiscalReceiptDate, 
                                    fiscalReceiptTime,zRepNumber</elementList> 
                                    <lastCommand>74</lastCommand> 
                                    <printerStatus>20010</printerStatus> 
                                    <fiscalReceiptNumber>500</fiscalReceiptNumber> 
                                    <fiscalReceiptAmount>210,56</fiscalReceiptAmount> 
                                    <fiscalReceiptDate>10/4/2006</fiscalReceiptDate>
                                    <fiscalReceiptTime>12:34</fiscalReceiptTime>
                                    <zRepNumber>1</zRepNumber>
                                    </addInfo>
                                </response>
                                </s:Body> 
                                </s:Envelope>";
        
            EpsonFiscalPrinter epsonFiscalPrinter = new EpsonFiscalPrinter("127.0.0.1");


            var expectedResponse = new EpsonFiscalPrinterResponse
            {
                Code = "SUCCESS",
                IsSuccess = true,
                Status = "2",
                AdditionalInfo = new Dictionary<string, string>
                {
                    { "lastCommand", "74" },
                    { "printerStatus", "20010" },
                    { "fiscalReceiptNumber", "500" },
                    { "fiscalReceiptAmount", "210,56" },
                    { "fiscalReceiptDate", "10/4/2006" },
                    { "fiscalReceiptTime", "12:34" },
                    { "zRepNumber", "1" }
                }

            };


            var parsedResponse = epsonFiscalPrinter.ParseResponse(response);

            Assert.Equal(parsedResponse.IsSuccess, expectedResponse.IsSuccess);
            Assert.Equal(parsedResponse.Status, expectedResponse.Status);
            Assert.Equal(parsedResponse.Code, expectedResponse.Code);

            Assert.Equal(parsedResponse.FiscalReceiptNumber, expectedResponse.FiscalReceiptNumber);
            Assert.Equal(parsedResponse.FiscalReceiptAmount, expectedResponse.FiscalReceiptAmount);
            Assert.Equal(parsedResponse.FiscalReceiptDate, expectedResponse.FiscalReceiptDate);
            Assert.Equal(parsedResponse.FiscalReceiptTime, expectedResponse.FiscalReceiptTime);
            Assert.Equal(parsedResponse.FiscalReceiptDateTime, expectedResponse.FiscalReceiptDateTime);
            Assert.Equal(parsedResponse.ZRepNumber, expectedResponse.ZRepNumber);
        }

        [Fact]
        public void ParseResponse_ShouldReturnFalse_WithTimeout()
        {
            string response = @"<?xml version=""1.0"" encoding=""utf-8""?> 
                                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""> 
                                <s:Body> 
                                <response success=""false"" code=""LAN_TIME_OUT"" status=""0"" /> 
                                </s:Body> 
                                </s:Envelope>";

            EpsonFiscalPrinter epsonFiscalPrinter = new EpsonFiscalPrinter("127.0.0.1");


            var expectedResponse = new EpsonFiscalPrinterResponse
            {
                Code = "LAN_TIME_OUT",
                IsSuccess = false,
                Status = "0"
            };


            var parsedResponse = epsonFiscalPrinter.ParseResponse(response);

            Assert.Equal(parsedResponse.IsSuccess, expectedResponse.IsSuccess);
            Assert.Equal(parsedResponse.Status, expectedResponse.Status);
            Assert.Equal(parsedResponse.Code, expectedResponse.Code);
        }

        [Fact]
        public void ParseResponse_ShouldReturnFalse_WithAdditionalInfo()
        {
            string response = @"<?xml version=""1.0"" encoding=""utf-8""?> 
                                <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/""> 
                                    <s:Body>
                                        <response success=""false"" code=""PRINTER ERROR"" status=""21""> 
                                            <addInfo> 
                                                <elementList>lastCommand,printerStatus</elementList> 
                                                <lastCommand>80</lastCommand> 
                                                <printerStatus>00100</printerStatus> 
                                            </addInfo>
                                        </response>
                                    </s:Body> 
                                </s:Envelope>";

            EpsonFiscalPrinter epsonFiscalPrinter = new EpsonFiscalPrinter("127.0.0.1");


            var expectedResponse = new EpsonFiscalPrinterResponse
            {
                Code = "PRINTER ERROR",
                IsSuccess = false,
                Status = "21"
            };


            var parsedResponse = epsonFiscalPrinter.ParseResponse(response);

            Assert.Equal(parsedResponse.IsSuccess, expectedResponse.IsSuccess);
            Assert.Equal(parsedResponse.Status, expectedResponse.Status);
            Assert.Equal(parsedResponse.Code, expectedResponse.Code);
            Assert.Contains("lastCommand", parsedResponse.AdditionalInfo);
            Assert.Contains("printerStatus", parsedResponse.AdditionalInfo);
        }
    }
}