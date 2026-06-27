using System.Linq;

namespace FalconNetSdk.Scripts.Bigdata.Encrypts.Aes
{
    public class AesEncrypted
    {
        public AesEncrypted(byte[] data, byte[] iv)
        {
            Data = data;
            Iv = iv;
        }

        public AesEncrypted(byte[] concat)
        {
            using var aesAlg = System.Security.Cryptography.Aes.Create();
            var blockBytes = aesAlg.BlockSize/8;
            Iv = concat.Take(blockBytes).ToArray();
            Data = concat.Skip(blockBytes).ToArray();
        }

        public byte[] Data { get; }
        public byte[] Iv { get; }

        public byte[] Concat()
        {
            var result = new byte[Data.Length + Iv.Length];
            Iv.CopyTo(result, 0);
            Data.CopyTo(result, Iv.Length);
            return result;
        }
    }
}