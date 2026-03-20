using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

[Index(nameof(FilePath), IsUnique = true)]
[Index(nameof(MusicSheetId), nameof(SortOrder), IsUnique = true)]
public class MusicSheetFile
{
    [Key]
    public int MusicSheetFileId { get; set; }

    [Required]
    public int MusicSheetId { get; set; }

    public MusicSheet? MusicSheet { get; set; }

    [Required]
    [MaxLength(255)]
    public required string FilePath { get; set; }

    [Required]
    public int SortOrder { get; set; }

    [Required]
    public int Filesize { get; set; }

    [Required]
    public int PageCount { get; set; }

    [Required]
    [MaxLength(255)]
    public required string FileHash { get; set; }
}