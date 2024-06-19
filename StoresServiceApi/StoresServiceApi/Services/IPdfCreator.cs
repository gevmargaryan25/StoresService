using System.Runtime.InteropServices;

namespace StoresServiceApi.Services
{
    public interface IPdfCreator
    {
        Task<byte[]> Create(string content);
    }
}
