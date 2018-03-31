namespace eshop.api.order.Models
{
    public class Article
    {
        public int ArticleId { get; set; }
        public string ArticleName { get; set; }
        public string ArticleDescription { get; set; }
        public double ArticlePrice { get; set; }
        public double TotalPrice { get; set; }
        public string ImageUrl { get; set; }
        public int Quantity { get; set; }
    }
}
