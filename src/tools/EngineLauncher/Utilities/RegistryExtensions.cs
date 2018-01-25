// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Win32;

namespace EngineLauncher.Utilities
{
    internal static class RegistryExtensions
    {
        public static Guid? GetGuidValue(this RegistryKey key, string name)
        {
            string rawValue = key.GetValue(name) as string;

            return GuidHelper.TryParseGuid(rawValue);
        }

        public static int? GetIntValue(this RegistryKey key, string name)
        {
            object rawValue = key.GetValue(name);
            if (rawValue == null || !(rawValue is int))
            {
                return null;
            }

            return (int)rawValue;
        }

        public static bool? GetBoolValue(this RegistryKey key, string name)
        {
            int? rawValue = key.GetIntValue(name);

            // Treat any non-null value other than zero as true
            return rawValue != null && rawValue != 0;
        }
    }
}
