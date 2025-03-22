using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using FrameResizer.Interface;

namespace FrameResizer.Service;
public class CustomiseImage:ICustomiseImage
{
    /// <summary>
    /// 画像をリサイズ、枠線を描画して保存する
    /// </summary>
    /// <param name="sourceImagePath">元の画像ファイルのパス</param>
    /// <param name="outputImagePath">出力する画像ファイルのパス</param>
    /// <param name="outputHeight">リサイズする縦幅 ※0の場合は横幅を基準</param>
    /// <param name="outputWidth">リサイズする横幅 ※0の場合は縦幅を基準</param>
    /// <param name="borderSize">枠線の太さ</param>
    /// <param name="borderColor">枠線の色</param>
    public void Convert(String sourceImagePath, String outputImagePath,
                                          Int32 outputHeight, Int32 outputWidth,
                                          Int32 borderSize, Color borderColor)
    {
        using Image tmpImage = Image.Load(sourceImagePath);

        // アスペクト比を維持したリサイズ処理
        /// 元の画像の比率を取得
        Double aspectRatio = (Double)tmpImage.Width / tmpImage.Height;
        /// Height or Width を基準にリサイズ ※どちらかは1にする
        if(outputWidth > 1 && outputHeight == 1)
            outputHeight = (Int32)(outputWidth / aspectRatio);
        if(outputHeight > 1 && outputWidth == 1)
            outputWidth = (Int32)(outputHeight * aspectRatio);
        /// borderの長さだけ余分に縮小
        Int32 innerWidth = outputWidth - (borderSize * 2);
        Int32 innerHeight = outputHeight - (borderSize * 2);
        /// リサイズ実行
        tmpImage.Mutate(ctx => ctx.Resize(innerWidth, innerHeight));

        // 出力サイズの画像を作成し、背景(border色)で塗り、縮小された画像を描画
        /// border色の背景色の画像を作成
        using Image<Rgba32> outputImage = new Image<Rgba32>(outputWidth, outputHeight);
        outputImage.Mutate(ctx => ctx.Fill(borderColor));
        /// Pointで左上を基準にborderのサイズだけずらして描画
        outputImage.Mutate(ctx => ctx.DrawImage(tmpImage, new Point(borderSize, borderSize), 1f));

        SaveImage(outputImagePath, outputImage);
    }


    /// <summary>
    /// リサイズのみ実行
    /// </summary>
    public void Convert(String sourceImagePath, String outputImagePath,
                                        Int32 outputHeight, Int32 outputWidth)
    {
        using Image<Rgba32> outputImage = Image.Load<Rgba32>(sourceImagePath);

        Double aspectRatio = (Double)outputImage.Width / outputImage.Height;
        if(outputWidth > 1 && outputHeight == 1)
            outputHeight = (Int32)(outputWidth / aspectRatio);
        if(outputHeight > 1 && outputWidth == 1)
            outputWidth = (Int32)(outputHeight * aspectRatio);
        outputImage.Mutate(ctx => ctx.Resize(outputWidth, outputHeight));

        SaveImage(outputImagePath, outputImage);
    }



    /// <summary>
    /// 枠線描画のみ実行
    /// </summary>
    public void Convert(String sourceImagePath, String outputImagePath,
                                          Int32 borderSize, Color borderColor)
    {
        using Image<Rgba32> tmpImage = Image.Load<Rgba32>(sourceImagePath);

        // Border分サイズは大きくなる
        Int32 outputWidth = tmpImage.Width + (borderSize * 2);
        Int32 outputHeight = tmpImage.Height + (borderSize * 2);

        using Image<Rgba32> outputImage = new Image<Rgba32>(outputWidth, outputHeight);
        outputImage.Mutate(ctx => ctx.Fill(borderColor));
        outputImage.Mutate(ctx => ctx.DrawImage(tmpImage, new Point(borderSize, borderSize), 1f));

        SaveImage(outputImagePath, outputImage);
    }


    /// <summary>
    /// 画像を保存
    /// </summary>
    private void SaveImage(String outputImagePath, Image<Rgba32> outputImage)
    {
        // 拡張子(jpg, png)に応じて保存
        String extension = System.IO.Path.GetExtension(outputImagePath);
        if(extension == ".png")
            outputImage.Save(outputImagePath, new PngEncoder());
        if(extension is ".jpg" or ".jpeg")
            outputImage.Save(outputImagePath, new JpegEncoder() { Quality = 100 });
    }
}
