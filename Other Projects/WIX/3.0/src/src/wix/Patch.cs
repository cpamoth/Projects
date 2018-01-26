using System;
using System.Collections;
using System.Globalization;
using System.Text;
using Microsoft.Tools.WindowsInstallerXml.Msi;
using Microsoft.Tools.WindowsInstallerXml.Msi.Interop;

namespace Microsoft.Tools.WindowsInstallerXml
{
    public class Patch
    {
        private Output patch;
        private TableDefinitionCollection tableDefinitions;

        public Output PatchOutput
        {
            get { return this.patch; }
        }

        public Patch()
        {
            this.tableDefinitions = Installer.GetTableDefinitions();
        }

        public void Load(string patchPath)
        {
            this.patch = Output.Load(patchPath, false, false);
        }

        /// <summary>
        /// Include transforms in a patch.
        /// </summary>
        /// <param name="transforms">List of transforms to attach.</param>
        public void AttachTransforms(ArrayList transforms)
        {
            int emptyTransform = 0;

            if (transforms == null || transforms.Count == 0)
            {
                throw new WixException(WixErrors.PatchWithoutTransforms());
            }

            // Get the patch id from the WixPatchId table.
            string patchId = null;
            Table wixPatchIdTable = this.patch.Tables["WixPatchId"];
            if (null != wixPatchIdTable && 0 < wixPatchIdTable.Rows.Count)
            {
                Row patchIdRow = wixPatchIdTable.Rows[0];
                if (null != patchIdRow)
                {
                    patchId = patchIdRow[0].ToString();
                }
            }

            // enumerate patch.Media to map diskId to Media row
            Hashtable mediaRows = new Hashtable();
            Table patchMediaTable = patch.Tables["Media"];
            if (patchMediaTable != null)
            {
                foreach (MediaRow row in patchMediaTable.Rows)
                {
                    int media = row.DiskId;
                    mediaRows[media] = row;
                }
            }

            // enumerate patch.WixPatchBaseline to map baseline to diskId
            Hashtable baselineMedia = new Hashtable();
            Table patchBaselineTable = patch.Tables["WixPatchBaseline"];
            if (patchBaselineTable != null)
            {
                foreach (Row row in patchBaselineTable.Rows)
                {
                    string baseline = (string)row[0];
                    int media = (int)row[1];
                    if (baselineMedia.Contains(baseline))
                    {
                        throw new InvalidOperationException(String.Format("PatchBaseline '{0}' authored into multiple Media.", baseline));
                    }
                    baselineMedia[baseline] = media;
                }
            }

            // enumerate transforms
            ArrayList productCodes = new ArrayList();
            ArrayList transformNames = new ArrayList();
            int transformCount = 0;
            foreach (PatchTransform mainTransform in transforms)
            {
                string baseline = null;
                int media = -1;
                
                if (baselineMedia.Contains(mainTransform.Baseline))
                {
                    int newMedia = (int)baselineMedia[mainTransform.Baseline];
                    if (media != -1 && media != newMedia)
                    {
                        throw new InvalidOperationException(String.Format("Transform authored into multiple Media '{0}' and '{1}'.", media, newMedia));
                    }
                    baseline = mainTransform.Baseline;
                    media = newMedia;
                }

                if (media == -1)
                {
                    // transform's baseline not attached to any Media
                    continue;
                }

                Table patchRefTable = patch.Tables["WixPatchRef"];
                if (patchRefTable != null && patchRefTable.Rows.Count > 0)
                {
                    if (!this.ReduceTransform(mainTransform.Transform, patchRefTable))
                    {
                        // transform has none of the content authored into this patch
                        emptyTransform++;
                        continue;
                    }
                }

                // ensure consistent File.Sequence within each Media
                MediaRow mediaRow = (MediaRow)mediaRows[media];
                // TODO: should this be authored rather than inferring it from DiskId?
                mediaRow.LastSequence = mediaRow.DiskId;

                // ignore media table from transform.
                mainTransform.Transform.Tables.Remove("Media");
                mainTransform.Transform.Tables.Remove("WixMedia");
                mainTransform.Transform.Tables.Remove("MsiDigitalSignature");

                string productCode = null;
                Output pairedTransform = this.BuildPairedTransform(patchId, mainTransform.Transform, mediaRow, ref productCode);
                productCodes.Add(productCode);

                // attach these transforms to the patch object
                // TODO: is this an acceptable way to auto-generate transform stream names?
                string transformName = baseline + "." + (++transformCount).ToString();
                patch.SubStorages.Add(new SubStorage(transformName, mainTransform.Transform));
                patch.SubStorages.Add(new SubStorage("#" + transformName, pairedTransform));
                transformNames.Add(":" + transformName);
                transformNames.Add(":#" + transformName);
            }

            if (emptyTransform == transforms.Count)
            {
                throw new WixException(WixErrors.PatchWithoutValidTransforms());
            }

            // populate MSP summary information
            Table patchSummaryInfo = patch.EnsureTable(this.tableDefinitions["_SummaryInformation"]);
            // remove any existing data for these fields
            for (int i = patchSummaryInfo.Rows.Count - 1; i >= 0; i--)
            {
                Row row = patchSummaryInfo.Rows[i];
                switch ((SummaryInformation.Patch)row[0])
                {
                    case SummaryInformation.Patch.ProductCodes:
                    case SummaryInformation.Patch.TransformNames:
                    case SummaryInformation.Patch.PatchCode:
                    case SummaryInformation.Patch.InstallerRequirement:
                        patchSummaryInfo.Rows.RemoveAt(i);
                        break;
                }
            }

            // Semicolon delimited list of the product codes that can accept the patch.
            Row templateRow = patchSummaryInfo.CreateRow(null);
            templateRow[0] = (int)SummaryInformation.Patch.ProductCodes;
            templateRow[1] = String.Join(";", (string[])productCodes.ToArray(typeof(string)));

            // Semicolon delimited list of transform substorage names in the order they are applied.
            Row savedbyRow = patchSummaryInfo.CreateRow(null);
            savedbyRow[0] = (int)SummaryInformation.Patch.TransformNames;
            savedbyRow[1] = String.Join(";", (string[])transformNames.ToArray(typeof(string)));

            // GUID patch code for the patch. 
            Row revisionRow = patchSummaryInfo.CreateRow(null);
            revisionRow[0] = (int)SummaryInformation.Patch.PatchCode;
            revisionRow[1] = patchId;

            // Indicates the minimum Windows Installer version that is required to install the patch. 
            Row wordsRow = patchSummaryInfo.CreateRow(null);
            wordsRow[0] = (int)SummaryInformation.Patch.InstallerRequirement;
            wordsRow[1] = ((int)SummaryInformation.InstallerRequirement.Version31).ToString();

            Row security = patchSummaryInfo.CreateRow(null);
            security[0] = 19; //PID_SECURITY
            security[1] = "4";

            Table msiPatchMetadataTable = patch.Tables["MsiPatchMetadata"];
            if (null != msiPatchMetadataTable)
            {
                Hashtable metadataTable = new Hashtable();
                foreach (Row row in msiPatchMetadataTable.Rows)
                {
                    metadataTable.Add(row.Fields[1].Data.ToString(), row.Fields[2].Data.ToString());
                }

                if (metadataTable.Contains("DisplayName"))
                {
                    string comment = String.Concat("This patch contains the logic and data required to install ", metadataTable["DisplayName"]);

                    Row title = patchSummaryInfo.CreateRow(null);
                    title[0] = 2; //PID_TITLE
                    title[1] = metadataTable["DisplayName"];

                    Row comments = patchSummaryInfo.CreateRow(null);
                    comments[0] = 6; //PID_COMMENTS
                    comments[1] = comment;
                }

                if (metadataTable.Contains("CodePage"))
                {
                    Row codePage = patchSummaryInfo.CreateRow(null);
                    codePage[0] = 1; //PID_CODEPAGE
                    codePage[1] = metadataTable["CodePage"];
                }

                if (metadataTable.Contains("Description"))
                {
                    Row subject = patchSummaryInfo.CreateRow(null);
                    subject[0] = 3; //PID_SUBJECT
                    subject[1] = metadataTable["Description"];
                }

                if (metadataTable.Contains("ManufacturerName"))
                {
                    Row author = patchSummaryInfo.CreateRow(null);
                    author[0] = 4; //PID_AUTHOR
                    author[1] = metadataTable["ManufacturerName"];
                }
            }
        }

        /// <summary>
        /// Reduce the transform according to the patch references.
        /// </summary>
        /// <param name="transform">transform generated by torch.</param>
        /// <param name="patchRefTable">Table contains patch family filter.</param>
        /// <returns>true if the transform is not empty</returns>
        public bool ReduceTransform(Output transform, Table patchRefTable)
        {
            // identify sections to keep
            Hashtable oldSections = new Hashtable();
            Hashtable newSections = new Hashtable();
            Hashtable tableKeyRows = new Hashtable();
            ArrayList sequenceList = new ArrayList();
            Hashtable customActionTable = new Hashtable();

            foreach (Row patchRefRow in patchRefTable.Rows)
            {
                string tableName = (string)patchRefRow[0];
                string key = (string)patchRefRow[1];

                Table table = transform.Tables[tableName];
                if (table == null)
                {
                    // table not found
                    continue;
                }

                // index this table
                if (!tableKeyRows.Contains(tableName))
                {
                    Hashtable newKeyRows = new Hashtable();
                    foreach (Row newRow in table.Rows)
                    {
                        newKeyRows[newRow.GetPrimaryKey('/')] = newRow;
                    }
                    tableKeyRows[tableName] = newKeyRows;
                }
                Hashtable keyRows = (Hashtable)tableKeyRows[tableName];

                Row row = (Row)keyRows[key];
                if (row == null)
                {
                    // row not found
                    continue;
                }

                // Differ.sectionDelimiter
                string[] sections = row.SectionId.Split('/');
                oldSections[sections[0]] = row;
                newSections[sections[1]] = row;
            }

            // throw away sections not referenced
            int keptRows = 0;
            ArrayList tablesToDelete = new ArrayList();
            foreach (Table table in transform.Tables)
            {
                if (table.Name == "_SummaryInformation")
                {
                    continue;
                }

                if (table.Name == "AdminExcuteSequence"
                    || table.Name == "AdminUISequence"
                    || table.Name == "AdvtExecuteSequence"
                    || table.Name == "InstallUISequence"
                    || table.Name == "InstallExecuteSequence")
                {
                    sequenceList.Add(table);
                    continue;
                }

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    Row row = table.Rows[i];

                    if (table.Name == "CustomAction")
                    {
                        customActionTable.Add(row[0], row);
                    }

                    if (null == row.SectionId)
                    {
                        table.Rows.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        string[] sections = row.SectionId.Split('/');
                        // ignore the row without section id.
                        if (sections[0] == string.Empty && sections[1] == string.Empty)
                        {
                            table.Rows.RemoveAt(i);
                            i--;
                        }
                        else if (IsInPatchFamily(sections[0], sections[1], oldSections, newSections))                        
                        {
                            keptRows++;
                        }
                        else
                        {
                            table.Rows.RemoveAt(i);
                            i--;
                        }
                    }
                }

                if (table.Rows.Count == 0)
                {
                    tablesToDelete.Add(table.Name);
                }
            }

            keptRows += ReduceTransformSequenceTable(sequenceList, oldSections, newSections, customActionTable, tablesToDelete);

            // delete separately to avoid messing up enumeration
            foreach (string tableName in tablesToDelete)
            {
                transform.Tables.Remove(tableName);
            }

            return keptRows > 0;
        }

        /// <summary>
        /// Check if the section is in a PatchFamily.
        /// </summary>
        /// <param name="oldSection">Section id in target wixout</param>
        /// <param name="newSection">Section id in upgrade wixout</param>
        /// <param name="oldSections">Hashtable contains section id should be kept in the baseline wixout.</param>
        /// <param name="newSections">Hashtable contains section id should be kept in the upgrade wixout.</param>
        /// <returns>true if section in patch family</returns>
        private bool IsInPatchFamily(string oldSection, string newSection, Hashtable oldSections, Hashtable newSections)
        {
            bool result = false;

            if ((oldSection == string.Empty && newSections.Contains(newSection)) || (newSection == string.Empty && oldSections.Contains(oldSection)))
            {
                result = true;
            }
            else if (oldSection != string.Empty && newSection != string.Empty && (oldSections.Contains(oldSection) || newSections.Contains(newSection)))
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Reduce the transform sequence tables.
        /// </summary>
        /// <param name="sequenceList">ArrayList of tables to be reduced</param>
        /// <param name="oldSections">Hashtable contains section id should be kept in the baseline wixout.</param>
        /// <param name="newSections">Hashtable contains section id should be kept in the target wixout.</param>
        /// <param name="customAction">Hashtable contains all the rows in the CustomAction table.</param>
        /// <param name="tablesToDelete">ArrayList contains tables that should be deleted.</param>
        /// <returns>Number of rows left</returns>
        private int ReduceTransformSequenceTable(ArrayList sequenceList, Hashtable oldSections, Hashtable newSections, Hashtable customAction, ArrayList tablesToDelete)
        {
            int keptRows = 0;

            foreach (Table currentTable in sequenceList)
            {
                for (int i = 0; i < currentTable.Rows.Count; i++)
                {
                    Row row = currentTable.Rows[i];
                    string actionName = row.Fields[0].Data.ToString();
                    string[] sections = row.SectionId.Split('/');
                    bool isSectionIdEmpty = (sections[0] == string.Empty && sections[1] == string.Empty);
                    
                    if (row.Operation == RowOperation.None)
                    {
                        // ignore the rows without section id.
                        if (isSectionIdEmpty)
                        {
                            currentTable.Rows.RemoveAt(i);
                            i--;
                        }
                        else if (IsInPatchFamily(sections[0], sections[1], oldSections, newSections))
                        {
                            keptRows++;
                        }
                        else
                        {
                            currentTable.Rows.RemoveAt(i);
                            i--;
                        }
                    }
                    else if (row.Operation == RowOperation.Modify)
                    {
                        bool sequenceChanged = row.Fields[2].Modified;
                        bool conditionChanged = row.Fields[1].Modified;

                        if (sequenceChanged && !conditionChanged)
                        {
                            keptRows++;
                        }
                        else if (!sequenceChanged && conditionChanged)
                        {
                            if (isSectionIdEmpty)
                            {
                                currentTable.Rows.RemoveAt(i);
                                i--;
                            }
                            else if (IsInPatchFamily(sections[0], sections[1], oldSections, newSections))
                            {
                                keptRows++;
                            }
                            else
                            {
                                currentTable.Rows.RemoveAt(i);
                                i--;
                            }
                        }
                        else if (sequenceChanged && conditionChanged )
                        {
                            if (isSectionIdEmpty)
                            {
                                row.Fields[1].Modified = false;
                                keptRows++;
                            }
                            else if (IsInPatchFamily(sections[0], sections[1], oldSections, newSections))
                            {
                                keptRows++;
                            }
                            else
                            {
                                row.Fields[1].Modified = false;
                                keptRows++;
                            }
                        }
                    }
                    else if (row.Operation == RowOperation.Delete)
                    {
                        if (isSectionIdEmpty)
                        {
                            // it is a stardard action which is added by wix, we should keep this action.
                            row.Operation = RowOperation.None;
                            keptRows++;
                        }
                        else if (IsInPatchFamily(sections[0], sections[1], oldSections, newSections))
                        {
                            keptRows++;
                        }
                        else
                        {
                            if (customAction.ContainsKey(actionName))
                            {
                                currentTable.Rows.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                // it is a stardard action, we should keep this action.
                                row.Operation = RowOperation.None;
                                keptRows++;
                            }
                        }
                    }
                    else if (row.Operation == RowOperation.Add)
                    {
                        if (isSectionIdEmpty)
                        {
                            keptRows++;
                        }
                        else if (IsInPatchFamily(sections[0], sections[1], oldSections, newSections))
                        {
                            keptRows++;
                        }
                        else
                        {
                            if (customAction.ContainsKey(actionName))
                            {
                                currentTable.Rows.RemoveAt(i);
                                i--;
                            }
                            else
                            {
                                keptRows++;
                            }
                        }
                    }

                }

                if (currentTable.Rows.Count == 0)
                {
                    tablesToDelete.Add(currentTable.Name);
                }

            }
            return keptRows;

        }

        /// <summary>
        /// Create the #transform for the given main transform.
        /// </summary>
        /// <param name="patchId">patch GUID from patch authoring.</param>
        /// <param name="mainTransform">transform generated by torch.</param>
        /// <param name="mediaRow">media authored into patch.</param>
        /// <param name="productCode">output string to receive ProductCode.</param>
        public Output BuildPairedTransform(string patchId, Output mainTransform, MediaRow mediaRow, ref string productCode)
        {
            Output pairedTransform = new Output(null);
            pairedTransform.Type = OutputType.Transform;
            pairedTransform.Codepage = mainTransform.Codepage;

            // lookup productVersion property to correct summaryInformation
            string newProductVersion = null;
            Table mainPropertyTable = mainTransform.Tables["Property"];
            if (mainPropertyTable != null)
            {
                foreach (Row row in mainPropertyTable.Rows)
                {
                    if ("ProductVersion" == (string)row[0])
                    {
                        newProductVersion = (string)row[1];
                    }
                }
            }

            // TODO: build class for manipulating SummaryInformation table
            Table mainSummaryTable = mainTransform.Tables["_SummaryInformation"];
            // add required properties
            Hashtable mainSummaryRows = new Hashtable();
            foreach (Row mainSummaryRow in mainSummaryTable.Rows)
            {
                mainSummaryRows[mainSummaryRow[0]] = mainSummaryRow;
            }
            if (!mainSummaryRows.Contains((int)SummaryInformation.Transform.ValidationFlags))
            {
                Row mainSummaryRow = mainSummaryTable.CreateRow(null);
                mainSummaryRow[0] = (int)SummaryInformation.Transform.ValidationFlags;
                mainSummaryRow[1] = "0";
            }

            // copy summary information from core transform
            Table pairedSummaryTable = pairedTransform.EnsureTable(this.tableDefinitions["_SummaryInformation"]);
            foreach (Row mainSummaryRow in mainSummaryTable.Rows)
            {
                string value = (string)mainSummaryRow[1];
                switch ((SummaryInformation.Transform)mainSummaryRow[0])
                {
                    case SummaryInformation.Transform.ProductCodes:
                        string[] propertyData = value.Split(';');
                        string oldProductVersion = propertyData[0].Substring(38);
                        string upgradeCode = propertyData[2];
                        productCode = propertyData[0].Substring(0, 38);
                        if (newProductVersion == null)
                        {
                            newProductVersion = oldProductVersion;
                        }

                        // force mainTranform to old;new;upgrade and pairedTransform to new;new;upgrade
                        mainSummaryRow[1] = String.Concat(productCode, oldProductVersion, ';', productCode, newProductVersion, ';', upgradeCode);
                        value = String.Concat(productCode, newProductVersion, ';', productCode, newProductVersion, ';', upgradeCode);
                        break;
                    case SummaryInformation.Transform.ValidationFlags:
                        // TODO: ensure this row exists in mainSummaryTable!!!!
                        // TODO: author these settings in patch XML or set in torch.exe
                        int i = Convert.ToInt32(value);
                        i |= (int)SummaryInformation.TransformFlags.ErrorAddExistingRow;
                        i |= (int)SummaryInformation.TransformFlags.ErrorDeleteMissingRow;
                        i |= (int)SummaryInformation.TransformFlags.ErrorAddExistingTable;
                        i |= (int)SummaryInformation.TransformFlags.ErrorDeleteMissingTable;
                        i |= (int)SummaryInformation.TransformFlags.ErrorUpdateMissingRow;
                        i |= (int)SummaryInformation.TransformFlags.ValidateProduct;
                        mainSummaryRow[1] = value = i.ToString();
                        break;
                }
                Row pairedSummaryRow = pairedSummaryTable.CreateRow(null);
                pairedSummaryRow[0] = mainSummaryRow[0];
                pairedSummaryRow[1] = value;
            }

            if (productCode == null)
            {
                throw new InvalidOperationException("Could not determine ProductCode from transform summary information");
            }

            // copy File table
            Table mainFileTable = mainTransform.Tables["File"];
            Table mainWixFileTable = mainTransform.Tables["WixFile"];
            if (mainFileTable != null)
            {
                FileRowCollection mainFileRows = new FileRowCollection();
                mainFileRows.AddRange(mainFileTable.Rows);

                Table pairedFileTable = pairedTransform.EnsureTable(mainFileTable.Definition);
                foreach (Row mainWixFileRow in mainWixFileTable.Rows)
                {
                    FileRow mainFileRow = mainFileRows[(string)mainWixFileRow[0]];

                    // set File.Sequence to non null to satisfy transform bind
                    mainFileRow.Sequence = 1;

                    // delete's don't need rows in the paired transform
                    if (mainFileRow.Operation == RowOperation.Delete)
                    {
                        continue;
                    }

                    FileRow pairedFileRow = (FileRow)pairedFileTable.CreateRow(null);
                    pairedFileRow.Operation = RowOperation.Modify;
                    for (int i = 0; i < mainFileRow.Fields.Length; i++)
                    {
                        object value = mainFileRow[i];
                        pairedFileRow[i] = value;
                    }

                    // override authored media for patch bind
                    // TODO: consider using File/@DiskId for patch media
                    mainFileRow.DiskId = mediaRow.DiskId;
                    mainWixFileRow[5] = mediaRow.DiskId;
                    // suppress any change to File.Sequence to avoid bloat
                    mainFileRow.Fields[7].Modified = false;

                    // force File row to appear in the transform
                    if (RowOperation.Modify == mainFileRow.Operation)
                    {
                        mainFileRow.Operation = RowOperation.Modify;
                        pairedFileRow.Attributes |= MsiInterop.MsidbFileAttributesPatchAdded;
                        pairedFileRow.Fields[6].Modified = true;
                        pairedFileRow.Operation = RowOperation.Modify;
                    }
                    else if (RowOperation.Add == mainFileRow.Operation)
                    {
                        // set msidbFileAttributesPatchAdded
                        pairedFileRow.Attributes |= MsiInterop.MsidbFileAttributesPatchAdded;
                        pairedFileRow.Fields[6].Modified = true;
                        pairedFileRow.Operation = RowOperation.Add;
                    }
                    else
                    {
                        pairedFileRow.Attributes = mainFileRow.Attributes;
                        pairedFileRow.Fields[6].Modified = false;
                    }
                }

            }

            // add Media row to pairedTransform
            Table pairedMediaTable = pairedTransform.EnsureTable(this.tableDefinitions["Media"]);
            Row pairedMediaRow = pairedMediaTable.CreateRow(null);
            pairedMediaRow.Operation = RowOperation.Add;
            for (int i = 0; i < mediaRow.Fields.Length; i++)
            {
                pairedMediaRow[i] = mediaRow[i];
            }

            // add PatchPackage for this Media
            Table pairedPackageTable = pairedTransform.EnsureTable(this.tableDefinitions["PatchPackage"]);
            pairedPackageTable.Operation = TableOperation.Add;
            Row pairedPackageRow = pairedPackageTable.CreateRow(null);
            pairedPackageRow.Operation = RowOperation.Add;
            pairedPackageRow[0] = patchId;
            pairedPackageRow[1] = mediaRow.DiskId;

            // add property to both identify client patches and whether those patches are removable or not
            string patchPropertyId = new Guid(patchId).ToString("N", CultureInfo.InvariantCulture).ToUpper();
            int allowRemoval = 0;
            Table msiPatchMetadataTable = this.patch.Tables["MsiPatchMetadata"];
            if (null != msiPatchMetadataTable)
            {
                foreach (Row msiPatchMetadataRow in msiPatchMetadataTable.Rows)
                {
                    if (string.Empty == (string)msiPatchMetadataRow[0] && "AllowRemoval" == (string)msiPatchMetadataRow[1])
                    {
                        allowRemoval = Convert.ToInt32((string)msiPatchMetadataRow[2]);
                    }
                }
            }
            Table pairedPropertyTable = pairedTransform.EnsureTable(this.tableDefinitions["Property"]);
            pairedPropertyTable.Operation = TableOperation.Add;
            Row pairedPropertyRow = pairedPropertyTable.CreateRow(null);
            pairedPropertyRow.Operation = RowOperation.Add;
            pairedPropertyRow[0] = string.Format(CultureInfo.InvariantCulture, "_{0}.AllowRemoval", patchPropertyId);
            pairedPropertyRow[1] = allowRemoval.ToString();

            return pairedTransform;
        }
    }
}
