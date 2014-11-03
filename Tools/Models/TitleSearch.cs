using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ModelService.ItemMaster.Models;

namespace Tools.Models
{
    public class TitleSearch
    {
        [Display(Name ="ISBN-13")]
        public long? Isbn13 { get; set; }

        [Display(Name="Product Code")]
        public string ProductCode { get; set; }

        [Display(Name ="Title")]
        public string Title { get; set; }

        [Display(Name ="Product Binding Type")]
        public string ProductTypeName { get; set; }

        [Display(Name ="SubTitle")]
        public string Subtitle { get; set; }

        [Display(Name ="Isbn10")]
        public string Isbn10 { get; set; }

        [Display(Name ="Date Created")]
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:mm-dd-yyy}")]
        public DateTime? Ctime { get; set; }

        [Display(Name ="Last Modified")]
        [DisplayFormat(ApplyFormatInEditMode = true, ConvertEmptyStringToNull = true, DataFormatString = "{0:mm-dd-yyy}")]
        public DateTime? Atime { get; set; }

        [Display(Name = "Imprint")]
        public string Imprint { get; set; }

        public int? ProductType { get; set; } // Will be assigned to dropdownfor()

        public List<Product> Results { get; set; }

        public TitleSearch()
        {
            Results = new List<Product>();
        }
    }    
}