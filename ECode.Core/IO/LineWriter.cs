using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ECode.Utility;

namespace ECode.IO
{
    public sealed class LineWriter
    {
        private Stream      m_pStream   = null;


        /// <summary>
        /// Gets or sets if CRLF used.
        /// </summary>
        public bool CRLFLines
        { get; set; } = true;

        /// <summary>
        /// Gets or sets string related encoding.
        /// </summary>
        public Encoding Encoding
        { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets how many lines are written.
        /// </summary>
        public int LinesWritten
        { get; private set; }

        /// <summary>
        /// Gets how many bytes are written.
        /// </summary>
        public long BytesWritten
        { get; private set; }


        public LineWriter(Stream stream)
        {
            if (stream == null)
            { throw new ArgumentNullException(nameof(stream)); }

            if (!stream.CanWrite)
            { throw new ArgumentException($"Argument '{nameof(stream)}' cannot be written.", nameof(stream)); }

            m_pStream = stream;
        }


        /// <summary>
        /// Writes specified line to stream.
        /// </summary>
        /// <param name="line">Line to send.</param>
        /// <returns>Returns number of bytes written.</returns>
        public int Write(string line)
        {
            AssertUtil.ArgumentNotNull(line, nameof(line));

            if (this.CRLFLines && !line.EndsWith("\r\n", StringComparison.InvariantCultureIgnoreCase))
            { line += "\r\n"; }
            else if (!this.CRLFLines && !line.EndsWith("\n", StringComparison.InvariantCultureIgnoreCase))
            { line += "\n"; }

            var bytes = this.Encoding.GetBytes(line);
            m_pStream.Write(bytes, 0, bytes.Length);
            //inStream.Flush();

            LinesWritten++;
            BytesWritten += bytes.Length;

            return bytes.Length;
        }

        /// <summary>
        /// Writes specified line to stream.
        /// </summary>
        /// <param name="line">Line to send.</param>
        /// <returns>Returns number of bytes written.</returns>
        public Task<int> WriteAsync(string line)
        {
            AssertUtil.ArgumentNotNull(line, nameof(line));

            return Task.Factory.StartNew(() =>
            {
                return Write(line);
            });
        }
    }
}
