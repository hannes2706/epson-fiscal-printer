using System.Net;

namespace epson_fiscal_printer
{
    public class EpsonFiscalPrinter
    {
        private string _data;
        private string _host;
        private int _paymentType;

        /**
            * paymentType = 0 cash
            * paymentType = 2 creditcard
        **/
        public EpsonFiscalPrinter(string host, int paymentType)
        {
            _host = $"http://{host}/cgi-bin/fpmate.cgi?devid=local_printer&timeout=500";
            _paymentType = paymentType;
        }

        public void BeginInvoice()
        {
            _data += "<printerFiscalReceipt><beginFiscalReceipt operator=\"10\" />";
        }

        public void AddProduct(string name, decimal price, int department = 1)
        {
            _data += $"<printRecItem operator=\"10\" description=\"{name}\" quantity=\"1\" unitPrice=\"{price}\" department=\"{department}\" justification=\"1\" />";
        }

        public void EndInvoice(string message = "Thank you")
        {

            _data += $"<printRecTotal operator=\"10\" description=\"PAGAMENTO\" payment=\"0\" paymentType=\"{_paymentType}\" index=\"0\" justification=\"1\" />";
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

        public void AddTextToDocument(string text)
        {
            _data += $"<printNormal  operator=\"10\" font=\"4\" data=\"{text}\" />";
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

        public void Print()
        {
            HttpWebRequest request = (HttpWebRequest) WebRequest.Create(_host);
            byte[] bytes;
            bytes = System.Text.Encoding.ASCII.GetBytes(_data);
            request.ContentType = "text/xml; encoding='utf-8'";
            request.ContentLength = bytes.Length;
            request.Method = "POST";
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(bytes, 0, bytes.Length);
            requestStream.Close();
            HttpWebResponse response;

            response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream responseStream = response.GetResponseStream();
                string responseStr = new StreamReader(responseStream).ReadToEnd();
                Console.WriteLine("Response " + responseStr);
            }
        }
    }
}
