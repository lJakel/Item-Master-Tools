using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.Data.SqlClient;
using ModelService.ItemMaster.Models;
using ModelService;
using ModelService.ItemMaster.SearchModels;

namespace Tools.Controllers
{
    
    [RoutePrefix("Imports")]
    [Route("{action}")]
    public class ImportsController : Controller
    {
        private ProductionUpload prodUp;
        private ProductionConsumer consumer;
        private Models.TitleSearch tsrch;
        private Product tDetail;

        public ImportsController() : base()
        {
            prodUp = new ProductionUpload();
            consumer = new ProductionConsumer();
            tsrch = new Models.TitleSearch();
            tDetail = new Product();
        }

        /*
         * Controller Methods for Views
         * -nt 10/23/14
         */
        [HttpGet]
        [Route]
        public ActionResult Index()
        {
            return View();
        }
        
        [HttpGet]
        public ActionResult ProductionMapping()
        {
            return View("~/Views/Imports/ProductionSettingsMap.cshtml", prodUp);            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProductionMapping(HttpPostedFileBase FileIn, string Source)
        {
            var cmd = new internalFunctions();            
            if (FileIn != null && FileIn.ContentLength > 0)
            {
                // then process according to file source
                switch (Source)
                    {
                    case "1": // Excel XML
                        prodUp.ErrorMsg = "Excel Uploaded Successfully";
                        prodUp.MappingHeaders = cmd.getExcelHeaders(FileIn.InputStream);
                        prodUp.Source = "xlsx";
                        break;
                    case "2": // Classic Excel
                        prodUp.ErrorMsg = "Excel Uploaded Successfully";
                        prodUp.MappingHeaders = cmd.extractOldExcelHeaders(FileIn.InputStream);
                        prodUp.Source = "xls";
                        break;
                    case "3": // XML
                        prodUp.ErrorMsg = "XML Uploaded Successfully";
                        prodUp.Source = "xml";
                        break;
                    }                
            }            
            return ProductionMapping();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubmitProductionCodex(ProductionUpload model)
        {
            var cmd = new internalFunctions();
            model = cmd.applyUserMapping(model);
            if (await cmd.SaveUpdateMapping(model))
            { 
                prodUp.ErrorMsg = "Saved"; 
            }
           return ProductionMapping();
        }

       
        [HttpGet]
        public ActionResult PodConsumer()
        {
            var cmd = new internalFunctions();
            this.consumer.Mappings = cmd.getLatestMappings();
            
            return View("~/Views/Imports/ProductionTitleImport.cshtml", this.consumer );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PodTitleUpload(ProductionConsumer model)
        {
            var cmd = new internalFunctions();
            if (model.FileIn != null)
            {
                var dctResult = new List<PodLineEntry>();
                switch (model.Source)
                {
                    case "1": // Newer Excel File .xlsx
                        dctResult = cmd.processSpreadsheet(model.FileIn.InputStream, model.MappingName);
                        break;
                    case "2": // Older Excel File .xls
                        dctResult = cmd.consumeBinExcelFilee(model.FileIn.InputStream, model.MappingName);
                        break;
                    case "3": // XML Flat File .xml
                        break;
                    case "0": // No format selected
                        break;

                }
                this.consumer.Entries = dctResult;
                int ctID = -1;
                if (Int32.TryParse(model.txtContractID, out ctID))
                {
                    foreach (var item in consumer.Entries)
                    {
                        item.ContractID = ctID;
                    }
                }
            }
            return PodConsumer();
        }

        [HttpGet]
        public ActionResult TitleSearch()
        {
            return View("~/Views/Shared/ReusableControls/TitleSearch.cshtml", tsrch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TitleSearch(ProductSearch srch)
        {
            srch.prepareQuery(); // Collects the values from the form and puts them in array
            var resultset = SvcSearchBll.EntitySearch<Product>(srch.Query);
            tsrch.Results = resultset.ToList();
            return TitleSearch();
        }

        [HttpGet]
        public ActionResult TitleDetails(int? id)
        {
            var prd = new Product();
            var sqlp = new SqlParameter[1];
            if (id.HasValue)
            {
                sqlp[0] = new SqlParameter("@ProductID", id.Value);
                //sqlp[1] = new SqlParameter("@Isbn13", null);
                tDetail = SvcTableModelBll.Load<Product>(sqlp)[0] ?? new Product();
            }
            return View("~/Views/Imports/TitleDetails.cshtml", tDetail);
        }
    }
}