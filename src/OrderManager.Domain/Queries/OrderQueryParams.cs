namespace OrderManager.Domain.Queries
{
    public class OrderQueryParams
    {
        public int? Page { get; set; } = 1;
        public int? PageSize { get; set; } = 10;

        public string? CustomerName { get; set; }
        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? SortBy { get; set; }        // ex: "CreatedAt", "Status", "CustomerName"
        public bool SortDesc { get; set; } = true; // true = desc, false = asc
    }
}
