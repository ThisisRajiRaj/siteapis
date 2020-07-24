using System.IO;
using System.Text;

namespace Rajirajcom.Api
{
    public class StringWriterWithEncoding : StringWriter
    {
        public StringWriterWithEncoding(StringBuilder sb, Encoding encoding)
            : base(sb)
        {
            this.encoding = encoding;
        }
        private readonly Encoding encoding;
        public override Encoding Encoding
        {
            get
            {
                return this.encoding;
            }
        }
    }
}
