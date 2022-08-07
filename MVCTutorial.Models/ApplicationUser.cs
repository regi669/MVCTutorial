using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MVCTutorial.Models;

public class ApplicationUser : IdentityUser
{
    [Required] 
    public string Name { get; set; }
    public string? StreetAdress { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public int? CompanyId { get; set; }
    [ValidateNever]
    public Company Company { get; set; }
    
}