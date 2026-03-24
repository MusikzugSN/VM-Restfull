using System.ComponentModel.DataAnnotations;

namespace Vereinsmanager.Database.ScoreManagment;

public enum PrintMode
{
    Exact = 0,
    Over = 1,
    Under = 2
}

public enum DuplexMode
{
    No = 0,
    Long = 1,
    Short = 2
}

public class PrintSettings : MetaData
{
    [Key]
    public int PrintConfigId { get; set; }

    [Required]
    public int PageCount { get; set; }

    [Required]
    public PrintMode Mode { get; set; }

    [Required]
    public DuplexMode Duplex { get; set; }

    [Required]
    public int FileFormat { get; set; }
}

