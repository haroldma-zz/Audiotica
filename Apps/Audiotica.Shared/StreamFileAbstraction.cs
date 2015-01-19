using System.IO;
using TagLib;

namespace Audiotica
{
    //From an old article I wrote :)
    //http://www.geekchamp.com/articles/reading-and-writing-metadata-tags-with-taglib
    public class SimpleFileAbstraction : File.IFileAbstraction
    {
        public SimpleFileAbstraction(string name, Stream readStream, Stream writeStream)
        {
            WriteStream = writeStream;
            ReadStream = readStream;
            Name = name;
        }

        public string Name { get; private set; }

        public Stream ReadStream { get; private set; }

        public Stream WriteStream { get; private set; }

        public void CloseStream(Stream stream)
        {
            stream.Position = 0;
        }
    }
}