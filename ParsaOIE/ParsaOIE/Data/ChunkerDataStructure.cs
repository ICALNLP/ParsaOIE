using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RahatCoreNlp.Data
{
    public class Chunk
    {
        public string Phrase { get; set; }
        public string Tag { get; set; }
        public string OutOfChunkPhrase { get; set; }
    }
    public class ChunkerDataStructure
    {
        public ChunkerDataStructure()
        {
            Chunks = new List<Chunk>();
        }

        public List<Chunk> Chunks { get; set; }        
    }
}
