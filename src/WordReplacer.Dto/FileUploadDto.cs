namespace WordReplacer.Dto;

public record FileUploadDto
{
    public required string Name { get; init; }
    
    public required string Type { get; init; }
    
    public required byte[] Content { get; init; }
    public long Size { get; init; }
    public DateTimeOffset LastModified { get; init; }
    

}