using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NLog;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace BouncyCastleWrapper
{
    public static class Wrapper
    {
        private static readonly SecureRandom _Random = new SecureRandom();
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        // ReSharper disable InconsistentNaming
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        // ReSharper restore InconsistentNaming
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        //Preconfigured Encryption Parameters
        private static readonly int NonceBitSize = 128;
        private static readonly int MacBitSize = 128;
        private static readonly int KeyBitSize = 256;

        //Preconfigured Password Key Derivation Parameters
        public static readonly int SaltBitSize = 128;
        public static readonly int Iterations = 10000;
        public static readonly int MinPasswordLength = 12;

        public static string SignPublicKey(string inputMessage, string asymmetricKey)
        {
            var utf8Enc = new UTF8Encoding();

            // Converting the string message to byte array
            byte[] inputBytes = utf8Enc.GetBytes(inputMessage);

            // Extracting the public key from the pair
            RsaKeyParameters publicKey = GetKey(asymmetricKey);

            // Creating the RSA algorithm object
            IAsymmetricBlockCipher cipher = new RsaEngine();

            // Initializing the RSA object for Encryption with RSA public key. 
            // Remember, for encryption, public key is needed
            cipher.Init(true, publicKey);

            //Encrypting the input bytes
            Log.Debug("inputMessage: " + inputMessage.Length);

            byte[] cipheredBytes = cipher.ProcessBlock(inputBytes, 0, inputMessage.Length);
            var signedString = Convert.ToBase64String(cipheredBytes);
            return signedString;

        }
       
        public static string SignPublicKey(AsymmetricKeyParameter asymmetricKey)
        {
            string inputMessage = "Test Message";
            UTF8Encoding utf8enc = new UTF8Encoding();
 
            // Converting the string message to byte array
            byte[] inputBytes = utf8enc.GetBytes(inputMessage); 
 
            // Extracting the public key from the pair
            RsaKeyParameters publicKey = (RsaKeyParameters)asymmetricKey;
 
            // Creating the RSA algorithm object
            IAsymmetricBlockCipher cipher = new RsaEngine();
 
            // Initializing the RSA object for Encryption with RSA public key. 
            // Remember, for encryption, public key is needed
            cipher.Init(true,publicKey);
 
            //Encrypting the input bytes
            byte[] cipheredBytes = cipher.ProcessBlock(inputBytes, 0, inputMessage.Length);
            var signedString = Convert.ToBase64String(cipheredBytes);
            return signedString;
        }

        public static string VerifyPrivateKey(string inputString, string asymmetricKey)
        {
            var inputBytes = Convert.FromBase64String(inputString);

            UTF8Encoding utf8Enc = new UTF8Encoding();

            RsaKeyParameters privateKey = GetKey(asymmetricKey);
            IAsymmetricBlockCipher cipher = new RsaEngine();
            cipher.Init(false, privateKey);
            byte[] deciphered = cipher.ProcessBlock(inputBytes, 0, inputBytes.Length);
            string decipheredText = utf8Enc.GetString(deciphered);

            return decipheredText;
        }

        public static string VerifyPrivateKey(string inputString, AsymmetricKeyParameter asymmetricKey)
        {
            var inputBytes = Convert.FromBase64String(inputString);

            UTF8Encoding utf8enc = new UTF8Encoding();

            RsaKeyParameters privateKey = (RsaKeyParameters)asymmetricKey;
            IAsymmetricBlockCipher cipher = new RsaEngine();
            cipher.Init(false, privateKey);
            byte[] deciphered = cipher.ProcessBlock(inputBytes, 0, inputBytes.Length);
            string decipheredText = utf8enc.GetString(deciphered);

            return decipheredText;
        }

        public static String Sign(String data, AsymmetricKeyParameter asymmetricKey)
        {
            /* Make the key */
            RsaKeyParameters key = (RsaKeyParameters) asymmetricKey;

            /* Init alg */
            ISigner sig = SignerUtilities.GetSigner("SHA256withRSA");

            /* Populate key */
            sig.Init(true, key);

            /* Get the bytes to be signed from the string */
            var bytes = Encoding.UTF8.GetBytes(data);

            /* Calc the signature */
            sig.BlockUpdate(bytes, 0, bytes.Length);
            byte[] signature = sig.GenerateSignature();

            /* Base 64 encode the sig so its 8-bit clean */
            var signedString = Convert.ToBase64String(signature);

            return signedString;
        }

        public static bool Verify(String data, String expectedSignature, AsymmetricKeyParameter asymmetricKey)
        {
            /* Make the key */
            RsaKeyParameters key = (RsaKeyParameters)asymmetricKey;

            /* Init alg */
            ISigner signer = SignerUtilities.GetSigner("SHA256withRSA");

            /* Populate key */
            signer.Init(false, key);

            /* Get the signature into bytes */
            var expectedSig = Convert.FromBase64String(expectedSignature);

            /* Get the bytes to be signed from the string */
            var msgBytes = Encoding.UTF8.GetBytes(data);

            /* Calculate the signature and see if it matches */
            signer.BlockUpdate(msgBytes, 0, msgBytes.Length);
            return signer.VerifySignature(expectedSig);
        }

        public static AsymmetricCipherKeyPair GenerateKeys(int keySizeInBits)
        {
            var r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(), keySizeInBits));
            AsymmetricCipherKeyPair keys = r.GenerateKeyPair();
            return keys;
        }

        public static string[] SaveToString(AsymmetricCipherKeyPair keys)
        {
            TextWriter textWriter = new StringWriter();
            var pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Private);
            pemWriter.Writer.Flush();

            var result = new string[2];
            result[0] = textWriter.ToString();

            textWriter = new StringWriter();
            pemWriter = new PemWriter(textWriter);
            pemWriter.WriteObject(keys.Public);
            pemWriter.Writer.Flush();

            result[1] = textWriter.ToString();

            return result;
        }

        public static AsymmetricCipherKeyPair GetKeys(string privateKeyString, string publicKeyString)
        {
            var publicStream = new MemoryStream(Encoding.ASCII.GetBytes(publicKeyString));
            var publicKeyStreamReader = new StreamReader(publicStream);
            var pr = new PemReader(publicKeyStreamReader);

            var privateKeyStream = new MemoryStream(Encoding.ASCII.GetBytes(privateKeyString));
            var privateKeyStreamReader = new StreamReader(privateKeyStream);
            var privateKeyReader = new PemReader(privateKeyStreamReader);

            var publicKey = (RsaKeyParameters)pr.ReadObject();
            var privateKey = ((AsymmetricCipherKeyPair)privateKeyReader.ReadObject()).Private;

            var keyPair = new AsymmetricCipherKeyPair(publicKey, privateKey);

            publicStream.Close();
            publicKeyStreamReader.Close();

            privateKeyStream.Close();
            privateKeyStreamReader.Close();

            return keyPair;
        }

        public static RsaKeyParameters GetKey(string keyString)
        {
            var privateKeyStream = new MemoryStream(Encoding.ASCII.GetBytes(keyString));
            var privateKeyStreamReader = new StreamReader(privateKeyStream);
            var privateKeyReader = new PemReader(privateKeyStreamReader);

            var rawKey = privateKeyReader.ReadObject();
            RsaKeyParameters privateKey;
            if (rawKey is RsaKeyParameters)
            {
                // Public Key
                privateKey = (RsaKeyParameters)rawKey;
            }
            else
            {
                privateKey = (RsaKeyParameters)((AsymmetricCipherKeyPair)rawKey).Private;
            }

            privateKeyStream.Close();
            privateKeyStreamReader.Close();

            return privateKey;
        }

        public static byte[] Encrypt(byte[] data, AsymmetricKeyParameter key)
        {
            var e = new RsaEngine();
            e.Init(true, key);
            int blockSize = e.GetInputBlockSize();

            var output = new List<byte>();

            for (int chunkPosition = 0; chunkPosition < data.Length; chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, data.Length - chunkPosition * blockSize);
                output.AddRange(e.ProcessBlock(data, chunkPosition, chunkSize));
            }

            return output.ToArray();
        }

        public static byte[] Decrypt(byte[] data, AsymmetricKeyParameter key)
        {
            var e = new RsaEngine();
            e.Init(false, key);

            int blockSize = e.GetInputBlockSize();

            var output = new List<byte>();

            for (int chunkPosition = 0; chunkPosition < data.Length;
                 chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, data.Length -
                                                    chunkPosition * blockSize);
                output.AddRange(e.ProcessBlock(data, chunkPosition,
                                               chunkSize));
            }

            return output.ToArray();
        }

        public static string ComputeHash(string input, string salt)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var saltArray = UrlBase64.Decode(salt);

            var saltedInput = new Byte[saltArray.Length + inputBytes.Length];
            saltArray.CopyTo(saltedInput, 0);
            inputBytes.CopyTo(saltedInput, saltArray.Length);

            Byte[] hashedBytes = GetSha512(saltedInput);

            return Encoding.ASCII.GetString(UrlBase64.Encode(hashedBytes));
        }

        private static byte[] GetSha512(byte[] key)
        {
            var digester = new SHA512Managed();
            digester.Initialize();
            return digester.ComputeHash(key);
        }

        public static string GetSha512(string key)
        {
            var digester = new SHA512Managed();
            digester.Initialize();
            return Encoding.ASCII.GetString(UrlBase64.Encode(digester.ComputeHash(Encoding.ASCII.GetBytes(key))));
        }

        public static byte[] GetSha256(byte[] key)
        {
            var digester = new SHA256Managed();
            digester.Initialize();
            return digester.ComputeHash(key);
        }

        public static string GenerateSalt()
        {
            return Encoding.ASCII.GetString(UrlBase64.Encode(GenerateSalt(64)));
        }

        public static byte[] GenerateSalt(int len)
        {
            var generator = new DigestRandomGenerator(new Sha512Digest());
            generator.AddSeedMaterial(DateTime.Now.Ticks);
            var result = new byte[len];
            generator.NextBytes(result);

            return result;
        }

        public static string GetRandomMac(int len = 4)
        {
            var str = new StringBuilder();
            for (int i = 0; i < len; i++)
            {
                str.Append(_Random.Next(0, 9));
            }

            return str.ToString();
        }

        public static string GetUserMac(string userId, long iterationNumber, int digits = 4)
        {
            //Here the system converts the iteration number to a byte[]
            byte[] iterationNumberByte = BitConverter.GetBytes(iterationNumber);
            //To BigEndian (MSB LSB)
            if (BitConverter.IsLittleEndian) Array.Reverse(iterationNumberByte);

            //Hash the userId by HMAC-SHA-1 (Hashed Message Authentication Code)
            byte[] userIdByte = Encoding.ASCII.GetBytes(userId);
            var userIdHmac = new HMACSHA1(userIdByte, true);
            byte[] hash = userIdHmac.ComputeHash(iterationNumberByte); //Hashing a message with a secret key

            //RFC4226 http://tools.ietf.org/html/rfc4226#section-5.4
            int offset = hash[hash.Length - 1] & 0xf; //0xf = 15d
            int binary =
                    ((hash[offset] & 0x7f) << 24)      //0x7f = 127d
                    | ((hash[offset + 1] & 0xff) << 16) //0xff = 255d
                    | ((hash[offset + 2] & 0xff) << 8)
                    | (hash[offset + 3] & 0xff);

            int password = binary % (int)Math.Pow(10, digits); // Shrink: 6 digits
            return password.ToString(new string('0', digits));
        }

        private static byte[] FormatKey(IEnumerable<byte> key)
        {
            var newKey = new List<byte>();
            newKey.AddRange(key);

            if (newKey.Count > KeyBitSize / 8)
            {
                throw new ArgumentException("KeyInvalid " + newKey.Count + " Req key count: " + KeyBitSize / 8, "key");
            }

            while (newKey.Count < KeyBitSize / 8)
            {
                newKey.Add(0x00);
            }

            return newKey.ToArray();
        }

        public static string AesEncrypt(string secretMessage, string key)
        {
            //User Error Checks
            var formatKey = FormatKey(Encoding.UTF8.GetBytes(key));
            var message = Encoding.UTF8.GetBytes(secretMessage);

            if (message == null || message.Length == 0)
            {
                throw new ArgumentException("Secret Message Required!", "secretMessage");
            }

            //Non-secret Payload Optional
            var nonSecretPayload = new byte[] { };

            //Using random nonce large enough not to repeat
            var nonce = new byte[NonceBitSize / 8];
            _Random.NextBytes(nonce, 0, nonce.Length);

            var cipher = new GcmBlockCipher(new AesFastEngine());
            var parameters = new AeadParameters(new KeyParameter(formatKey), MacBitSize, nonce, nonSecretPayload);
            cipher.Init(true, parameters);

            //Generate Cipher Text With Auth Tag
            var cipherText = new byte[cipher.GetOutputSize(message.Length)];
            var len = cipher.ProcessBytes(message, 0, message.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            //Assemble Message
            using (var combinedStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(combinedStream))
                {
                    //Prepend Authenticated Payload
                    binaryWriter.Write(nonSecretPayload);
                    //Prepend Nonce
                    binaryWriter.Write(nonce);
                    //Write Cipher Text
                    binaryWriter.Write(cipherText);
                }
                return Encoding.UTF8.GetString(UrlBase64.Encode(combinedStream.ToArray()));
            }
        }

        public static string AesDecrypt(string encryptedMessage, string rawKey)
        {
            //User Error Checks
            var formatKey = FormatKey(Encoding.UTF8.GetBytes(rawKey));
            formatKey = FormatKey(formatKey);

            byte[] buffer = UrlBase64.Decode(Encoding.UTF8.GetBytes(encryptedMessage));

            if (buffer == null || buffer.Length == 0)
            {
                throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");
            }

            using (var cipherStream = new MemoryStream(buffer))
            {
                using (var cipherReader = new BinaryReader(cipherStream))
                {
                    //Grab Payload
                    var nonSecretPayload = cipherReader.ReadBytes(0);

                    //Grab Nonce
                    var nonce = cipherReader.ReadBytes(NonceBitSize / 8);

                    var cipher = new GcmBlockCipher(new AesFastEngine());
                    var parameters = new AeadParameters(new KeyParameter(formatKey), MacBitSize, nonce, nonSecretPayload);
                    cipher.Init(false, parameters);

                    //Decrypt Cipher Text
                    var cipherText = cipherReader.ReadBytes(buffer.Length);
                    var plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

                    try
                    {
                        var len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                        cipher.DoFinal(plainText, len);
                    }
                    catch (InvalidCipherTextException)
                    {
                        //Return null if it doesn't authenticate
                        return null;
                    }

                    return Encoding.UTF8.GetString(plainText);
                }
            }
        }

        public static string SignWithPrivateKey(string privateKey, string data)
        {
            RSACryptoServiceProvider rsa = GetPrivateKeyProvider(privateKey);
            byte[] hash = GetSha256(Encoding.UTF8.GetBytes(data));
            var sig = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));

            return Convert.ToBase64String(sig);
        }

        public static bool VerifyWithPublicKey(string publicKey, string data, string sig)
        {
            var rsa = GetPublicKeyProvider(publicKey);
            byte[] hash = GetSha256(Encoding.UTF8.GetBytes(data));

            return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), Convert.FromBase64String(sig));
        }

        public static string SignWithPrivateKey(string privateKey, string data, ref RSACryptoServiceProvider rsa)
        {
            byte[] hash = GetSha256(Encoding.UTF8.GetBytes(data));
            var sig = rsa.SignHash(hash, CryptoConfig.MapNameToOID("SHA256"));

            return Convert.ToBase64String(sig);
        }

        public static bool VerifyWithPublicKey(string publicKey, string data, string sig,
            ref RSACryptoServiceProvider rsa)
        {
            byte[] hash = GetSha256(Encoding.UTF8.GetBytes(data));
            return rsa.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA256"), Convert.FromBase64String(sig));
        }

        public static RSACryptoServiceProvider GetPublicKeyProvider(string publicKey)
        {
            RsaKeyParameters kparam;

            using (var stringReader = new StringReader(publicKey))
            {
                var pemParser = new PemReader(stringReader);
                kparam = (RsaKeyParameters)pemParser.ReadObject();
            }

            RSAParameters p1 = DotNetUtilities.ToRSAParameters(kparam);
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(p1);

            return rsa;
        }

        public static RSACryptoServiceProvider GetPrivateKeyProvider(string privateKey)
        {
            RSACryptoServiceProvider rsa;
            using (var keyreader = new StringReader(privateKey))
            {
                var pemreader = new PemReader(keyreader);
                var rsaPrivKey = (AsymmetricCipherKeyPair)pemreader.ReadObject();
                rsa = (RSACryptoServiceProvider)RSA.Create();
                var rsaParameters = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)rsaPrivKey.Private);
                rsa.ImportParameters(rsaParameters);
            }

            return rsa;
        }

        public static string CalculateMd5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string

            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("X2"));
            }

            return sb.ToString();
        }
    }
}
