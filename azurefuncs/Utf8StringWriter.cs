using System.Text;
using System.IO;

namespace Rajirajcom.Api
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
