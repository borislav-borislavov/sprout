using Sprout.Core.Services.Configurations;
using System.IO;

namespace Sprout.Tests
{
    public class SeedFileCryptoTests
    {
        [Fact]
        public void EncryptDecrypt_RoundTrip_ReturnsOriginalText()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                SeedFileCrypto.Encrypt(path, "{ \"hello\": \"world\" }", "EatLessSalt");

                Assert.True(SeedFileCrypto.IsEncrypted(path));
                var decrypted = SeedFileCrypto.Decrypt(path, "EatLessSalt");

                Assert.Equal("{ \"hello\": \"world\" }", decrypted);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public void IsEncrypted_PlainTextFile_ReturnsFalse()
        {
            var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            try
            {
                File.WriteAllText(path, "just some plain text that is long enough to pass the length check");
                Assert.False(SeedFileCrypto.IsEncrypted(path));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
