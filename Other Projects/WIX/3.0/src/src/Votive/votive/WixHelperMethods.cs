//-------------------------------------------------------------------------------------------------
// <copyright file="WixHelperMethods.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Contains the WixHelperMethods class.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.VisualStudio
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Globalization;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// Contains useful helper methods.
    /// </summary>
    internal static class WixHelperMethods
    {
        private const string FailFastMessage = "A catastrophic error has occurred and the process was terminated.";

        /// <summary>
        /// Adds the <see cref="Path.DirectorySeparatorChar"/> character to the end of the path if it doesn't already exist at the end.
        /// </summary>
        /// <param name="path">The string to add the trailing directory separator character to.</param>
        /// <returns>The original string with the specified character at the end.</returns>
        public static string EnsureTrailingDirectoryChar(string path)
        {
            return EnsureTrailingChar(path, Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Adds the specified character to the end of the string if it doesn't already exist at the end.
        /// </summary>
        /// <param name="value">The string to add the trailing character to.</param>
        /// <param name="charToEnsure">The character that will be at the end of the string upon return.</param>
        /// <returns>The original string with the specified character at the end.</returns>
        public static string EnsureTrailingChar(string value, char charToEnsure)
        {
            VerifyStringArgument(value, "value");

            if (value[value.Length - 1] != charToEnsure)
            {
                value += charToEnsure;
            }
            return value;
        }

        /// <summary>
        /// Gets a strongly-typed service from the environment, throwing an exception if the service cannot be retrieved.
        /// </summary>
        /// <typeparam name="TInterface">The interface type to get (i.e. IVsShell).</typeparam>
        /// <typeparam name="TService">The service type to get (i.e. SvsShell).</typeparam>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> to use for retrieving the service.</param>
        /// <returns>An object that implements the interface from the environment.</returns>
        public static TInterface GetService<TInterface, TService>(IServiceProvider serviceProvider)
            where TInterface : class
            where TService : class
        {
            VerifyNonNullArgument(serviceProvider, "serviceProvider");

            TInterface service = serviceProvider.GetService(typeof(TService)) as TInterface;

            if (service == null)
            {
                string message = SafeStringFormat(CultureInfo.CurrentCulture, WixStrings.CannotGetService, typeof(TInterface).Name);
                throw new InvalidOperationException(message);
            }

            return service;
        }

        /// <summary>
        /// Returns a value indicating whether we can recover from the specified exception. If we can't recover,
        /// then it's expected that the caller will immediately call <see cref="Shutdown"/>.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsExceptionUnrecoverable(Exception e)
        {
            return (e is StackOverflowException);
        }

        /// <summary>
        /// Combines two registry paths.
        /// </summary>
        /// <param name="path1">The first path to combine.</param>
        /// <param name="path2">The second path to combine.</param>
        /// <returns>The concatenation of the first path with the second, delimeted with a '\'.</returns>
        public static string RegistryPathCombine(string path1, string path2)
        {
            VerifyStringArgument(path1, "path1");
            VerifyStringArgument(path2, "path2");

            return EnsureTrailingChar(path1, '\\') + path2;
        }

        /// <summary>
        /// Attempts to format the specified string by calling <see cref="System.String.Format(IFormatProvider, string, object[])"/>.
        /// If a <see cref="FormatException"/> is raised, then <paramref name="format"/> is returned.
        /// </summary>
        /// <param name="provider">An <see cref="IFormatProvider"/> that supplies culture-specific formatting information.</param>
        /// <param name="format">A string containing zero or more format items.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the string equivalent of the corresponding instances of object in args.</returns>
        public static string SafeStringFormat(IFormatProvider provider, string format, params object[] args)
        {
            string formattedString = format;

            try
            {
                if (args != null && args.Length > 0)
                {
                    formattedString = String.Format(provider, format, args);
                }
            }
            catch (FormatException)
            {
            }

            return formattedString;
        }

        /// <summary>
        /// Attempts to format the specified string by calling <c>System.PackageUtility.SafeStringFormatInvariant(format, args)</c>.
        /// If a <see cref="FormatException"/> is raised, then <paramref name="format"/> is returned.
        /// </summary>
        /// <param name="format">A string containing zero or more format items.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        /// <returns>A copy of <paramref name="format"/> in which the format items have been replaced by the string equivalent of the corresponding instances of object in args.</returns>
        public static string SafeStringFormatInvariant(string format, params object[] args)
        {
            return SafeStringFormat(CultureInfo.InvariantCulture, format, args);
        }

        /// <summary>
        /// Performs a ship assertion, which raises an assertion dialog. TODO: Generate a call stack and email it to some alias.
        /// </summary>
        /// <param name="condition">The condition to assert.</param>
        /// <param name="message">The message to show in the assertion.</param>
        /// <param name="args">An array of arguments for the formatted message.</param>
        public static void ShipAssert(bool condition, string message, params object[] args)
        {
            if (!condition)
            {
                VerifyStringArgument(message, "message");

                try
                {
                    // get the stack trace (not including this method)
                    StackTrace stack = new StackTrace(1, true);
                    string stackTrace = stack.ToString();

                    // create a StringBuilder to do our string concatenations
                    StringBuilder formattedMessage = new StringBuilder(message.Length + stackTrace.Length);

                    // append the message to the string
                    formattedMessage.Append(SafeStringFormat(CultureInfo.CurrentCulture, message, args));

                    // append the stack trace
                    formattedMessage.Append(Environment.NewLine);
                    formattedMessage.Append(stackTrace);

                    // trace the message and show an assertion dialog
                    TraceFail(formattedMessage.ToString());
                }
                catch (Exception e)
                {
                    if (IsExceptionUnrecoverable(e)) { Shutdown(); }
                    TraceFail("There was an exception while trying to perform a ShipAssert: {0}", e);
                }
            }
        }

        /// <summary>
        /// Shuts down the process by calling <see cref="Environment.FailFast"/>, which will write an event log
        /// entry and create a managed Watson dump.
        /// </summary>
        public static void Shutdown()
        {
            Environment.FailFast(FailFastMessage);
        }

        /// <summary>
        /// Calls <see cref="Trace.Fail(string)"/> with a formatted message.
        /// </summary>
        /// <param name="message">The message to format.</param>
        /// <param name="args">The arguments to use in the format. Can be null or empty.</param>
        [Conditional("TRACE")]
        public static void TraceFail(string message, params object[] args)
        {
            if (args != null && args.Length > 0)
            {
                message = SafeStringFormat(CultureInfo.CurrentCulture, message, args);
            }

            Trace.Fail(message);
        }

        /// <summary>
        /// Verifies that the specified argument is not null and throws an <see cref="ArgumentNullException"/> if it is.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void VerifyNonNullArgument(object argument, string argumentName)
        {
            if (argument == null)
            {
                TraceFail("The argument '{0}' is null.", argumentName);
                throw new ArgumentNullException(argumentName);
            }
        }

        /// <summary>
        /// Verifies that the specified string argument is non-null and non-empty, asserting if it
        /// is not and throwing a new <see cref="ArgumentException"/>.
        /// </summary>
        /// <param name="argument">The argument to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        public static void VerifyStringArgument(string argument, string argumentName)
        {
            if (argument == null || argument.Length == 0 || argument.Trim().Length == 0)
            {
                string message = String.Format(CultureInfo.InvariantCulture, "The string argument '{0}' is null or empty.", argumentName);
                TraceFail("Invalid string argument", message);
                throw new ArgumentException(message, argumentName);
            }
        }
    }
}