namespace WalletApp.Services.VcRender
{
    /// <summary>
    /// The result of resolving an <c>html</c> render suite for a Verifiable Credential,
    /// per the W3C VC Rendering Methods specification.
    /// </summary>
    public class HtmlRenderResult
    {
        /// <summary>
        /// The resolved HTML template fragment (no &lt;html&gt;/&lt;head&gt;/&lt;body&gt; tags),
        /// ready to be injected into the wrapper code's &lt;body&gt;.
        /// </summary>
        public required string TemplateHtml { get; init; }

        /// <summary>
        /// The (optionally <c>renderProperty</c>-filtered) Verifiable Credential JSON
        /// to embed in the iframe data block (<c>&lt;script type="application/vc"&gt;</c>).
        /// </summary>
        public required string VcJson { get; init; }
    }
}
