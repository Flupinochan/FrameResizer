using SixLabors.ImageSharp;

namespace FrameResizer.Interface;
public interface ICustomiseImage
{
    public void Convert(String sourceImagePath, String outputImagePath, Int32 outputHeight, Int32 outputWidth, Int32 borderSize, Color borderColor);
    public void Convert(String sourceImagePath, String outputImagePath, Int32 outputHeight, Int32 outputWidth);
    public void Convert(String sourceImagePath, String outputImagePath, Int32 borderSize, Color borderColor);
}