namespace Catalog.API.Application.DTOs.MediaAsset
{
    public class CreateMediaAssetDto
    {
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public string AltText { get; set; } = string.Empty;
    }
}
