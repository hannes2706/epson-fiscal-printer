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
                                <response success=""true"" code=""SUCCESS"" status=""2"" /> 
                                </s:Body> 
                                </s:Envelope>";
        
            EpsonFiscalPrinter epsonFiscalPrinter = new EpsonFiscalPrinter("127.0.0.1");


            var expectedResponse = new EpsonFiscalPrinterResponse
            {
                Code = "SUCCESS",
                IsSuccess = true,
                Status = "2",
                AdditionalInfo = null
            };


            var parsedResponse = epsonFiscalPrinter.ParseResponse(response);

            Assert.Equal(parsedResponse.IsSuccess, expectedResponse.IsSuccess);
            Assert.Equal(parsedResponse.Status, expectedResponse.Status);
            Assert.Equal(parsedResponse.Code, expectedResponse.Code);
            Assert.Equal(parsedResponse.AdditionalInfo, expectedResponse.AdditionalInfo);
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
                Status = "0",
                AdditionalInfo = null
            };


            var parsedResponse = epsonFiscalPrinter.ParseResponse(response);

            Assert.Equal(parsedResponse.IsSuccess, expectedResponse.IsSuccess);
            Assert.Equal(parsedResponse.Status, expectedResponse.Status);
            Assert.Equal(parsedResponse.Code, expectedResponse.Code);
            Assert.Equal(parsedResponse.AdditionalInfo, expectedResponse.AdditionalInfo);
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
                Status = "21",
            };


            var parsedResponse = epsonFiscalPrinter.ParseResponse(response);

            Assert.Equal(parsedResponse.IsSuccess, expectedResponse.IsSuccess);
            Assert.Equal(parsedResponse.Status, expectedResponse.Status);
            Assert.Equal(parsedResponse.Code, expectedResponse.Code);
            Assert.Contains("lastCommand", parsedResponse.AdditionalInfo);
        }
    }
}