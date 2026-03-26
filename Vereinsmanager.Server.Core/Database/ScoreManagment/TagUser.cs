using System.ComponentModel.DataAnnotations;
using Vereinsmanager.Database.Base;

namespace Vereinsmanager.Database.ScoreManagment;

public class TagUser
{
    [Key]
    public int TagUserId { get; set; }
    
    [Required]
    public int TagId { get; set; }
    [Required]
    public int UserId { get; set; }
    [Required]
    public int MusicSheetId { get; set; }
    
    
    public Tag? Tag { get; set; }
    public User? User { get; set; }
    public MusicSheet? MusicSheet { get; set; }
}