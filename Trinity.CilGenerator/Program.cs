﻿// LICENSE:
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// AUTHORS:
//
//  Moritz Eberl <moritz@semiodesk.com>
//  Sebastian Faubel <sebastian@semiodesk.com>
//
// Copyright (c) Semiodesk GmbH 2015-2019

using System;
using System.Diagnostics;
using System.IO;
using Mono.Options;
using Semiodesk.Trinity.CilGenerator.Loggers;

namespace Semiodesk.Trinity.CilGenerator
{
    class Program
    {
        #region Methods
        static int Main(string[] args)
        {
            var input = "";
            var output = "";

            var help = false;
            var writeSymbols = true;
            var overwriteInput = false;

            var options = new OptionSet()
            {
                { "i|input=", "Input assembly.", v => input = v },
                { "o|output=", "Output assembly.", v => output = v },
                { "O|overwrite", "Overwrite input assembly.", v => overwriteInput = true },
                { "s|no-symbols", "Do not generate debug symbols.", v => writeSymbols = false },
                { "h|help",  "Show this message and exit.", v => help = v != null }
            };

            try
            {
                options.Parse(args);

                if (!help && !string.IsNullOrEmpty(input) && (!string.IsNullOrEmpty(output) || overwriteInput))
                {
                    if (!File.Exists(input))
                    {
                        Console.WriteLine("Error: Source file does not exist: {0}", input);

                        return -1;
                    }

                    var generator = new ILGenerator(new ConsoleLogger(), writeSymbols);

                    if (generator.ProcessFile(input, output))
                    {
                        return 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            catch (OptionException e)
            {
                Debug.WriteLine(e.Message);
            }

            Console.WriteLine("CIL Generator, Copyright (C) 2015-2019 Semiodesk GmbH");
            Console.WriteLine();
            Console.WriteLine("Generate byte code for Semiodesk Trinity specific attributes in CIL assemblies.");
            Console.WriteLine("Usage: cilg.exe [OPTIONS]");
            Console.WriteLine();
            Console.WriteLine("Options:");

            options.WriteOptionDescriptions(Console.Out);

            return -1;
        }

        #endregion
    }
}
