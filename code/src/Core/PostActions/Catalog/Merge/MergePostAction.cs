﻿// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Templates.Core.PostActions.Catalog.Merge
{
    public class MergePostAction : PostAction<string>
    {
        public const string Extension = "_postaction.";
        public const string GlobalExtension = "$*_gpostaction.";
        public const string PostactionRegex = @"(\$\S*)?(_postaction|_gpostaction)\.";

        public MergePostAction(string config) : base(config)
        {
        }

        public override void Execute()
        {
            var originalFilePath = Regex.Replace(_config, PostactionRegex, ".");

            var source = File.ReadAllLines(originalFilePath).ToList();
            var merge = File.ReadAllLines(_config).ToList();

            var result = source.Merge(merge);

            File.WriteAllLines(originalFilePath, result);
            File.Delete(_config);
        }
    }
}
