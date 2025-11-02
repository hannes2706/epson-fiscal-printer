using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace epson_fiscal_printer
{
    public class EpsonFiscalPrinterResponse
    {
        public string Code { get; set; }

        public bool IsSuccess { get; set; }

        public string Status { get; set; }

        public Dictionary<string, string> AdditionalInfo { get; set; } = new Dictionary<string, string>();


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

        public DateOnly? FiscalReceiptDate =>
            DateOnly.TryParseExact(FiscalReceiptDateRaw, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d : null;

        public string? FiscalReceiptTimeRaw => Get("fiscalReceiptTime");

        public TimeOnly? FiscalReceiptTime =>
            TimeOnly.TryParseExact(FiscalReceiptTimeRaw, "HH:mm",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var t)
                ? t : null;

        public DateTime? FiscalReceiptDateTime
        {
            get
            {
                if (FiscalReceiptDate is { } d && FiscalReceiptTime is { } t)
                {
                    var dt = d.ToDateTime(t);
                    return DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
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
