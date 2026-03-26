using System.Diagnostics;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace epson_fiscal_printer
{
    public class EpsonFiscalPrinter
    {
        public string Data { get; private set; }
        private string _host;
        private decimal _totalRefundPrice = 0;

        public EpsonFiscalPrinter(string host)
        {
            _host = $"http://{host}/cgi-bin/fpmate.cgi?devid=local_printer";
        }

        public void BeginInvoice()
        {
            Data = "<printerFiscalReceipt><beginFiscalReceipt operator=\"10\" />";
        }

        public void AddProduct(string name, decimal price, int department = 1)
        {
            Data += $"<printRecItem operator=\"10\" description=\"{XmlEscape(name)}\" quantity=\"1\" unitPrice=\"{price.ToString(CultureInfo.InvariantCulture)}\" department=\"{department}\" justification=\"1\" />";
        }

        public void EndInvoice(PaymentType paymentType, decimal givenMoney, string message = "Thank you")
        {
            Data += $"<printRecTotal operator=\"10\" description=\"PAGAMENTO\" payment=\"{givenMoney.ToString(CultureInfo.InvariantCulture)}\" paymentType=\"{(int)paymentType}\" index=\"1\" justification=\"1\" />";
            Data += $"<printRecMessage operator=\"10\" messageType=\"3\" index=\"1\" font=\"4\" message=\"{XmlEscape(message)}\" />";
            Data += $"<endFiscalReceipt operator=\"10\" /></printerFiscalReceipt>";

            Data = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                    "<s:Body>" +
                    Data +
                    "</s:Body>" +
                    "</s:Envelope>";
        }

        public void BeginDocument()
        {
            Data = "";
            Data += "<printerNonFiscal><beginNonFiscal operator=\"10\" />";
        }

        public void AddTextToDocument(string text, FontType fontType = FontType.NORMAL)
        {
            Data += $"<printNormal operator=\"10\" font=\"{(int)fontType}\" data=\"{text}\" />";
        }

        public void EndDocument()
        {
            Data += "<endNonFiscal operator=\"10\" /></printerNonFiscal>";

            Data = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                    "<s:Body>" +
                    Data +
                    "</s:Body>" +
                    "</s:Envelope>";
        }

        public void BeginRefundInvoice(string zReportNumber, string docNumber, DateTime docDate, string fiscalSerialNo = "")
        {
            string paddedZReportNumber = zReportNumber.PadLeft(4, '0');
            string paddedDocNumber = docNumber.PadLeft(4, '0');

            string refundMessage = string.IsNullOrWhiteSpace(fiscalSerialNo)
                ? $"RESO MERCE N.{paddedZReportNumber}-{paddedDocNumber} del {docDate:dd-MM-yyyy}"
                : $"REFUND {paddedZReportNumber} {paddedDocNumber} {docDate:ddMMyyyy} {fiscalSerialNo}";

            _totalRefundPrice = 0;
            Data = $@"<printerFiscalReceipt>
            <printRecMessage operator=""10"" message=""{refundMessage}"" messageType=""4"" />
            <beginFiscalReceipt operator=""10""/>";
        }

        public void AddRefundItem(string name, decimal price, int department)
        {
            _totalRefundPrice += price;
            Data += $"<printRecRefund operator=\"10\" description=\"{XmlEscape(name)}\" quantity=\"1\" unitPrice=\"{price.ToString(CultureInfo.InvariantCulture)}\" department=\"{department}\" justification=\"1\" />";
        }

        public void EndRefundInvoice(PaymentType paymentType, string message = "RIMBORSO")
        {
            Data += $@"
                <printRecTotal
                description=""{XmlEscape(message)}""
                  operator=""10""
                  payment=""{_totalRefundPrice.ToString(CultureInfo.InvariantCulture)}""
                  paymentType=""{(int)paymentType}""
                  index=""1""/>
                <endFiscalReceipt operator=""10""/></printerFiscalReceipt>";

            Data = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                    "<s:Body>" +
                    Data +
                    "</s:Body>" +
                    "</s:Envelope>";
        }

        public async Task<EpsonFiscalPrinterResponse> Print()
        {
            StringContent content = new StringContent(Data, Encoding.UTF8, "text/xml");

            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(50);            
            HttpResponseMessage response = await httpClient.PostAsync(_host, content);

            string rawResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) {
                var epsonResponse = new EpsonFiscalPrinterResponse();
                epsonResponse.RawResponse = rawResponse;
                epsonResponse.IsSuccess = false;
                epsonResponse.Code = response.StatusCode.ToString();
                epsonResponse.Status = "NETWORK_ERROR";
                return epsonResponse;
            }

            return ParseResponse(rawResponse);
        }

        public EpsonFiscalPrinterResponse ParseResponse(string response)
        {
            XDocument xmlDoc = XDocument.Parse(response);


            XElement responseElement = xmlDoc.Descendants("response").FirstOrDefault();

            var epsonResponse = new EpsonFiscalPrinterResponse();
            epsonResponse.RawResponse = response;

            if (responseElement == null)
            {
                return new EpsonFiscalPrinterResponse() { IsSuccess = false, Status = "INVALID RESPONSE", Code = "-1" };
            }


            var successAttr = (string?)responseElement.Attribute("success");
            epsonResponse.IsSuccess = successAttr != null && bool.TryParse(successAttr, out var s) && s;

            epsonResponse.Status = (string?)responseElement.Attribute("status") ?? "";
            epsonResponse.Code = (string?)responseElement.Attribute("code") ?? "";

            var addInfoElement = responseElement.Element("addInfo");

            if (addInfoElement != null)
            {
                // Save raw additional info
                var raw = addInfoElement.Value;
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    epsonResponse.AdditionalInfo["raw"] = raw;
                }

                var elementListRaw = (string?)addInfoElement.Element("elementList") ?? "";
                var names = elementListRaw
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => n.Trim())
                    .Where(n => n.Length > 0)
                    .ToArray();

                foreach (var name in names)
                {
                    var val = addInfoElement.Element(name)?.Value;
                    if (val != null)
                    {
                        epsonResponse.AdditionalInfo[name] = val;
                    }
                }
            }


            return epsonResponse;
        }

        public override string ToString()
        {
            return Data;
        }

        private static string XmlEscape(string? s) => SecurityElement.Escape(s) ?? "";
    }
}