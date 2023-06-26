using SixLabors.Fonts;

namespace Magus.Common.ImageSharp
{
    public static class FontFamilies
    {
        private static readonly FontCollection fontCollection = new();

        static FontFamilies()
        {
            AddNotoSans();
        }

        static void AddNotoSans()
        {
            fontCollection.Add(Common.Fonts.NotoSans_Black.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_BlackItalic.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_Bold.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_BoldItalic.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_ExtraBold.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_ExtraBoldItalic.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_ExtraLight.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_ExtraLightItalic.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_Italic.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_Light.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_LightItalic.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_Medium.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_MediumItalic.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_Regular.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_SemiBold.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_SemiBoldItalic.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_Thin.AsMemoryStream());
            fontCollection.Add(Common.Fonts.NotoSans_ThinItalic.AsMemoryStream());
        }

        public static FontFamily NotoSans => fontCollection.Get("Noto Sans");

        private static MemoryStream AsMemoryStream(this byte[] font)
            => new(font);
    }
}
