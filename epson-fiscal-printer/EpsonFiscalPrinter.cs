using System.Text;
using System.Xml.Linq;

namespace epson_fiscal_printer
{
    public class EpsonFiscalPrinter
    {
        private string _data;
        private string _host;

        public EpsonFiscalPrinter(string host)
        {
            _host = $"http://{host}/cgi-bin/fpmate.cgi?devid=local_printer";
        }

        public void BeginInvoice()
        {
            _data += "<printerFiscalReceipt><beginFiscalReceipt operator=\"10\" />";
        }

        public void AddProduct(string name, decimal price, int department = 1)
        {
            _data += $"<printRecItem operator=\"10\" description=\"{name}\" quantity=\"1\" unitPrice=\"{price}\" department=\"{department}\" justification=\"1\" />";
        }

        public void EndInvoice(PaymentType paymentType, string message = "Thank you")
        {
            _data += $"<printRecTotal operator=\"10\" description=\"PAGAMENTO\" payment=\"0\" paymentType=\"{(int)paymentType}\" index=\"1\" justification=\"1\" />";
            _data += $"<printRecMessage operator=\"10\" messageType=\"3\" index=\"1\" font=\"4\" message=\"{message}\" />";
            _data += $"<endFiscalReceipt operator=\"10\" /></printerFiscalReceipt>";

            _data = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                    "<s:Body>" +
                    _data +
                    "</s:Body>" +
                    "</s:Envelope>";
        }

        public void BeginDocument()
        {
            _data += "<printerNonFiscal><beginNonFiscal operator=\"10\" />";
        }

        public void AddTextToDocument(string text, FontType fontType = FontType.NORMAL)
        {
            _data += $"<printNormal  operator=\"10\" font=\"{(int)fontType}\" data=\"{text}\" />";
        }

        public void EndDocument()
        {
            _data += "<endNonFiscal operator=\"10\" /></printerNonFiscal>";

            _data = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\">" +
                    "<s:Body>" +
                    _data +
                    "</s:Body>" +
                    "</s:Envelope>";
        }

        public async Task<EpsonFiscalPrinterResponse> Print()
        {
            StringContent content = new StringContent(_data, Encoding.UTF8, "text/xml");

            HttpClient httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(50);            
            HttpResponseMessage response = await httpClient.PostAsync(_host, content);

            if (!response.IsSuccessStatusCode) {
                var epsonResonse = new EpsonFiscalPrinterResponse();
                epsonResonse.IsSuccess = false;
                epsonResonse.Code = response.StatusCode.ToString();
                epsonResonse.Status = await response.Content.ReadAsStringAsync();
                return epsonResonse;
            }

            return ParseResponse(await response.Content.ReadAsStringAsync());
        }

        public EpsonFiscalPrinterResponse ParseResponse(string response)
        {
            XDocument xmlDoc = XDocument.Parse(response);


            XElement responseElement = xmlDoc.Descendants("response").FirstOrDefault();

            var epsonResponse = new EpsonFiscalPrinterResponse();

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
            return _data;
        }
    }
}
