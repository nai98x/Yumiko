namespace Yumiko.Datatypes
{
    public class Country
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "JSON Format")]
        public string name_en { get; set; } = null!;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "JSON Format")]
        public string name_es { get; set; } = null!;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "JSON Format")]
        public string dial_code { get; set; } = null!;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "JSON Format")]
        public string code { get; set; } = null!;
    }
}
