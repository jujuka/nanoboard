using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using Chaos.NaCl;
using NDB;
using nboard;
using NServer;

namespace captcha
{
    /*
        Class encapsulates captcha retriveal (from pack) and verification.

        Each post has it's designated captcha. 
        It is determined by post's SHA256 hash (signature tag and it's contents
        are excluded from hash calculation).
        This hash should match POW filter - at least three consecutive bytes
        with values from zero to one, starting from fourth byte of a hash - 
        otherwise post is considered to be invalid (without POW).
        First three bytes of hash is captcha index (wrapped by amount of captchas
        in the pack).
        The code extracts relevant captcha by index extracted from post's hash,
        and captcha is public ed25519 key, 32-byte seed encrypted by XORing with
        SHA512(UTF-8(captcha answer + public key in hexstring form))
        and captcha image (1-bit) 50x20 pixels (column by column, each bit 
        represents a pixel (1 - black, 0 - white).
    */
    class Captcha
    {
        private static SHA256 _sha;
        private const int PowByteOffset = 3;
        private const int PowLength = 3;
        private const int PowTreshold = 1;
        private const int CaptchaBlockLength = 32 + 32 + 125;
        private const string DaraUriPngPrefix = "data:image/png;base64,";
        private const string CaptchaImageFileSuffix = ".png";
        public const string SignatureTag = "sign";
        private const string PowTag = "pow";

        private static string _packFile;

        static Captcha()
        {
            _packFile = Configurator.Instance.GetValue("captcha_pack_file", "captcha.nbc");
            _sha = SHA256.Create();
        }

        public string ImageDataUri
        {
            get
            {
                return LoadImageAsDataUri();
            }
        }

        private readonly byte[] _publicKey;
        private readonly byte[] _encryptedSeed;
        private readonly byte[] _imageBits;
        private string _imageDataUri = null;

        public Captcha(byte[] publicKey, byte[] encryptedSeed, byte[] imageBits)
        {
            _publicKey = publicKey;
            _encryptedSeed = encryptedSeed;
            _imageBits = imageBits;
        }

        public string LoadImageAsDataUri()
        {
            if (_imageDataUri != null) return _imageDataUri;
            var bitmap = BitmapConvert.Convert(_imageBits);
            var imageFile = Guid.NewGuid().ToString() + CaptchaImageFileSuffix;
            bitmap.Save(imageFile);
            var uri = DaraUriPngPrefix + Convert.ToBase64String(File.ReadAllBytes(imageFile));
            File.Delete(imageFile);
            _imageDataUri = uri;
            return _imageDataUri;
        }

        public string AddSignatureToThePost(string post, string guess)
        {
            var dec_seed = ByteEncryptionUtil.WrappedXor(_encryptedSeed, guess + _publicKey.Stringify());
            var privateKey = Ed25519.ExpandedPrivateKeyFromSeed(dec_seed);
            var signature = Ed25519.Sign(Encoding.UTF8.GetBytes(post), privateKey);
            return post + "[" + SignatureTag + "=" + signature.Stringify() + "]";
        }

        public bool CheckSignature(string postWithSignature)
        {
            var post = Encoding.UTF8.GetBytes(postWithSignature.ExceptSignature());
            var sign = postWithSignature.Signature();
            return Ed25519.Verify(sign, post, _publicKey);
        }

        public bool CheckGuess(string guess)
        {
            var dec_seed = ByteEncryptionUtil.WrappedXor(_encryptedSeed, guess + _publicKey.Stringify());
            var privateKey = Ed25519.ExpandedPrivateKeyFromSeed(dec_seed);
            var dummyMessage = new byte[]{(byte)0};
            var signature = Ed25519.Sign(dummyMessage, privateKey);
            return Ed25519.Verify(signature, dummyMessage, _publicKey);
        }

        public static Captcha GetCaptchaForPost(string captchaPackFilename, string post)
        {
            var size = new FileInfo(captchaPackFilename).Length;
            int count = (int) (size / CaptchaBlockLength);
            return GetCaptchaForIndex(captchaPackFilename, CaptchaIndex(post, count));
        }

        public static Captcha GetCaptchaForIndex(string captchaPackFilename, int captchaIndex)
        {
            if (captchaIndex == -1) return null;
            var publicKey = FileUtil.Read(captchaPackFilename, captchaIndex * CaptchaBlockLength, 32);
            var encryptedSeed = FileUtil.Read(captchaPackFilename, captchaIndex * CaptchaBlockLength + 32, 32);
            var image = FileUtil.Read(captchaPackFilename, captchaIndex * CaptchaBlockLength + 32 + 32, 125);
            return new Captcha(publicKey, encryptedSeed, image);
        }

        public static bool PostHasSolvedCaptcha(string post)
        {
            var captcha = GetCaptchaForPost(_packFile, post);
            if (captcha == null) return false;
            return captcha.CheckSignature(post);
        }

        public static bool PostHasValidPOW(string post)
        {
            post = post.ExceptSignature();
            var hash = _sha.ComputeHash(Encoding.UTF8.GetBytes(post));
            return hash.MaxConsecZeros(PowByteOffset, PowTreshold) >= PowLength;
        }

        public static int CaptchaIndex(string post, int max)
        {
            post = post.ExceptSignature();
            var hash = _sha.ComputeHash(Encoding.UTF8.GetBytes(post));
            if (hash.MaxConsecZeros(PowByteOffset, PowTreshold) < PowLength) return -1;
            return (hash[0] + hash[1] * 256 + hash[2] * 256 * 256) % max;
        }

        public static string GetHash(string post)
        {
            post = post.ExceptSignature();
            return _sha.ComputeHash(Encoding.UTF8.GetBytes(post)).Stringify();
        }

        public static string AddPow(string post)
        {
            post = post.ExceptSignature();
            byte[] hash = null;
            string result = post + "["+PowTag+"="+new string('0', 256)+"]";
            var buffer = new byte[128];
            var rand = new RNGCryptoServiceProvider();

            while (hash == null || hash.MaxConsecZeros(PowByteOffset, PowTreshold) < PowLength)
            {
                rand.GetBytes(buffer);
                result = post + "["+PowTag+"=" + buffer.Stringify() + "]";
                hash = _sha.ComputeHash(Encoding.UTF8.GetBytes(result));
            }

            return result;
        }
    }
}