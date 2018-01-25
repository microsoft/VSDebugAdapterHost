// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;

namespace EngineLauncher.Utilities
{
    internal class GuidHelper
    {
        internal static Guid? TryParseGuid(string input)
        {
            Guid value;
            if (String.IsNullOrEmpty(input) || !Guid.TryParse(input, out value))
            {
                return null;
            }

            return value;
        }
    }
}
