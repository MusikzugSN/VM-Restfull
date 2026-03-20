using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Vereinsmanager.Database.ScoreManagment;

public enum MusicSheetStatus
{
    Ungeprueft = 0,
    Geprueft = 1
}

[Index(nameof(ScoreId), nameof(VoiceId), IsUnique = true)]
public class MusicSheet : MetaData
{
    [Key]
    public int MusicSheetId { get; set; }

    [Required]
    public int Filesize { get; set; }

    [Required]
    public int PageCount { get; set; }

    [Required]
    public DateTime FileModifiedDate { get; set; }

    [Required]
    [MaxLength(255)]
    public required string FileHash { get; set; }

    [Required]
    public bool IsMarschbuch { get; set; }

    [Required]
    public MusicSheetStatus Status { get; set; } = MusicSheetStatus.Ungeprueft;

    [Required]
    public required Score Score { get; set; }
    public virtual int ScoreId { get; set; }

    [Required]
    public required Voice Voice { get; set; }
    public virtual int VoiceId { get; set; }

    [Required]
    public ICollection<MusicSheetFile> Files { get; set; } = new List<MusicSheetFile>();
}