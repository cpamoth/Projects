using System;
using System.Collections;
using System.Text;

namespace Microsoft.Tools.WindowsInstallerXml
{
    public class PatchTransform : IMessageHandler
    {
        private string baseline;
        private Output transform;

        public string Baseline
        {
            get { return this.baseline; }
        }

        public Output Transform
        {
            get { return this.transform; }
        }

        public PatchTransform(Output transform, string baseline)
        {
            this.transform = transform;
            this.baseline = baseline;
            this.Validate();
        }

        /// <summary>
        /// Event for messages.
        /// </summary>
        public event MessageEventHandler Message;

        /// <summary>
        /// Validates that the differences in the transform are valid for patch transforms.
        /// </summary>
        public void Validate()
        {
            Table componentTable = this.transform.Tables["Component"];
            Hashtable deletedComponent = new Hashtable();
            Hashtable componentKeyPath = new Hashtable();

            if (null != componentTable)
            {
                // Index Component table and check for keypath modifications
                foreach (Row row in componentTable.Rows)
                {
                    string keypath = (null == row.Fields[5].Data) ? String.Empty : row.Fields[5].Data.ToString();
                    componentKeyPath.Add(row.Fields[0].Data.ToString(), keypath);
                    if (RowOperation.Delete == row.Operation)
                    {
                        deletedComponent.Add(row.Fields[0].Data.ToString(), row);
                    }
                    else if (RowOperation.Modify == row.Operation)
                    {
                        // If the keypath is modified its an error
                        if (row.Fields[5].Modified)
                        {
                            this.OnMessage(WixErrors.InvalidKeypathChange(row.SourceLineNumbers, row.Fields[0].Data.ToString()));
                        }
                    }
                }

                // Verify changes in the file table
                Table fileTable = this.transform.Tables["File"];
                if (null != fileTable)
                {
                    Hashtable componentWithChangedKeyPath = new Hashtable();
                    Hashtable componentWithNonKeyPathChanged = new Hashtable();
                    foreach (Row row in fileTable.Rows)
                    {
                        if (RowOperation.None != row.Operation)
                        {
                            string fileId = row.Fields[0].Data.ToString();
                            string componentId = row.Fields[1].Data.ToString();

                            if (0 != String.Compare((string)componentKeyPath[componentId], fileId, false))
                            {
                                if (!componentWithNonKeyPathChanged.ContainsKey(componentId))
                                {
                                    componentWithNonKeyPathChanged.Add(componentId, fileId);
                                }
                            }
                            else
                            {
                                if (!componentWithChangedKeyPath.ContainsKey(componentId))
                                {
                                    componentWithChangedKeyPath.Add(componentId, fileId);
                                }
                            }

                            if (RowOperation.Delete == row.Operation)
                            {
                                // If the file is removed from a component that is not deleted.
                                if (!deletedComponent.ContainsKey(componentId))
                                {
                                    this.OnMessage(WixWarnings.InvalidRemoveFile(row.SourceLineNumbers, fileId, componentId));
                                }
                            }
                        }
                    }

                    foreach (DictionaryEntry componentFile in componentWithNonKeyPathChanged)
                    {
                        // Make sure all changes to non keypaths also had a change in the keypath.
                        if (!componentWithChangedKeyPath.ContainsKey(componentFile.Key))
                        {
                            this.OnMessage(WixWarnings.RemovalOfNonKeyPathFile((string)componentFile.Value, (string)componentFile.Key, (string)componentKeyPath[componentFile.Key]));
                        }
                    }
                }

                if (0 < deletedComponent.Count)
                {
                    // Index feature table
                    Table featureTable = this.transform.Tables["Feature"];
                    Hashtable deletedFeature = new Hashtable();
                    if (null != featureTable)
                    {
                        foreach (Row row in featureTable.Rows)
                        {
                            if (RowOperation.Delete == row.Operation)
                            {
                                deletedFeature.Add(row.Fields[0].Data.ToString(), row);
                            }
                        }
                    }

                    // Index FeatureComponents table.
                    Table featureComponentsTable = this.transform.Tables["FeatureComponents"];
                    Hashtable featureComponents = new Hashtable();

                    if (null != featureComponentsTable)
                    {
                        foreach (Row row in featureComponentsTable.Rows)
                        {
                            ArrayList features;
                            string componentId = row.Fields[1].Data.ToString();

                            if (featureComponents.Contains(componentId))
                            {
                                features = (ArrayList)featureComponents[componentId];
                            }
                            else
                            {
                                features = new ArrayList();
                                featureComponents.Add(row.Fields[1].Data.ToString(), features);
                            }
                            features.Add(row.Fields[0].Data.ToString());
                        }
                    }

                    // Check to make sure if a component was deleted, the feature was too.
                    foreach (DictionaryEntry entry in deletedComponent)
                    {
                        if (featureComponents.Contains(entry.Key.ToString()))
                        {
                            ArrayList features = (ArrayList)featureComponents[entry.Key.ToString()];
                            foreach (string featureId in features)
                            {
                                if (deletedFeature.Contains(featureId))
                                {
                                    //The feature has also been deleted.
                                    continue;
                                }
                                else
                                {
                                    this.OnMessage(WixWarnings.InvalidRemoveComponent(((Row)entry.Value).SourceLineNumbers, entry.Key.ToString(), featureId));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to the message delegate if there is one.
        /// </summary>
        /// <param name="mea">Message event arguments.</param>
        public void OnMessage(MessageEventArgs mea)
        {
            WixErrorEventArgs errorEventArgs = mea as WixErrorEventArgs;

            if (null != this.Message)
            {
                this.Message(this, mea);
            }
            else if (null != errorEventArgs)
            {
                throw new WixException(errorEventArgs);
            }
        }
    }
}