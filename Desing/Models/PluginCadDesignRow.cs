using System.Collections.Generic;

namespace Desing.Models
{
    public sealed class PluginCadDesignRow
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string OfferNumber { get; set; }
    }

    public sealed class PluginCadLookupItem
    {
        public long Id { get; set; }
        public string Label { get; set; }
    }

    public sealed class PluginCadJobsideRow
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Label { get; set; }
    }

    public sealed class PluginCadOfferRow
    {
        public long Id { get; set; }
        public long JobsideId { get; set; }
        public string Number { get; set; }
        public string Label { get; set; }
    }

    public sealed class PluginCadHomeVm
    {
        public string UserName { get; set; }
        public List<PluginCadJobsideRow> Jobsides { get; set; }
        public List<PluginCadOfferRow> Offers { get; set; }
        public List<PluginCadDesignRow> Designs { get; set; }
        public List<PluginCadLookupItem> Clients { get; set; }
        public List<PluginCadLookupItem> Branches { get; set; }
        public List<PluginCadLookupItem> OfferStates { get; set; }
    }

    public sealed class PluginCadBlockRow
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string CodeName { get; set; }
        public string Label { get; set; }
        public string Caption { get; set; }
        public string IcoUrl { get; set; }
        public string StlUrl { get; set; }
        public string StlPhenolicUrl { get; set; }
        public string DwgUrl { get; set; }
        public string DwgUrl3D { get; set; }
        public string DwgUrl3DRef { get; set; }
        public string DwgUrlXr { get; set; }
    }

    public sealed class PluginCadArticleIcoRow
    {
        public long IdObject { get; set; }
        public string ImgIco { get; set; }
    }

    public sealed class PluginCadBlocksVm
    {
        public string LogoUrl { get; set; }
        public List<PluginCadBlockRow> Items { get; set; }
    }

    public sealed class PluginCadLibraryFile
    {
        public string Code { get; set; }
        public string View { get; set; }
        public string Folder { get; set; }
        public string File { get; set; }
        public string Url { get; set; }
        public string Version { get; set; }
        public long Size { get; set; }
    }

    public sealed class PluginCadLibraryManifest
    {
        public bool Ok { get; set; }
        public string GeneratedUtc { get; set; }
        public List<PluginCadLibraryFile> Files { get; set; }
        public List<PluginCadBlockRow> Catalog { get; set; }
    }
}
