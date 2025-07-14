namespace WordReplacer.Models;

public record FileUploadDto
{
    public required string Name { get; init; }

    public required string Type { get; init; }
    public long Size { get; init; }
    public DateTimeOffset LastModified { get; init; }

}