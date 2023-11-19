using SixLabors.Fonts;

namespace Magus.Common.ImageSharp;

public static class FontFamilies
{
    private static readonly FontCollection fontCollection = new();

    static FontFamilies()
    {
        AddNotoSans();
    }

    static void AddNotoSans()
    {
        fontCollection.Add(Fonts.NotoSans_Black.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_BlackItalic.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_Bold.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_BoldItalic.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_ExtraBold.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_ExtraBoldItalic.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_ExtraLight.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_ExtraLightItalic.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_Italic.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_Light.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_LightItalic.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_Medium.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_MediumItalic.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_Regular.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_SemiBold.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_SemiBoldItalic.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_Thin.AsMemoryStream());
        fontCollection.Add(Fonts.NotoSans_ThinItalic.AsMemoryStream());
    }

    public static FontFamily NotoSans => fontCollection.Get("Noto Sans");

    private static MemoryStream AsMemoryStream(this byte[] font)
        => new(font);
}
