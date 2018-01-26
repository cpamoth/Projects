//-------------------------------------------------------------------------------------------------
// <copyright file="Validator.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Runs internal consistency evaluators (ICEs) from cub files against a database.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using Microsoft.Tools.WindowsInstallerXml.Msi;
    using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

    /// <summary>
    /// Runs internal consistency evaluators (ICEs) from cub files against a database.
    /// </summary>
    public sealed class Validator : IMessageHandler
    {
        private StringCollection cubeFiles;
        private bool encounteredError;
        private ValidatorExtension extension;
        private Output output;
        private string[] suppressedICEs;
        private TempFileCollection tempFiles;

        /// <summary>
        /// Instantiate a new Validator.
        /// </summary>
        public Validator()
        {
            this.cubeFiles = new StringCollection();
            this.extension = new ValidatorExtension();
        }

        /// <summary>
        /// Gets or sets a <see cref="ValidatorExtension"/> that directs messages from the validator.
        /// </summary>
        /// <value>A <see cref="ValidatorExtension"/> that directs messages from the validator.</value>
        public ValidatorExtension Extension
        {
            get { return this.extension; }
            set { this.extension = value; }
        }

        /// <summary>
        /// Gets or sets the output used for finding source line information.
        /// </summary>
        /// <value>The output used for finding source line information.</value>
        public Output Output
        {
            // cache Output object until validation for changes in extension
            get { return this.output; }
            set { this.output = value; }
        }

        /// <summary>
        /// Gets or sets the suppressed ICEs.
        /// </summary>
        /// <value>The suppressed ICEs.</value>
        public string[] SuppressedICEs
        {
            get { return this.suppressedICEs; }
            set { this.suppressedICEs = value; }
        }

        /// <summary>
        /// Gets or sets the temporary path for the Binder.  If left null, the binder
        /// will use %TEMP% environment variable.
        /// </summary>
        /// <value>Path to temp files.</value>
        public string TempFilesLocation
        {
            get
            {
                return null == this.tempFiles ? String.Empty : this.tempFiles.BasePath;
            }

            set
            {
                if (null == value)
                {
                    this.tempFiles = new TempFileCollection();
                }
                else
                {
                    this.tempFiles = new TempFileCollection(value);
                }
            }
        }

        /// <summary>
        /// Add a cube file to the validation run.
        /// </summary>
        /// <param name="cubeFile">A cube file.</param>
        public void AddCubeFile(string cubeFile)
        {
            this.cubeFiles.Add(cubeFile);
        }

        /// <summary>
        /// Validate a database.
        /// </summary>
        /// <param name="databaseFile">The database to validate.</param>
        /// <returns>true if validation succeeded; false otherwise.</returns>
        public bool Validate(string databaseFile)
        {
            InstallUIHandler currentUIHandler = null;
            Hashtable indexedSuppressedICEs = new Hashtable();
            int previousUILevel = (int)InstallUILevels.Basic;
            IntPtr previousHwnd = IntPtr.Zero;
            InstallUIHandler previousUIHandler = null;

            if (null == databaseFile)
            {
                throw new ArgumentNullException("databaseFile");
            }

            // initialize the validator extension
            this.extension.DatabaseFile = databaseFile;
            this.extension.Output = this.output;
            this.extension.InitializeValidator();

            // if we don't have the temporary files object yet, get one
            if (null == this.tempFiles)
            {
                this.tempFiles = new TempFileCollection();
            }
            Directory.CreateDirectory(this.TempFilesLocation); // ensure the base path is there

            // index the suppressed ICEs
            if (null != this.suppressedICEs)
            {
                foreach (string suppressedICE in this.suppressedICEs)
                {
                    indexedSuppressedICEs[suppressedICE] = null;
                }
            }

            // copy the database to a temporary location so it can be manipulated
            string tempDatabaseFile = Path.Combine(this.TempFilesLocation, Path.GetFileName(databaseFile));
            File.Copy(databaseFile, tempDatabaseFile);

            // remove the read-only property from the temporary database
            FileAttributes attributes = File.GetAttributes(tempDatabaseFile);
            File.SetAttributes(tempDatabaseFile, attributes & ~FileAttributes.ReadOnly);

            try
            {
                using (Database database = new Database(tempDatabaseFile, OpenDatabase.Direct))
                {
                    bool propertyTableExists = database.TableExists("Property");
                    string productCode = null;

                    // remove the product code from the database before opening a session to prevent opening an installed product
                    if (propertyTableExists)
                    {
                        using (View view = database.OpenExecuteView("SELECT `Value` FROM `Property` WHERE Property = 'ProductCode'"))
                        {
                            Record record = null;

                            try
                            {
                                if (null != (record = view.Fetch()))
                                {
                                    productCode = record.GetString(1);

                                    using (View dropProductCodeView = database.OpenExecuteView("DELETE FROM `Property` WHERE `Property` = 'ProductCode'"))
                                    {
                                    }
                                }
                            }
                            finally
                            {
                                if (null != record)
                                {
                                    record.Close();
                                }
                            }
                        }
                    }

                    // merge in the cube databases
                    foreach (string cubeFile in this.cubeFiles)
                    {
                        try
                        {
                            using (Database cubeDatabase = new Database(cubeFile, OpenDatabase.ReadOnly))
                            {
                                try
                                {
                                    database.Merge(cubeDatabase, "MergeConflicts");
                                }
                                catch
                                {
                                    // ignore merge errors since they are expected in the _Validation table
                                }
                            }
                        }
                        catch (Win32Exception e)
                        {
                            if (0x6E == e.NativeErrorCode) // ERROR_OPEN_FAILED
                            {
                                throw new WixException(WixErrors.CubeFileNotFound(cubeFile));
                            }

                            throw;
                        }
                    }

                    // commit the database before proceeding to ensure the streams don't get confused
                    database.Commit();

                    // the property table may have been added to the database
                    // from a cub database without the proper validation rows
                    if (!propertyTableExists)
                    {
                        using (View view = database.OpenExecuteView("DROP table `Property`"))
                        {
                        }
                    }

                    // get all the action names for ICEs which have not been suppressed
                    ArrayList actions = new ArrayList();
                    using (View view = database.OpenExecuteView("SELECT `Action` FROM `_ICESequence` ORDER BY `Sequence`"))
                    {
                        Record record;

                        while (null != (record = view.Fetch()))
                        {
                            string action = record.GetString(1);

                            if (!indexedSuppressedICEs.Contains(action))
                            {
                                actions.Add(action);
                            }
                            record.Close();
                        }
                    }

                    // disable the internal UI handler and set an external UI handler
                    previousUILevel = Installer.SetInternalUI((int)InstallUILevels.None, ref previousHwnd);
                    currentUIHandler = new InstallUIHandler(this.ValidationUIHandler);
                    previousUIHandler = Installer.SetExternalUI(currentUIHandler, (int)InstallLogModes.Error | (int)InstallLogModes.Warning | (int)InstallLogModes.User, IntPtr.Zero);

                    // create a session for running the ICEs
                    using (Session session = new Session(database))
                    {
                        // add the product code back into the database
                        if (null != productCode)
                        {
                            using (View view = database.OpenExecuteView(String.Format(CultureInfo.InvariantCulture, "INSERT INTO `Property` (`Property`, `Value`) VALUES ('ProductCode', '{0}')", productCode)))
                            {
                            }
                        }

                        foreach (string action in actions)
                        {
                            session.DoAction(action);
                        }
                    }
                }
            }
            catch (Win32Exception e)
            {
                // avoid displaying errors twice since one may have already occurred in the UI handler
                if (!this.encounteredError)
                {
                    this.OnMessage(WixErrors.Win32Exception(e.NativeErrorCode, e.Message));
                }
            }
            finally
            {
                Installer.SetExternalUI(previousUIHandler, 0, IntPtr.Zero);
                Installer.SetInternalUI(previousUILevel, ref previousHwnd);

                // very important - this prevents the external UI delegate from being garbage collected too early
                GC.KeepAlive(currentUIHandler);

                this.cubeFiles.Clear();
                this.extension.FinalizeValidator();
            }

            return !this.encounteredError;
        }

        /// <summary>
        /// Cleans up the temp files used by the Validator.
        /// </summary>
        /// <returns>True if all files were deleted, false otherwise.</returns>
        public bool DeleteTempFiles()
        {
            if (null == this.tempFiles)
            {
                return true; // no work to do
            }
            else
            {
                bool deleted = Common.DeleteTempFiles(this.tempFiles.BasePath, this);

                if (deleted)
                {
                    this.tempFiles = null; // temp files have been deleted, no need to remember this now
                }

                return deleted;
            }
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        public void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

            if (null != errorEventArgs)
            {
                this.encounteredError = true;
            }

            this.extension.OnMessage(mea);
        }

        /// <summary>
        /// The validation external UI handler.
        /// </summary>
        /// <param name="context">Pointer to an application context.
        /// This parameter can be used for error checking.</param>
        /// <param name="messageType">Specifies a combination of one message box style,
        /// one message box icon type, one default button, and one installation message type.</param>
        /// <param name="message">Specifies the message text.</param>
        /// <returns>-1 for an error, 0 if no action was taken, 1 if OK, 3 to abort.</returns>
        private int ValidationUIHandler(IntPtr context, uint messageType, string message)
        {
            this.extension.Log(message);
            return 1;
        }
    }
}
