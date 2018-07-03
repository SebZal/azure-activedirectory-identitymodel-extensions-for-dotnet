//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

using Microsoft.IdentityModel.Tests;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens.Jwt.Tests;
using System.Security.Claims;
using Xunit;

#pragma warning disable CS3016 // Arrays as attribute arguments is not CLS-compliant

namespace Microsoft.IdentityModel.JsonWebTokens.Tests
{
    public class JsonWebTokenHandlerTests
    {
        [Theory, MemberData(nameof(SegmentTheoryData))]
        public void SegmentCanRead(JwtTheoryData theoryData)
        {
            var context = TestUtilities.WriteHeader($"{this}.SegmentCanRead", theoryData);

            var handler = new JsonWebTokenHandler();
            if (theoryData.CanRead != handler.CanReadToken(theoryData.Token))
                context.Diffs.Add($"theoryData.CanRead != handler.CanReadToken(theoryData.Token))");

            TestUtilities.AssertFailIfErrors(context);
        }

        public static TheoryData<JwtTheoryData> SegmentTheoryData()
        {
            var theoryData = new TheoryData<JwtTheoryData>();

            JwtTestData.InvalidRegExSegmentsData(theoryData);
            JwtTestData.InvalidNumberOfSegmentsData("IDX14110:", theoryData);
            JwtTestData.InvalidEncodedSegmentsData("", theoryData);
            JwtTestData.ValidEncodedSegmentsData(theoryData);

            return theoryData;
        }

        // Tests checks to make sure that the token string created by the JsonWebTokenHandler is consistent with the 
        // token string created by the JwtSecurityTokenHandler.
        [Theory, MemberData(nameof(CreateJWETheoryData))]
        public void CreateJWE(CreateTokenTheoryData theoryData)
        {
            var context = TestUtilities.WriteHeader($"{this}.CreateJWE", theoryData);
            try
            {
                string jweFromJwtHandler = theoryData.JwtSecurityTokenHandler.CreateEncodedJwt(theoryData.TokenDescriptor);
                string jweFromJsonHandler = theoryData.JsonWebTokenHandler.CreateJsonWebToken(theoryData.Payload, theoryData.TokenDescriptor.SigningCredentials, theoryData.TokenDescriptor.EncryptingCredentials);

                theoryData.JwtSecurityTokenHandler.ValidateToken(jweFromJwtHandler, theoryData.ValidationParameters, out SecurityToken validatedToken);
                theoryData.JsonWebTokenHandler.ValidateToken(jweFromJsonHandler, theoryData.ValidationParameters);

                theoryData.ExpectedException.ProcessNoException(context);
                var jweTokenFromJwtHandler = new JsonWebToken(jweFromJwtHandler);
                var jweTokenFromJsonHandler = new JsonWebToken(jweFromJsonHandler);

                context.PropertiesToIgnoreWhenComparing = new Dictionary<Type, List<string>>
                {
                    { typeof(JsonWebToken), new List<string> { "EncodedToken" } },
                };

                IdentityComparer.AreEqual(jweTokenFromJwtHandler, jweTokenFromJsonHandler, context);
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex, context);
            }

            TestUtilities.AssertFailIfErrors(context);
        }


        public static TheoryData<CreateTokenTheoryData> CreateJWETheoryData
        {
            get
            {
                var tokenHandler = new JwtSecurityTokenHandler
                {
                    SetDefaultTimesOnTokenCreation = false
                };

                tokenHandler.InboundClaimTypeMap.Clear();

                return new TheoryData<CreateTokenTheoryData>
                {
                    new CreateTokenTheoryData
                    {
                        First = true,
                        Payload = Default.Payload,
                        TokenDescriptor =  new SecurityTokenDescriptor
                        {
                            SigningCredentials = KeyingMaterial.JsonWebKeyRsa256SigningCredentials,
                            EncryptingCredentials = KeyingMaterial.DefaultSymmetricEncryptingCreds_Aes256_Sha512_512,
                            Subject = new ClaimsIdentity(Default.PayloadClaims),
                        },
                        JsonWebTokenHandler = new JsonWebTokenHandler(),
                        JwtSecurityTokenHandler = tokenHandler,
                        ValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = KeyingMaterial.JsonWebKeyRsa256SigningCredentials.Key,
                            TokenDecryptionKey = KeyingMaterial.DefaultSymmetricSecurityKey_512,
                            ValidAudience = Default.Audience,
                            ValidIssuer = Default.Issuer
                        }
                    },
                };
            }
        }

        // Tests checks to make sure that the token string created by the JsonWebTokenHandler is consistent with the 
        // token string created by the JwtSecurityTokenHandler.
        [Theory, MemberData(nameof(CreateJWSTheoryData))]
        public void CreateJWS(CreateTokenTheoryData theoryData)
        {
            var context = TestUtilities.WriteHeader($"{this}.CreateJWS", theoryData);
            try
            {
                string jwsFromJwtHandler = theoryData.JwtSecurityTokenHandler.CreateEncodedJwt(theoryData.TokenDescriptor);
                string jwsFromJsonHandler = theoryData.JsonWebTokenHandler.CreateToken(theoryData.Payload, KeyingMaterial.JsonWebKeyRsa256SigningCredentials);

                theoryData.JwtSecurityTokenHandler.ValidateToken(jwsFromJwtHandler, theoryData.ValidationParameters, out SecurityToken validatedToken);
                theoryData.JsonWebTokenHandler.ValidateToken(jwsFromJsonHandler, theoryData.ValidationParameters);

                theoryData.ExpectedException.ProcessNoException(context);
                var jwsTokenFromJwtHandler = new JsonWebToken(jwsFromJwtHandler);
                var jwsTokenFromHandler = new JsonWebToken(jwsFromJsonHandler);
                IdentityComparer.AreEqual(jwsTokenFromJwtHandler, jwsTokenFromHandler, context);
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex, context);
            }

            TestUtilities.AssertFailIfErrors(context);
        }


        public static TheoryData<CreateTokenTheoryData> CreateJWSTheoryData
        {
            get
            {
                var tokenHandler = new JwtSecurityTokenHandler
                {
                    SetDefaultTimesOnTokenCreation = false
                };

                tokenHandler.InboundClaimTypeMap.Clear();

                return new TheoryData<CreateTokenTheoryData>
                {
                    new CreateTokenTheoryData
                    {
                        First = true,
                        Payload = Default.Payload,
                        TokenDescriptor =  new SecurityTokenDescriptor
                        {
                            SigningCredentials = KeyingMaterial.JsonWebKeyRsa256SigningCredentials,
                            Subject = new ClaimsIdentity(Default.PayloadClaims)
                        },
                        JsonWebTokenHandler = new JsonWebTokenHandler(),
                        JwtSecurityTokenHandler = tokenHandler,
                        ValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningKey = KeyingMaterial.JsonWebKeyRsa256SigningCredentials.Key,
                            ValidAudience = Default.Audience,
                            ValidIssuer = Default.Issuer
                        }
                    },
                };
            }
        }

        // Test checks to make sure that the token payload retrieved from ValidateToken is the same as the payload
        // the token was initially created with. 
        [Fact]
        public void RoundTripJWS()
        {
            TestUtilities.WriteHeader($"{this}.RoundTripToken");
            var context = new CompareContext();

            var tokenHandler = new JsonWebTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidAudience = Default.Audience,
                ValidIssuer = Default.Issuer,
                IssuerSigningKey = KeyingMaterial.JsonWebKeyRsa256SigningCredentials.Key,
            };

            string jwtString = tokenHandler.CreateToken(Default.Payload, KeyingMaterial.JsonWebKeyRsa256SigningCredentials);
            var tokenValidationResult = tokenHandler.ValidateToken(jwtString, tokenValidationParameters);
            var validatedToken = tokenValidationResult.SecurityToken as JsonWebToken;
            IdentityComparer.AreEqual(Default.Payload, validatedToken.Payload, context);
            TestUtilities.AssertFailIfErrors(context);
        }

        [Theory, MemberData(nameof(RoundTripJWETheoryData))]
        public void RoundTripJWE(CreateTokenTheoryData theoryData)
        {
            var context = TestUtilities.WriteHeader($"{this}.RoundTripJWE", theoryData);
            var handler = new JsonWebTokenHandler();
            var jweCreatedInMemory = handler.CreateJsonWebToken(theoryData.Payload, theoryData.SigningCredentials, theoryData.EncryptingCredentials);
            var jweCreatedInMemoryToken = new JsonWebToken(jweCreatedInMemory);
            try
            {
                var tokenValidationResult = handler.ValidateToken(jweCreatedInMemory, theoryData.ValidationParameters);
                var outerToken = tokenValidationResult.SecurityToken as JsonWebToken;

                Assert.True(outerToken != null, "ValidateToken should not return a null token for the JWE token.");
                TestUtilities.CallAllPublicInstanceAndStaticPropertyGets(outerToken, theoryData.TestId);

                Assert.True(outerToken.InnerToken != null, "ValidateToken should not return a null token for the inner JWE token.");
                TestUtilities.CallAllPublicInstanceAndStaticPropertyGets(outerToken.InnerToken, theoryData.TestId);

                context.PropertiesToIgnoreWhenComparing = new Dictionary<Type, List<string>>
                {
                    { typeof(JsonWebToken), new List<string> { "EncodedToken" } },
                };

                if (!IdentityComparer.AreEqual(jweCreatedInMemoryToken.Payload, outerToken.Payload, context))
                    context.Diffs.Add("jweCreatedInMemory.Payload != jweValidated.Payload");

                if (!IdentityComparer.AreEqual(jweCreatedInMemoryToken.Payload, outerToken.InnerToken.Payload, context))
                    context.Diffs.Add("jweCreatedInMemory.Payload != jweValidated.InnerToken.Payload");

                TestUtilities.AssertFailIfErrors(string.Format(CultureInfo.InvariantCulture, "RoundTripJWE: "), context.Diffs);
            }
            catch (Exception ex)
            {
                theoryData.ExpectedException.ProcessException(ex, context);
            }
        }

        public static TheoryData<CreateTokenTheoryData> RoundTripJWETheoryData
        {
            get
            {
                return new TheoryData<CreateTokenTheoryData>
                {
                    new CreateTokenTheoryData()
                    {
                        TestId = "RoundTripJWEValid",
                        ValidationParameters = Default.SymmetricEncryptSignTokenValidationParameters,
                        Payload = Default.Payload,
                        SigningCredentials = Default.SymmetricSigningCredentials,
                        EncryptingCredentials = Default.SymmetricEncryptingCredentials
                    }
                };
            }
        }

        // Test checks to make sure that an access token can be successfully validated by the JsonWebTokenHandler.
        // Also ensures that a non-standard claim can be successfully retrieved from the payload and validated.
        [Fact]
        public void ValidateJWS()
        {
            TestUtilities.WriteHeader($"{this}.ValidateToken");

            var tokenHandler = new JsonWebTokenHandler();
            var accessToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IlJzYVNlY3VyaXR5S2V5XzIwNDgiLCJ0eXAiOiJKV1QifQ.eyJlbWFpbCI6IkJvYkBjb250b3NvLmNvbSIsImdpdmVuX25hbWUiOiJCb2IiLCJpc3MiOiJodHRwOi8vRGVmYXVsdC5Jc3N1ZXIuY29tIiwiYXVkIjoiaHR0cDovL0RlZmF1bHQuQXVkaWVuY2UuY29tIiwibmJmIjoiMTQ4OTc3NTYxNyIsImV4cCI6IjE2MTYwMDYwMTcifQ.GcIi6FGp1JS5VF70_ULa8g6GTRos9Y7rUZvPAo4hm10bBNfGhdd5uXgsJspiQzS8vwJQyPlq8a_BpL9TVKQyFIRQMnoZWe90htmNWszNYbd7zbLJZ9AuiDqDzqzomEmgcfkIrJ0VfbER57U46XPnUZQNng2XgMXrXmIKUqEph_vLGXYRQ4ndfwtRrR6BxQFd1PS1T5KpEoUTusI4VEsMcutzfXUygLDiRKIcnLFA0kQpeoHllO4Nb_Sxv63GCb0d1076FfSEYtyRxF4YSCz1In-ee5dwEK8Mw3nHscu-1hn0Fe98RBs-4OrUzI0WcV8mq9IIB3i-U-CqCJEP_hVCiA";
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidAudience = "http://Default.Audience.com",
                ValidIssuer = "http://Default.Issuer.com",
                IssuerSigningKey = KeyingMaterial.JsonWebKeyRsa256SigningCredentials.Key,
            };
            var tokenValidationResult = tokenHandler.ValidateToken(accessToken, tokenValidationParameters);
            var jsonWebToken = tokenValidationResult.SecurityToken as JsonWebToken;
            var email = jsonWebToken.Payload.Value<string>(JwtRegisteredClaimNames.Email);

            if (!email.Equals("Bob@contoso.com"))
                throw new SecurityTokenException("Token does not contain the correct value for the 'email' claim.");
        }
    }

    public class CreateTokenTheoryData : TheoryDataBase
    {
        public JObject Payload { get; set; }

        public EncryptingCredentials EncryptingCredentials { get; set; }

        public SigningCredentials SigningCredentials { get; set; }

        public SecurityTokenDescriptor TokenDescriptor { get; set; }

        public JsonWebTokenHandler JsonWebTokenHandler { get; set; }

        public JwtSecurityTokenHandler JwtSecurityTokenHandler { get; set; }

        public TokenValidationParameters ValidationParameters { get; set; }
    }
}
