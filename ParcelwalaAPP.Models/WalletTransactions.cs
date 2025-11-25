using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParcelwalaAPP.Models
{
    [Table("WalletTransactions")]
    public class WalletTransactions
    {
        [Key]
       public int WalletTransactionID {get;set;}
       public int UserID              {get;set;}
       public int CustomerID { get;set;}
       public string TransactionType     {get;set;}
       public decimal Amount              {get;set;}
       public decimal BalanceBefore       {get;set;}
       public decimal BalanceAfter        {get;set;}
       public string Description         {get;set;}
       public string ReferenceType       {get;set;}
       public int? ReferenceID         {get;set;}
       public DateTime CreatedAt { get; set; }
        [ForeignKey(nameof(UserID))]
        public Users User { get; set; } = null!;
        // Navigation property
        [ForeignKey("CustomerID")]
        public virtual CustomerProfiles CustomerProfile { get; set; } = null!;


    }                            
}
