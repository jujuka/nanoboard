using System;
using System.Security.Cryptography;
using System.Text;

namespace nboard
{
    class Hash
    {
        private readonly int _hashCode;
        public bool Invalid { get; private set; }
        public bool Zero { get; private set; }
        public readonly string Value;

        public static Hash CreateZero()
        {
            return new Hash(new string('0', HashCalculator.HashCrop*2));
        }

        public Hash(string value)
        {
            this.Value = value;
            _hashCode = value.GetHashCode();
            Zero = value == new string('0', HashCalculator.HashCrop*2);

            if (value.Length != HashCalculator.HashCrop*2)
            {
                Invalid = true;
                return;
            }

            for (int i = 0; i < HashCalculator.HashCrop*2; i++)
            {
                if (char.IsDigit(value[i]) ||
                    (value[i] >= 'a' && value[i] <= 'f'))
                {
                    continue;
                }

                else
                {
                    Invalid = true;
                    break;
                }
            }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Hash);
        }

        public bool Equals(Hash h)
        {
            return h != null && h.Value == Value;
        }
    }
}