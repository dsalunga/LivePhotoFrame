namespace LivePhotoFrame.Maui.Models;

public class Photo
{
    public string Path { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }

    public bool IsPortrait => Height > Width;
}
