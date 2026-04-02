namespace Catalog.API.Application.DTOs.MediaAsset
{
    public class MediaAssetResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string AltText { get; set; } = string.Empty;
    }
}
