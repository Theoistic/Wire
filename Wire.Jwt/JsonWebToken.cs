using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace Wire.Jwt
{
    /// <summary>
    /// Used to create, decode and validate JSON Web Tokens.
    /// </summary>
    public class JsonWebToken
    {
        /// <summary>
        /// Algorithm methods supported by <see cref="JsonWebToken"/>.
        /// </summary>
        public enum AlgorithmMethod
        {
            HS256,
            HS384,
            HS512
        }

        public class Base64Url
        {
            /// <summary>
            /// Encode input string using Base64 URL encoding.
            /// </summary>
            /// <param name="input">Byte representation of the string to encode.</param>
            /// <returns>Encoded string.</returns>
            public string Encode(byte[] input)
            {
                string output = Convert.ToBase64String(input);
                output = output.Split('=')[0];     // Remove any trailing '='s
                output = output.Replace('+', '-'); // 62nd char of encoding
                output = output.Replace('/', '_'); // 63rd char of encoding

                return output;
            }

            /// <summary>
            /// Decode a Base64 URL encoded string.
            /// </summary>
            /// <param name="input">Encoded string.</param>
            /// <returns>Decoded string int a byte array representation.</returns>
            public byte[] Decode(string input)
            {
                var output = input;
                output = output.Replace('-', '+'); // 62nd char of encoding
                output = output.Replace('_', '/'); // 63rd char of encoding

                switch (output.Length % 4)         // Pad with trailing '='s
                {
                    case 0: break;                 // No pad chars in this case
                    case 2: output += "=="; break; // Two pad chars
                    case 3: output += "="; break;  // One pad char
                    default: throw new FormatException("Invalid base64url string.");
                }

                return Convert.FromBase64String(output);
            }
        }

        /// <summary>
        /// Should be trown if <see cref="RegisteredClaims.ExpirationTime"/> is invalid when
        /// the property <see cref="TokenInformation.HasExpired"/> is accessed.
        /// </summary>
        public class InvalidExpirationTimeException : Exception
        {
            /// <summary>
            /// Creates a new instance of <see cref="InvalidExpirationTimeException"/>.
            /// </summary>
            /// <param name="invalidExpirationTime">The invalid <see cref="RegisteredClaims.ExpirationTime"/>.</param>
            public InvalidExpirationTimeException(object invalidExpirationTime)
                : base("Invalid expiration time.")
            {
                InvalidExpirationTime = invalidExpirationTime;
            }

            /// <summary>
            /// The invalid expiration time fond in the token payload.
            /// </summary>
            public object InvalidExpirationTime { get; set; }
        }

        /// <summary>
        /// Should be thrown if the token signature is not valid.
        /// </summary>
        public class InvalidSignatureException : Exception
        {
            public InvalidSignatureException(string signature, string expected)
                : base("Invalid signature.")
            {
                InvalidSignature = signature;
                ExpectedSignature = expected;
            }

            /// <summary>
            /// Invalid signature found in token.
            /// </summary>
            public string InvalidSignature { get; private set; }

            /// <summary>
            /// Expected signature to consider the token as valid.
            /// </summary>
            public string ExpectedSignature { get; private set; }
        }

        /// <summary>
        /// The following Claim Names are registered in the IANA "JSON Web Token
        /// Claims" registry established by Section 10.1.  None of the claims
        /// defined below are intended to be mandatory to use or implement in all
        /// cases, but rather they provide a starting point for a set of useful,
        /// interoperable claims.Applications using JWTs should define which
        /// specific claims they use and when they are required or optional.All
        /// the names are short because a core goal of JWTs is for the
        /// representation to be compact.
        /// </summary>
        public static class RegisteredClaims
        {
            /// <summary>
            /// The "iss" (issuer) claim identifies the principal that issued the
            /// JWT.The processing of this claim is generally application specific.
            /// The "iss" value is a case-sensitive string containing a StringOrURI
            /// value.Use of this claim is OPTIONAL.
            /// </summary>
            public static string Issuer = "iss";

            /// <summary>
            /// The "sub" (subject) claim identifies the principal that is the
            /// subject of the JWT.The claims in a JWT are normally statements
            /// about the subject.  The subject value MUST either be scoped to be
            /// locally unique in the context of the issuer or be globally unique.
            /// The processing of this claim is generally application specific.The
            /// "sub" value is a case-sensitive string containing a StringOrURI
            /// value.Use of this claim is OPTIONAL.
            /// </summary>
            public static string Subject = "sub";

            /// <summary>
            /// The "aud" (audience) claim identifies the recipients that the JWT is
            /// intended for.  Each principal intended to process the JWT MUST
            /// identify itself with a value in the audience claim.If the principal
            /// processing the claim does not identify itself with a value in the
            /// "aud" claim when this claim is present, then the JWT MUST be
            /// rejected.In the general case, the "aud" value is an array of case-
            /// sensitive strings, each containing a StringOrURI value.In the
            /// special case when the JWT has one audience, the "aud" value MAY be a
            /// single case-sensitive string containing a StringOrURI value.The
            /// interpretation of audience values is generally application specific.
            /// Use of this claim is OPTIONAL.
            /// </summary>
            public static string Audience = "aud";

            /// <summary>
            /// The "exp" (expiration time) claim identifies the expiration time on
            /// or after which the JWT MUST NOT be accepted for processing.The
            /// processing of the "exp" claim requires that the current date/time
            /// MUST be before the expiration date/time listed in the "exp" claim.
            /// Implementers MAY provide for some small leeway, usually no more than
            /// a few minutes, to account for clock skew.Its value MUST be a number
            /// containing a NumericDate value.Use of this claim is OPTIONAL.
            /// </summary>
            public static string ExpirationTime = "exp";

            /// <summary>
            /// The "nbf" (not before) claim identifies the time before which the JWT
            /// MUST NOT be accepted for processing.The processing of the "nbf"
            /// claim requires that the current date/time MUST be after or equal to
            /// the not-before date/time listed in the "nbf" claim.Implementers MAY
            /// provide for some small leeway, usually no more than a few minutes, to
            /// account for clock skew.Its value MUST be a number containing a
            /// NumericDate value.Use of this claim is OPTIONAL.
            /// </summary>
            public static string NotBefore = "nbf";

            /// <summary>
            /// The "iat" (issued at) claim identifies the time at which the JWT was
            /// issued.This claim can be used to determine the age of the JWT.Its
            /// value MUST be a number containing a NumericDate value.Use of this
            /// claim is OPTIONAL.
            /// </summary>
            public static string IssuedAt = "iat";

            /// <summary>
            /// The "jti" (JWT ID) claim provides a unique identifier for the JWT.
            /// The identifier value MUST be assigned in a manner that ensures that
            /// there is a negligible probability that the same value will be
            /// accidentally assigned to a different data object; if the application
            /// uses multiple issuers, collisions MUST be prevented among values
            /// produced by different issuers as well.The "jti" claim can be used
            /// to prevent the JWT from being replayed.The "jti" value is a case-
            /// sensitive string.  Use of this claim is OPTIONAL.
            /// </summary>
            public static string JWTID = "jti";
        }

        public sealed class TokenInformation
        {
            public TokenInformation(
                Dictionary<string, string> header, Dictionary<string, object> claims)
            {
                Header = new ReadOnlyDictionary<string, string>(header);
                Claims = new ReadOnlyDictionary<string, object>(claims);
            }

            /// <summary>
            /// Read-only dictionary that contains the header information.
            /// </summary>
            public IReadOnlyDictionary<string, string> Header { get; private set; }

            /// <summary>
            /// Read-only dictionary that contains all the token claims.
            /// </summary>
            public IReadOnlyDictionary<string, object> Claims { get; private set; }

            /// <summary>
            /// Verifies if the <see cref="RegisteredClaims.ExpirationTime"/> is less than
            /// the current UTC time.
            /// </summary>
            public bool HasExpired
            {
                get
                {
                    var expirationTime = GetExpirationTime();
                    if (expirationTime.HasValue)
                    {
                        return expirationTime.Value < UnixTimeStamp.ToUnixTimeStamp(DateTime.UtcNow);
                    }

                    return false;
                }
            }

            /// <summary>
            /// Gets the Unix TimeStamp information from the claims dictionary
            /// and returns it as <see cref="DateTime"/> object.
            /// </summary>
            public DateTime? ExpiresOn
            {
                get
                {
                    var expirationTime = GetExpirationTime();
                    if (expirationTime.HasValue)
                    {
                        return UnixTimeStamp.ToDateTime(expirationTime.Value);
                    }

                    return null;
                }
            }

            /// <summary>
            /// Extract the <see cref="RegisteredClaims.ExpirationTime"/> from the claims dictionary if it is valid.
            /// </summary>
            /// <param name="claims">Claims information, also known as JWT payload.</param>
            /// <returns>Expiration time in Unix TimeStamp format or <code>null</code> if not found or invalid.</returns>
            private long? GetExpirationTime()
            {
                if (Claims.ContainsKey(RegisteredClaims.ExpirationTime))
                {
                    var expirationValue = Claims[RegisteredClaims.ExpirationTime].ToString();

                    long unixTime = 0;
                    if (!long.TryParse(expirationValue, out unixTime))
                    {
                        throw new InvalidExpirationTimeException(expirationValue);
                    }

                    return unixTime;
                }

                return null;
            }
        }

        public static class UnixTimeStamp
        {
            private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            /// <summary>
            /// Converts <see cref="DateTime"/> to Unix TimeStamp format.
            /// </summary>
            /// <param name="date">Microsoft .NET DateTime format.</param>
            /// <returns>Unix TimeStamp format.</returns>
            public static long ToUnixTimeStamp(DateTime date)
            {
                return (long)Math.Round((date - UnixEpoch).TotalSeconds);
            }

            /// <summary>
            /// Converts from Unix TimeStamp format to <see cref="DateTime"/>.
            /// </summary>
            /// <param name="unixTimeStamp">Unix TimeStamp value.</param>
            /// <returns>Current time in <see cref="DateTime"/> format.</returns>
            public static DateTime ToDateTime(long unixTimeStamp)
            {
                return UnixEpoch.AddSeconds(unixTimeStamp);
            }
        }



        private readonly Base64Url _base64Url;
        //private readonly JavaScriptSerializer _serializer;

        public JsonWebToken()
        {
            _base64Url = new Base64Url();
            //_serializer = new JavaScriptSerializer();
        }

        /// <summary>
        /// Creates a JWT token in the format of {header}.{claims}.{signature}.
        /// </summary>
        /// <param name="key">The key used to sign the token.</param>
        /// <returns>JWT token in the format of {header}.{claims}.{signature}.</returns>
        public string CreateToken(byte[] key)
        {
            return CreateToken(key, null, AlgorithmMethod.HS256, null);
        }

        /// <summary>
        /// Creates a JWT token in the format of {header}.{claims}.{signature}.
        /// </summary>
        /// <param name="key">The key used to sign the token.</param>
        /// <param name="expirationTime">The <see cref="RegisteredClaims.ExpirationTime"/>.</param>
        /// <returns>JWT token in the format of {header}.{claims}.{signature}.</returns>
        public string CreateToken(byte[] key, DateTime? expirationTime)
        {
            return CreateToken(key, null, AlgorithmMethod.HS256, expirationTime);
        }

        /// <summary>
        /// Creates a JWT token in the format of {header}.{claims}.{signature}.
        /// </summary>
        /// <param name="claims">User claims, also known as token payload information.</param>
        /// <param name="key">The key used to sign the token.</param>
        /// <returns>JWT token in the format of {header}.{claims}.{signature}.</returns>
        public string CreateToken(byte[] key, Dictionary<string, object> claims)
        {
            return CreateToken(key, claims, AlgorithmMethod.HS256, null);
        }

        /// <summary>
        /// Creates a JWT token in the format of {header}.{claims}.{signature}.
        /// </summary>
        /// <param name="claims">User claims, also known as token payload information.</param>
        /// <param name="key">The key used to sign the token.</param>
        /// <param name="expirationTime">The <see cref="RegisteredClaims.ExpirationTime"/>.</param>
        /// <returns>JWT token in the format of {header}.{claims}.{signature}.</returns>
        public string CreateToken(byte[] key, Dictionary<string, object> claims, DateTime? expirationTime)
        {
            return CreateToken(key, claims, AlgorithmMethod.HS256, expirationTime);
        }

        /// <summary>
        /// Creates a JWT token in the format of {header}.{claims}.{signature}.
        /// </summary>
        /// <param name="key">The key used to sign the token.</param>
        /// <param name="method">Algoritm method to be used.</param>
        /// <returns>JWT token in the format of {header}.{claims}.{signature}.</returns>
        public string CreateToken(byte[] key, AlgorithmMethod method)
        {
            return CreateToken(key, null, method, null);
        }

        /// <summary>
        /// Creates a JWT token in the format of {header}.{claims}.{signature}.
        /// </summary>
        /// <param name="key">The key used to sign the token.</param>
        /// <param name="claims">User claims, also known as token payload information.</param>
        /// <param name="method">Algoritm method to be used.</param>
        /// <returns>JWT token in the format of {header}.{claims}.{signature}.</returns>
        public string CreateToken(byte[] key, Dictionary<string, object> claims, AlgorithmMethod method)
        {
            return CreateToken(key, claims, method, null);
        }

        /// <summary>
        /// Creates a JWT token in the format of {header}.{claims}.{signature}.
        /// </summary>
        /// <param name="claims">User claims, also known as token payload information.</param>
        /// <param name="method">Algoritm method to be used.</param>
        /// <param name="key">The key used to sign the token.</param>
        /// <param name="expirationTime">The <see cref="RegisteredClaims.ExpirationTime"/>.</param>
        /// <returns>JWT token in the format of {header}.{claims}.{signature}.</returns>
        public string CreateToken(byte[] key, Dictionary<string, object> claims, AlgorithmMethod method, DateTime? expirationTime)
        {
            if (claims == null)
            {
                claims = new Dictionary<string, object>();
            }

            var header = new Dictionary<string, string>()
            {
                { "alg", method.ToString() },
                { "typ", "JWT" }
            };

            IncludeExpirationTime(claims, expirationTime);

            var encodedHeader = _base64Url.Encode(GetBytes(JsonConvert.SerializeObject(header)));
            var encodedPayload = _base64Url.Encode(GetBytes(JsonConvert.SerializeObject(claims)));
            var encodedSignature = CreateSignature(method, key, encodedHeader, encodedPayload);

            return $"{encodedHeader}.{encodedPayload}.{encodedSignature}";
        }

        /// <summary>
        /// Decode token, validates it and returns the user claims in a <see cref="Dictionary{String, Object}"/>.
        /// </summary>
        /// <param name="token">Encoded JWT token.</param>
        /// <returns>User claims as populated in a <see cref="Dictionary{String, Object}"/>.</returns>
        public TokenInformation Decode(string token)
        {
            return Decode(token, null);
        }

        /// <summary>
        /// Decode token, validates it and returns the user claims in a <see cref="Dictionary{String, Object}"/>.
        /// </summary>
        /// <param name="token">Encoded JWT token.</param>
        /// <param name="key">Key used to validate the token signature.</param>
        /// <param name="validateSignature"><code>true</code> to validate the token signature.</param>
        /// <returns>User claims as populated in a <see cref="Dictionary{String, Object}"/>.</returns>
        public TokenInformation Decode(string token, byte[] key)
        {
            var parts = token.Split('.');

            var header = parts[0];
            var claims = parts[1];
            var signature = parts[2];

            var decodedHeader = _base64Url.Decode(parts[0]);
            var decodedClaims = _base64Url.Decode(parts[1]);

            var headerDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(GetString(decodedHeader));
            var claimsDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(GetString(decodedClaims));

            if (key != null)
            {
                AlgorithmMethod algorithm;

                if (!Enum.TryParse(headerDictionary["alg"], out algorithm))
                {
                    throw new NotImplementedException($"Algorithm not implemented: {headerDictionary["alg"]}");
                }

                var expectedSignature = CreateSignature(algorithm, key, header, claims);

                if (!string.Equals(signature, expectedSignature))
                {
                    throw new InvalidSignatureException(signature, expectedSignature);
                }
            }

            return new TokenInformation(headerDictionary, claimsDictionary);
        }

        /// <summary>
        /// Creates an instance of <see cref="System.Security.Cryptography.HMAC"/> based on <see cref="AlgorithmMethod"/>.)
        /// </summary>
        /// <param name="method">Algorithm method.</param>
        /// <param name="key">Key used to instanciate the <see cref="System.Security.Cryptography.HMAC"/> algorithm class.</param>
        /// <returns>A new instance of <see cref="System.Security.Cryptography.HMAC"/>.</returns>
        private HMAC CreateAlgorithm(AlgorithmMethod method, byte[] key)
        {
            switch (method)
            {
                case AlgorithmMethod.HS256: return new HMACSHA256(key);
                case AlgorithmMethod.HS384: return new HMACSHA384(key);
                default: return new HMACSHA512(key);
            }
        }

        /// <summary>
        /// Get the bytes representation of the UTF8 string.
        /// </summary>
        /// <param name="value">String to be encoded to bytes.</param>
        /// <returns>Byte array representation of the string.</returns>
        private byte[] GetBytes(string value)
        {
            return Encoding.UTF8.GetBytes(value);
        }

        /// <summary>
        /// Get the bytes representation of the UTF8 string.
        /// </summary>
        /// <param name="value">String to be encoded to bytes.</param>
        /// <returns>Byte array representation of the string.</returns>
        private string GetString(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// Creates a valid signature based on the algorithm, key, header and claims/payload.
        /// </summary>
        /// <param name="method">Algorith used to calculate the signature hash.</param>
        /// <param name="key">Key used to hash the signature.</param>
        /// <param name="encodedHeader">Encoded JWT header.</param>
        /// <param name="encodedClaims">Encoded JWT payload or claims.</param>
        /// <returns></returns>
        private string CreateSignature(AlgorithmMethod method, byte[] key, string encodedHeader, string encodedClaims)
        {
            return _base64Url.Encode(CreateAlgorithm(method, key).ComputeHash(GetBytes($"{encodedHeader}.{encodedClaims}")));
        }

        /// <summary>
        /// Includes or overrides the expiration time for a given payload/claims. See <see cref="RegisteredClaims.ExpirationTime"/>.
        /// </summary>
        /// <param name="claims">Claims information, also known as JWT payload.</param>
        /// <param name="expirationTime">Expiration time in the format of <see cref="DateTime"/>. It will be converted to Unix Time.</param>
        private void IncludeExpirationTime(Dictionary<string, object> claims, DateTime? expirationTime)
        {
            if (expirationTime.HasValue)
            {
                long unixTimeStamp = UnixTimeStamp.ToUnixTimeStamp(expirationTime.Value);
                if (claims.ContainsKey(RegisteredClaims.ExpirationTime))
                {
                    claims[RegisteredClaims.ExpirationTime] = unixTimeStamp;
                }
                else
                {
                    claims.Add(RegisteredClaims.ExpirationTime, unixTimeStamp);
                }
            }
        }

        public static string Sha256(string str)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(str), 0, Encoding.UTF8.GetByteCount(str));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public static string Secret => Sha256(typeof(JsonWebToken).AssemblyQualifiedName);

        public static string NewToken(Dictionary<string, object> claims)
        {
            var key = Encoding.UTF8.GetBytes(JsonWebToken.Secret);
            return new JsonWebToken().CreateToken(key, claims, DateTime.UtcNow.AddMinutes(10));
        }

        public static TokenInformation DecodeToken(string token)
        {
            var key = Encoding.UTF8.GetBytes(JsonWebToken.Secret);
            return new JsonWebToken().Decode(token, key);
        }
    }
}
