// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using System.Reflection;
using System.Globalization;

// Copied from https://github.com/aspnet/EntityFramework/blob/dev/src/Shared/Check.cs
namespace System.Linq.Dynamic.Core.Validation
{
    [DebuggerStepThrough]
    internal static class Check
    {

        [ContractAnnotation("value:null => halt")]
        public static T NotNull<T>([NoEnumeration] T value, [InvokerParameterName] [NotNull] string parameterName) {
            if (ReferenceEquals(value, null)) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        [ContractAnnotation("value:null => halt")]
        public static string NotEmpty(string value, [InvokerParameterName] [NotNull] string parameterName) {
            Exception e = null;
            if (ReferenceEquals(value, null)) {
                e = new ArgumentNullException(parameterName);
            } else if (value.Trim().Length == 0) {
                e = new ArgumentException(ArgumentIsEmpty(parameterName));
            }

            if (e != null) {
                NotEmpty(parameterName, nameof(parameterName));

                throw e;
            }

            return value;
        }

        // CoreStrings.

        static string ArgumentIsEmpty([CanBeNull] string argumentName)
        {
            return string.Format(CultureInfo.CurrentCulture, "The string argument '{0}' cannot be empty.", argumentName);
        }

        public static IList<T> HasNoNulls<T>(IList<T> value, [InvokerParameterName] [NotNull] string parameterName)
            where T : class {
            NotNull(value, parameterName);

            if (value.Any(e => e == null)) {
                NotEmpty(parameterName, nameof(parameterName));

                throw new ArgumentException(parameterName);
            }

            return value;
        }
    }
}