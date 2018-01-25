// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System.Text;

namespace SampleDebugAdapter.Directives
{
    internal interface IDirective
    {
        string Name { get; }
        bool Execute(string[] args, StringBuilder output);
        object ParseArgs(string[] args);
    }
}
