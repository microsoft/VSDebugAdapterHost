// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Utilities;
using static System.FormattableString;

namespace SampleDebugAdapter
{
    internal class SampleSource
    {
        public SampleSource(string name, string path, int sourceReference)
        {
            this.Name = name;
            this.Path = path;
            this.SourceReference = sourceReference;
        }

        public string Name { get; }
        public string Path { get; }
        public int SourceReference { get; }

        internal Source GetProtocolSource()
        {
            return new Source(
                name: this.Name,
                path: this.Path,
                sourceReference: this.SourceReference.ZeroToNull());
        }

        internal static SampleSource Create(StringBuilder output, SampleSourceManager sampleScriptManager, string name, string path, int sourceReference)
        {
            if (sourceReference > 0 && !String.IsNullOrWhiteSpace(path))
            {
                output.AppendLine(Invariant($"Source {name} should not have both a path and a source reference!"));
            }
            return new SampleSource(name, path, sourceReference);
        }
    }
}
