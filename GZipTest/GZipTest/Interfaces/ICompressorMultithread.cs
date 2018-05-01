namespace GZipTest.Interfaces
{
    public interface ICompressorMultiThread : ICompressor
    {
        void ThreadPoolCallback(object threadContext);
        
        int Status();
    }
}
