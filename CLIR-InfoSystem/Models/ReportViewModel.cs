namespace CLIR_InfoSystem.Models
{
    public class ReportViewModel
    {
        public int TotalBooks { get; set; }
        public int ActiveLoans { get; set; }
        public int PendingServices { get; set; }
        public List<BookBorrowing> RecentTransactions { get; set; }
    }
}