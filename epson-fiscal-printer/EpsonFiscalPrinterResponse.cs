using System.Globalization;

namespace epson_fiscal_printer
{
    public class EpsonFiscalPrinterResponse
    {
        public string Code { get; set; }

        public bool IsSuccess { get; set; }

        public string Status { get; set; }

        public Dictionary<string, string> AdditionalInfo { get; set; } = new Dictionary<string, string>();

        public string RawResponse { get; set; }

        private string? Get(string key) => AdditionalInfo.TryGetValue(key, out var v) ? v?.Trim() : null;


        public string? FiscalReceiptNumber => Get("fiscalReceiptNumber");

        public decimal? FiscalReceiptAmount
        {
            get
            {
                if (AdditionalInfo.TryGetValue("fiscalReceiptAmount", out var v))
                {
                    var normalized = v.Replace(',', '.'); // Epson uses comma for decimals
                    if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
                    {
                        return amount;
                    }
                }

                return null;
            }
        }

        public string? FiscalReceiptDateRaw => Get("fiscalReceiptDate");

        public DateOnly? FiscalReceiptDate => DateOnly.TryParse(FiscalReceiptDateRaw, out var d) ? d : null;

        string[] timeFormats = { "HH:mm", "H:mm", "HH:m", "H:m" };

        public string? FiscalReceiptTimeRaw => Get("fiscalReceiptTime");

        public TimeOnly? FiscalReceiptTime => TimeOnly.TryParseExact(FiscalReceiptTimeRaw, timeFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ? t : null;

        public string? ReceiptISODateTimeRaw => Get("receiptISODateTime");

        public DateTime? FiscalReceiptDateTime
        {
            get
            {
                var iso = ReceiptISODateTimeRaw?.Trim();
                
                if (!string.IsNullOrWhiteSpace(iso) && DateTime.TryParseExact(iso, "yyyyMMdd'T'HHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                {
                    return dt;
                }

                return null;
            }
        }

        public string? ZRepNumber => Get("zRepNumber");

        public int? ZRepNumberInt =>
            int.TryParse(ZRepNumber, out var n) ? n : null;


        public EpsonFiscalPrinterResponse()
        {
        }
    }
}
