namespace MVCTutorial.Models.ViewModels;

public class ShoppingCartVM
{
    public IEnumerable<ShoppingCart> ListCarts { get; set; }
    public double CartTotal { get; set; }
}