using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using System.Data.OleDb;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using OfficeOpenXml;
using System.Data.SqlClient;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using ModelService.ItemMaster.Models;
using ModelService;

namespace Tools
{
    internal class internalFunctions
    {
        private static string msss = System.Configuration.ConfigurationManager.ConnectionStrings["ImiLink"].ConnectionString;
        private static string mssql = System.Configuration.ConfigurationManager.ConnectionStrings["Amstitle"].ConnectionString;

        internal internalFunctions()
        {

        }

        internal Dictionary<string, string[]> getExcelHeaders(Stream fileSrc)
        {

            using (ExcelPackage ep = new ExcelPackage(fileSrc))
            {
                var wb = ep.Workbook.Worksheets;
                Dictionary<string, string[]> headers = new Dictionary<string, string[]>();
                foreach (var sht in wb)
                {
                    object[,] cols = (object[,])sht.Cells["1:1"].Value;
                    List<string> hdrs = new List<string>();
                    hdrs.Add(" ");
                    for (int x = 0; x < cols.Length; x++)
                    {
                        if (cols[0, x] != null) { hdrs.Add(cols[0, x].ToString()); }
                    }

                    headers.Add(sht.Name, hdrs.ToArray());
                }
                return headers;
            }
        }

        internal async Task<bool> SaveUpdateMapping(ProductionUpload mapModel)
        {
            var mappingDct = mapModel.Mapping;
            var savemdl = new ProductionConsumer()
            {
                Isbn = mappingDct["Isbn"],
                Title = mappingDct["Title"],
                ContractID = mappingDct["ContractID"],
                ClothColor = mappingDct["ClothColor"],
                Format = mappingDct["Format"],
                MappingName = mapModel.SettingsName,
                Pagecount = mappingDct["NumberOfPages"],
                PaperStock = mappingDct["PaperStock"],
                PaperWeight = mappingDct["PaperWeight"],
                PrintSize = mappingDct["FormatSize"],
                PrintType = mappingDct["PrintType"],
                TrimSize = mappingDct["TrimSize"],
                Source = mapModel.Source
            };
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["ImiLink"].ConnectionString;
            using (var conn = new SqlConnection(connStr))
            {
                var sqlx = new SqlCommand() { Connection = conn, CommandType = CommandType.StoredProcedure, CommandText = "dbo.SaveProductionMapping" };
                sqlx.Parameters.AddWithValue("@isbn", savemdl.Isbn);
                sqlx.Parameters.AddWithValue("@title", savemdl.Title);
                sqlx.Parameters.AddWithValue("@contractid", savemdl.ContractID);
                sqlx.Parameters.AddWithValue("@pages", savemdl.Pagecount);
                sqlx.Parameters.AddWithValue("@trimsize", savemdl.TrimSize);
                sqlx.Parameters.AddWithValue("@pprstock", savemdl.PaperStock);
                sqlx.Parameters.AddWithValue("@pprweight", savemdl.PaperWeight);
                sqlx.Parameters.AddWithValue("@print", savemdl.PrintType);
                sqlx.Parameters.AddWithValue("@format", savemdl.Format);
                sqlx.Parameters.AddWithValue("@formatsize", savemdl.PrintSize);
                sqlx.Parameters.AddWithValue("@src", savemdl.Source);
                sqlx.Parameters.AddWithValue("@cloth", savemdl.ClothColor);
                sqlx.Parameters.AddWithValue("@Name", savemdl.MappingName);

                conn.Open();
                return await Task.FromResult(sqlx.ExecuteNonQuery() > 0);
            }
        }
        internal ProductionUpload applyUserMapping(ProductionUpload model)
        {
            model.Mapping = new Dictionary<string, int?>();
            model.Mapping.Add("Isbn", getSelectValue(model.ddlIsbn));
            model.Mapping.Add("Title", getSelectValue(model.ddlTitle));
            model.Mapping.Add("ClothColor", getSelectValue(model.ddlClothColor));
            model.Mapping.Add("ContractID", getSelectValue(model.ddlContractID));
            model.Mapping.Add("Format", getSelectValue(model.ddlFormat));
            model.Mapping.Add("FormatSize", getSelectValue(model.ddlFormatSize));
            model.Mapping.Add("NumberOfPages", getSelectValue(model.ddlNumberOfPages));
            model.Mapping.Add("PaperStock", getSelectValue(model.ddlPaperStock));
            model.Mapping.Add("PaperWeight", getSelectValue(model.ddlPaperWeight));
            model.Mapping.Add("TrimSize", getSelectValue(model.ddlTrimSize));
            model.Mapping.Add("PrintType", getSelectValue(model.ddlPrintType));
            return model;
        }

        internal string[] getLatestMappings()
        {
            using (var conn = new SqlConnection(msss))
            {
                var sqlx = new SqlCommand() { CommandType = CommandType.StoredProcedure, CommandText = "dbo.getMappingNames", Connection = conn };
                var dtResult = new DataTable();
                conn.Open();
                dtResult.Load(sqlx.ExecuteReader());
                return (from DataRow dr in dtResult.Rows select dr.Field<string>("mapping")).ToArray();
            }
        }

        internal int[] getProductionMapping(string mappingKey)
        {
            mappingKey = String.IsNullOrWhiteSpace(mappingKey) ? null : mappingKey;
            using (var conn = new SqlConnection(msss))
            {
                var sqlx = new SqlCommand() { CommandType = CommandType.StoredProcedure, CommandText = "dbo.getPodMappings", Connection = conn };
                sqlx.Parameters.AddWithValue("@pub", mappingKey);
                var dtResult = new DataTable();
                conn.Open();
                dtResult.Load(sqlx.ExecuteReader());
                List<int> values = new List<int>();
                if (dtResult.Rows.Count > 0)
                {
                    var dr = dtResult.Rows[0];
                    foreach (object val in dr.ItemArray)
                    {
                        int x;
                        Int32.TryParse(val.ToString(), out x);
                        values.Add(x);
                    }
                }
                return values.ToArray();
            }
        }

        internal List<PodLineEntry> processSpreadsheet(Stream fileSrc, string mapName)
        {
            int[] selections = getProductionMapping(mapName); // Load this publishers column selections in order
            var results = new List<PodLineEntry>();
            using (ExcelPackage ep = new ExcelPackage(fileSrc))
            {
                var workbk = ep.Workbook.Worksheets; // All sheets in file
                foreach (var sheet in workbk)
                {
                    DataTable dt = new DataTable();
                    dt.TableName = sheet.Name;
                    int barrier = 0;
                    if (sheet.Dimension != null) { barrier = sheet.Dimension.End.Row; } else { break; }
                    for (int x = 2; x <= barrier; x++) //start at 2 to skip header row
                    {
                        var lineItem = new PodLineEntry()
                        {
                            LineNumber = x,
                            Isbn = sheet.Cells[x, selections[0]].Value,
                            Title = sheet.Cells[x, selections[1]].Value,
                            ContractID = sheet.Cells[x, selections[2]].Value,
                            NumberOfPages = sheet.Cells[x, selections[3]].Value,
                            TrimSize = sheet.Cells[x, selections[4]].Value,
                            PaperStock = sheet.Cells[x, selections[5]].Value,
                            PaperWeight = sheet.Cells[x, selections[6]].Value,
                            Format = sheet.Cells[x, selections[7]].Value,
                            PrintType = sheet.Cells[x, selections[8]].Value,
                            FormatSize = sheet.Cells[x, selections[9]].Value,
                            ClothColor = sheet.Cells[x, selections[10]].Value
                        };


                        //List<object> vals = new List<object>();
                        //foreach(int col in selections)
                        //{

                        //}s
                        /*
                         * Will have to either grab the schema for this row of selections,
                         * or fast construct columns equal to the int[] count
                         */

                        //object[] items = vals.ToArray();
                        if (lineItem.Isbn != null) { results.Add(lineItem); }
                    }
                }

            }
            return results;
        }

        public Dictionary<string, string[]> extractOldExcelHeaders(Stream fstream)
        {
            HSSFWorkbook wkb = new HSSFWorkbook(fstream);
            var wks = wkb.GetSheetAt(0);
            var hdrs = new List<string>();
            bool tripHook = true;
            int x = 0;
            while (tripHook)
            {
                string currCellValue = wks.GetRow(0).GetCell(x).StringCellValue;
                if (!String.IsNullOrWhiteSpace(currCellValue)) { hdrs.Add(currCellValue); x++; } else { tripHook = false; }

            }
            var results = new Dictionary<string, string[]>();
            results.Add(wks.SheetName, hdrs.ToArray());
            return results;
        }


        /*
         *  TODO: Need to repeat similar functionality to previous excel read, to facilitate faster dev and more synchronized result sets 
         */
        public List<PodLineEntry> consumeBinExcelFilee(Stream fstream, string MappingName)
        {
            HSSFWorkbook wkb = new HSSFWorkbook(fstream);
            var wks = wkb.GetSheetAt(0);
            var results = new List<PodLineEntry>();
            List<int> mapper = new List<int>();
            using (var conn = new SqlConnection(msss))
            {
                var sqlx = new SqlCommand() { CommandType = CommandType.StoredProcedure, CommandText = "dbo.getPodMappings", Connection = conn };
                sqlx.Parameters.AddWithValue("@pub", MappingName);
                conn.Open();
                DataTable dt = new DataTable();
                dt.Load(sqlx.ExecuteReader());
                conn.Close();
                if (dt.Rows.Count > 0)
                {
                    mapper = new List<int>();
                    foreach (object obj in dt.Rows[0].ItemArray)
                    {
                        int x;
                        Int32.TryParse(obj.ToString(), out x);
                        mapper.Add(x);
                    }
                }
            }
            for (int x = 1; x < wks.PhysicalNumberOfRows; x++)
            {
                var row = wks.GetRow(x);
                if (row != null)
                {
                    var item = new PodLineEntry()
                    {
                        LineNumber = x,
                        Isbn = row.GetCell(mapper[0]),
                        Title = row.GetCell(mapper[1]),
                        ContractID = row.GetCell(mapper[2]),
                        NumberOfPages = row.GetCell(mapper[3]),
                        TrimSize = row.GetCell(mapper[4]),
                        PaperStock = row.GetCell(mapper[5]),
                        PaperWeight = row.GetCell(mapper[6]),
                        Format = row.GetCell(mapper[7]),
                        PrintType = row.GetCell(mapper[8]),
                        FormatSize = row.GetCell(mapper[9]),
                        ClothColor = row.GetCell(mapper[10])
                    };
                    if (item.Isbn != null) { results.Add(item); }
                }
            }
            return results;
        }

        private int? getSelectValue(string x)
        {
            if (Int32.Parse(x) < 1) { return null; } else { return Int32.Parse(x); }
        }

        public Product LoadTitleById(int id)
        {
            var sqlp = new SqlParameter("@ProductId", id);
            var products = SvcTableModelBll.Load<Product>(new SqlParameter[] { sqlp });
            return (products.Length > 0)?products[0] :new Product();                
        }
    }
}