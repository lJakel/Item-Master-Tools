using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace Tools
{
    public class ProductionConsumer
    {
        public int? Isbn { get; set; }

        public int? Title { get; set; }

        public int? ContractID { get; set; }

        public int? Pagecount { get; set; }

        public int? TrimSize { get; set; }

        public int? PaperStock { get; set; }

        public int? PaperWeight { get; set; }

        public int? Format { get; set; }

        public int? PrintType { get; set; }

        public int? PrintSize { get; set; }

        public int? ClothColor { get; set; }

        public string MappingName { get; set; }

        public string Source { get; set; }

        public int? FileType { get; set; }

        public HttpPostedFileBase FileIn { get; set; }

        public string[] Mappings { get; set; }

        public string txtContractID { get; set; }


        public HtmlString FileScan { get; set; }

        public List<PodLineEntry> Entries { get; set; }

        public ProductionConsumer()
        {
            Mappings = new string[] { };
            
            FileScan = new HtmlString("<tr><td></td></tr>");
            Entries = new List<PodLineEntry>();
        }
    }

    public class ProductionUpload
    {

        public Dictionary<string, int?> Mapping { get; set; }
        public Dictionary<string, string[]> MappingHeaders { get; set; }

        public string ErrorMsg { get; set; }

        public string SettingsName { get; set; }

        public string ddlIsbn { get; set; }

        public string ddlTitle { get; set; }

        public string ddlContractID { get; set; }

        public string ddlNumberOfPages { get; set; }

        public string ddlTrimSize { get; set; }

        public string ddlPaperStock { get; set; }

        public string ddlPaperWeight { get; set; }

        public string ddlFormat { get; set; }

        public string ddlPrintType { get; set; }

        public string ddlFormatSize { get; set; }

        public string ddlClothColor { get; set; }

        public string Source { get; set; }

        public DataTable ResultsData { get; set; }

        public ProductionUpload()
        {
            MappingHeaders = new Dictionary<string, string[]>();
            ResultsData = new DataTable();

        }
    }
    public class PodLineEntry
    {
        public int LineNumber { get; set; }
        public object Isbn { get; set; }
        public object Title { get; set; }
        public object ContractID { get; set; }
        public object NumberOfPages { get; set; }
        public object TrimSize { get; set; }
        public object PaperStock { get; set; }
        public object PaperWeight { get; set; }
        public object Format { get; set; }
        public object PrintType { get; set; }
        public object FormatSize { get; set; }
        public object ClothColor { get; set; }

        public PodLineEntry()
        {

        }

    }
}