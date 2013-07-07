// Copyright (c) 2013 Andrey Kurdyumov (kant2002@gmail.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ICSharpCode.ILSpy.TreeNodes
{
    public class UnicodeSupport
    {
        /// <summary>
        /// Replaces control codes and other non visible characters to unicode sequence.
        /// </summary>
        /// <param name="identifier">Identiifer which should be converted.</param>
        /// <returns>Converted identifier.</returns>
        public static string FormatUnicodeIdentifier(string identifier)
        {
            var sb = new StringBuilder();
            foreach (var c in identifier.ToCharArray())
            {
                if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' ')
                {
                    sb.Append(c);
                }
                else
                {
                    sb.Append(@"\u");
                    sb.AppendFormat("{0:X4}", (int)c);
                }
            }

            return sb.ToString();
        }
    }
}
