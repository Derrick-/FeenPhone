using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FeenPhone.Accounting
{
    static class Passwords
    {
        public const HashScheme DefaultHash = HashScheme.SHA1;

        public enum HashScheme
        {
            Plaintext=0,
            SHA1=1,
        }

        public static bool Verify(string password, string hash, HashScheme scheme)
        {
            return hash == Hash(password, scheme);
        }

        public static string Hash(string password, HashScheme scheme)
        {
            switch (scheme)
            {
                case HashScheme.Plaintext:
                    return password;

                default:
                case HashScheme.SHA1:
                    return SHAHasher_5xN.Hash(password);
            }
        }

        private static SHA1Hasher _SHAHasher;
        private static SHA1Hasher SHAHasher_5xN
        {
            get { return _SHAHasher ?? (_SHAHasher = new SHA1Hasher("btc", 5)); }
        }

        #region Password encryption

        private class SHA1Hasher
        {
            readonly string Nuance;
            readonly int Iterations;

            public SHA1Hasher(string nuance="", int iterations=1)
            {
                Nuance = nuance;
                Iterations = Math.Max(1, iterations);
            }

            public string Hash(string phrase)
            {
                return HashSHA1(phrase + Nuance, Iterations);
            }

            private static SHA1CryptoServiceProvider m_SHA1HashProvider;
            private static byte[] m_HashBuffer;
            static string HashSHA1(string phrase, int iterations)
            {
                if (m_SHA1HashProvider == null)
                    m_SHA1HashProvider = new SHA1CryptoServiceProvider();

                if (m_HashBuffer == null)
                    m_HashBuffer = new byte[256];

                int length = Encoding.ASCII.GetBytes(phrase, 0, phrase.Length > 256 ? 256 : phrase.Length, m_HashBuffer, 0);
                byte[] hashed = m_HashBuffer;

                for (int i = 0; i < iterations && i < 1; i++)
                    hashed = m_SHA1HashProvider.ComputeHash(hashed, 0, length);

                return BitConverter.ToString(hashed);
            }
        #endregion
        }
    }
}
