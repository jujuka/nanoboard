using System;
using System.Security.Cryptography;
using System.Text;

namespace nboard
{
    class NanoPost
    {
        public const int MaxPostByteLength = 65000;
        public const string RootStub = "{Welcome to Nanoboard}";
        private readonly Hash _hash;
        private readonly string _raw;
        private readonly byte[] _bytes;
        public bool Invalid { get; private set; }
        public readonly Hash ReplyTo;
        public readonly string Message;

        public int DepthTag = 0;

        // create thread at root
        public static NanoPost Create(string message)
        {
            return new NanoPost(new NanoPost(Hash.CreateZero(), RootStub).GetHash(), message);
        }

        // reply to thread (thread) to some post (to)
        public static NanoPost Reply(Hash to, string message)
        {
            return new NanoPost(to, message);
        }

        public NanoPost(string raw)
        {
            _raw = raw;
            _hash = HashCalculator.Calculate(_raw);

            if (raw.Length <= HashCalculator.HashCrop*2)
            {
                Invalid = true;
                return;
            }

            ReplyTo = new Hash(raw.Substring(0, HashCalculator.HashCrop*2));
            Message = raw.Substring(HashCalculator.HashCrop*2);

            _bytes = Encoding.UTF8.GetBytes(_raw);
            if (_bytes.Length > NanoPost.MaxPostByteLength) Invalid = true;

            if (ReplyTo.Invalid)
            {
                Invalid = true;
            }
        }

        public NanoPost(Hash replyTo, string message)
        {
            _raw = replyTo.Value + message;
            _hash = HashCalculator.Calculate(_raw);

            ReplyTo = replyTo;
            Message = message;

            _bytes = Encoding.UTF8.GetBytes(_raw);
            if (_bytes.Length > NanoPost.MaxPostByteLength) Invalid = true;

            if (ReplyTo.Invalid)
            {
                Invalid = true;
            }
        }

        public byte[] SerializedBytes()
        {
            return _bytes;
        }

        public string SerializedString()
        {
            return _raw;
        }

        public Hash GetHash()
        {
            return _hash;
        }

        public override int GetHashCode()
        {
            return _raw.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj != null) && (obj as NanoPost)._raw == _raw;
        }
    }
}