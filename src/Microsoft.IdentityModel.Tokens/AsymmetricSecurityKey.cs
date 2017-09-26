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

namespace Microsoft.IdentityModel.Tokens
{
    /// <summary>
    /// Base class for a Security Key that contains Asymmetric key material.
    /// </summary>
    public abstract class AsymmetricSecurityKey : SecurityKey
    {
        /// <summary>
        /// This must be overridden to get a bool indicating if a private key exists.
        /// </summary>
        /// <return>true if it has a private key; otherwise, false.</return>
        [System.Obsolete("HasPrivateKey method is deprecated, please use FoundPrivateKey instead.")]
        public abstract bool HasPrivateKey { get; }

        /// <summary>
        /// This must be overridden to get an enum indicating if a private key exists.
        /// </summary>
        /// <return>'Yes' if private key exists for sure; 'No' if private key doesn't exist for sure; 'Unknown' if we cannot determine.</return>
        public abstract PrivateKeyExistence FoundPrivateKey { get; }
    }

    /// <summary>
    /// Enum for the existence of private key
    /// </summary>
    public enum PrivateKeyExistence
    {
        /// <summary>
        /// private key exists for sure
        /// </summary>
        Yes,
        /// <summary>
        /// private key doesn't exist for sure
        /// </summary>
        No,
        /// <summary>
        /// unable to determine the existence of private key
        /// </summary>
        Unknown
    };
}
