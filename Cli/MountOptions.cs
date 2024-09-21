namespace CodingFS.Cli;

public sealed class MountOptions
{
	public string Point { get; set; } = string.Empty;

	public string? VolumeLabel { get; set; }

	public bool Readonly { get; set; } = false;

	public FileType Type { get; set; } = FileType.Source;
}
