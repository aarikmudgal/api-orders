namespace eshop.api.order.Models
{
    public class Article
    {
        public string ArticleId { get; set; }
        public int Quantity { get; set; }
        public string Price { get; set; }
        public string TotalPrice { get; set; }
    }
}
