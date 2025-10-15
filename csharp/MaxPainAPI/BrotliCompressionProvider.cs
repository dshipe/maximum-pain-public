using Microsoft.AspNetCore.ResponseCompression;
using System.IO;
using System.IO.Compression;


namespace MaxPainAPI
{
    public class BrotliCompressionProvider : ICompressionProvider
    {
        public string EncodingName => "br";

        public bool SupportsFlush => true;

        public Stream CreateStream(Stream outputStream) => new BrotliStream(outputStream, CompressionMode.Compress);
    }
}
