using Microsoft.AspNetCore.Mvc;

public class MenuCreateUpdateDTO
{
    [FromForm(Name = "ItemName")]
    public string ItemName { get; set; } = string.Empty;

    [FromForm(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [FromForm(Name = "CategoryName")]
    public string CategoryName { get; set; } = string.Empty;

    [FromForm(Name = "Price")]
    public decimal Price { get; set; }

    [FromForm(Name = "DietaryInfo")]
    public string DietaryInfo { get; set; } = string.Empty;

    [FromForm(Name = "TasteInfo")]
    public string TasteInfo { get; set; } = string.Empty;

    [FromForm(Name = "AvailabilityTime")]
    public string AvailabilityTime { get; set; } = string.Empty;

    [FromForm(Name = "NutritionalInfo")]
    public string NutritionalInfo { get; set; } = string.Empty;

    [FromForm(Name = "RestaurantId")]
    public int RestaurantId { get; set; }

    [FromForm(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    [FromForm(Name = "ImageFile")]
    public IFormFile? ImageFile { get; set; }
}
