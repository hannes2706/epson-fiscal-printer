using System;
using System.Collections.Generic;
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

        public string AdditionalInfo { get; set; }


        public EpsonFiscalPrinterResponse()
        {
        }
    }
}
