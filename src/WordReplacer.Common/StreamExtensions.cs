namespace WordReplacer.Common
{
    /* A static class that extends the Stream class. */
    public static class StreamExtensions
    {
        /// <summary>
        /// Converts a stream to a base64 string
        /// </summary>
        /// <param name="stream">The stream to convert to base64.</param>
        public static string ConvertToBase64(this Stream stream)
        {
            if (stream is MemoryStream memoryStream)
            {
                return Convert.ToBase64String(memoryStream.ToArray());
            }

            var bytes = new byte[(int)stream.Length];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);

            return Convert.ToBase64String(bytes);
        }
    }
}
