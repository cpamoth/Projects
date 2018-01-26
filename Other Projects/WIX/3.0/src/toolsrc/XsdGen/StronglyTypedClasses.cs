//-------------------------------------------------------------------------------------------------
// <copyright file="StronglyTypedClasses.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
// Generates the types for a given schema document.
// </summary>
//-------------------------------------------------------------------------------------------------

namespace Microsoft.Tools.WindowsInstallerXml.Tools
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;

    /// <summary>
    /// Type containing static Generate method, which fills in a compile unit from a 
    /// given schema.
    /// </summary>
    internal class StronglyTypedClasses
    {
        private static string outputXmlComment = "Processes this element and all child elements into an XmlTextWriter.";
        private static Hashtable simpleTypeNamesToClrTypeNames;
        private static Hashtable typeNamesToEnumDeclarations;

        /// <summary>
        /// Private constructor for static class.
        /// </summary>
        private StronglyTypedClasses()
        {
        }

        /// <summary>
        /// Generates strongly typed serialization classes for the given schema document
        /// under the given namespace and generates a code compile unit.
        /// </summary>
        /// <param name="xmlSchema">Schema document to generate classes for.</param>
        /// <param name="generateNamespace">Namespace to be used for the generated code.</param>
        /// <param name="commonNamespace">Namespace in which to find common classes and interfaces, 
        /// like ISchemaElement.</param>
        /// <returns>A fully populated CodeCompileUnit, which can be serialized in the language of choice.</returns>
        public static CodeCompileUnit Generate(XmlSchema xmlSchema, string generateNamespace, string commonNamespace)
        {
            if (xmlSchema == null)
            {
                throw new ArgumentNullException("xmlSchema");
            }
            if (generateNamespace == null)
            {
                throw new ArgumentNullException("generateNamespace");
            }

            simpleTypeNamesToClrTypeNames = new Hashtable();
            typeNamesToEnumDeclarations = new Hashtable();

            CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
            CodeNamespace codeNamespace = new CodeNamespace(generateNamespace);
            codeCompileUnit.Namespaces.Add(codeNamespace);
            codeNamespace.Imports.Add(new CodeNamespaceImport("System"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Collections"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Globalization"));
            codeNamespace.Imports.Add(new CodeNamespaceImport("System.Xml"));
            if (commonNamespace != null)
            {
                codeNamespace.Imports.Add(new CodeNamespaceImport(commonNamespace));
            }

            // NOTE: This hash table serves double duty so be sure to have the XSD
            //       type name mapped to the CLR type name *and* the CLR type name
            //       mapped to the same CLR type name.  Look at long and bool for
            //       examples below (and before you ask, no I don't know why DateTime
            //       just works).
            simpleTypeNamesToClrTypeNames.Add("dateTime", "DateTime");
            simpleTypeNamesToClrTypeNames.Add("integer", "int");
            simpleTypeNamesToClrTypeNames.Add("int", "int");
            simpleTypeNamesToClrTypeNames.Add("NMTOKEN", "string");
            simpleTypeNamesToClrTypeNames.Add("string", "string");
            simpleTypeNamesToClrTypeNames.Add("nonNegativeInteger", "long");
            simpleTypeNamesToClrTypeNames.Add("long", "long");
            simpleTypeNamesToClrTypeNames.Add("boolean", "bool");
            simpleTypeNamesToClrTypeNames.Add("bool", "bool");

            xmlSchema.Compile(null);

            foreach (XmlSchemaObject schemaObject in xmlSchema.SchemaTypes.Values)
            {
                XmlSchemaSimpleType schemaSimpleType = schemaObject as XmlSchemaSimpleType;
                if (schemaSimpleType != null)
                {
                    ProcessSimpleType(schemaSimpleType, codeNamespace);
                }
            }

            foreach (XmlSchemaObject schemaObject in xmlSchema.SchemaTypes.Values)
            {
                XmlSchemaComplexType schemaComplexType = schemaObject as XmlSchemaComplexType;
                if (schemaComplexType != null)
                {
                    ProcessComplexType(schemaComplexType, codeNamespace);
                }
            }

            foreach (XmlSchemaObject schemaObject in xmlSchema.Elements.Values)
            {
                XmlSchemaElement schemaElement = schemaObject as XmlSchemaElement;
                if (schemaElement != null)
                {
                    ProcessElement(schemaElement, codeNamespace);
                }
            }

            return codeCompileUnit;
        }

        /// <summary>
        /// Processes an XmlSchemaElement into corresponding types.
        /// </summary>
        /// <param name="schemaElement">XmlSchemaElement to be processed.</param>
        /// <param name="codeNamespace">CodeNamespace to be used when outputting code.</param>
        private static void ProcessElement(XmlSchemaElement schemaElement, CodeNamespace codeNamespace)
        {
            string elementType = schemaElement.SchemaTypeName.Name;
            string elementNamespace = schemaElement.QualifiedName.Namespace;
            string elementDocumentation = GetDocumentation(schemaElement.Annotation);

            if ((elementType == null || elementType.Length == 0) && schemaElement.SchemaType != null)
            {
                ProcessComplexType(schemaElement.Name, elementNamespace, (XmlSchemaComplexType)schemaElement.SchemaType, elementDocumentation, codeNamespace);
            }
            else
            {
                if (elementType == null || elementType.Length == 0)
                {
                    elementType = "string";
                }

                CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(schemaElement.Name);
                typeDeclaration.Attributes = MemberAttributes.Public;
                typeDeclaration.IsClass = true;

                if (elementDocumentation != null)
                {
                    GenerateSummaryComment(typeDeclaration.Comments, elementDocumentation);
                }

                CodeMemberMethod outputXmlMethod = new CodeMemberMethod();
                outputXmlMethod.Attributes = MemberAttributes.Public;
                outputXmlMethod.ImplementationTypes.Add("ISchemaElement");
                outputXmlMethod.Name = "OutputXml";
                outputXmlMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlTextWriter", "writer"));
                outputXmlMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteStartElement", new CodeSnippetExpression(String.Concat("\"", schemaElement.Name, "\"")), new CodeSnippetExpression(String.Concat("\"", elementNamespace, "\""))));
                GenerateSummaryComment(outputXmlMethod.Comments, outputXmlComment);

                if (simpleTypeNamesToClrTypeNames.ContainsKey(elementType))
                {
                    CodeMemberField parentField = new CodeMemberField("ISchemaElement", "parentElement");
                    typeDeclaration.Members.Add(parentField);

                    CodeMemberProperty parentProperty = new CodeMemberProperty();
                    parentProperty.Attributes = MemberAttributes.Public;
                    parentProperty.ImplementationTypes.Add("ISchemaElement");
                    parentProperty.Name = "ParentElement";
                    parentProperty.Type = new CodeTypeReference("ISchemaElement");
                    parentProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "parentElement")));
                    parentProperty.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "parentElement"), new CodeVariableReferenceExpression("value")));
                    typeDeclaration.Members.Add(parentProperty);

                    CodeMemberMethod setAttributeMethod = new CodeMemberMethod();
                    setAttributeMethod.Attributes = MemberAttributes.Public;
                    setAttributeMethod.ImplementationTypes.Add("ISetAttributes");
                    setAttributeMethod.Name = "SetAttribute";
                    setAttributeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "name"));
                    setAttributeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "value"));
                    setAttributeMethod.PrivateImplementationType = new CodeTypeReference("ISetAttributes");

                    GenerateFieldAndProperty("Content", (string)simpleTypeNamesToClrTypeNames[elementType], typeDeclaration, outputXmlMethod, setAttributeMethod, null, elementDocumentation, true, false);

                    typeDeclaration.Members.Add(setAttributeMethod);
                    typeDeclaration.BaseTypes.Add(new CodeTypeReference("ISetAttributes"));
                }
                else
                {
                    typeDeclaration.BaseTypes.Add(elementType);
                    outputXmlMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), "OutputXml", new CodeVariableReferenceExpression("writer")));
                    outputXmlMethod.Attributes |= MemberAttributes.Override;
                }

                outputXmlMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteEndElement"));

                typeDeclaration.BaseTypes.Add(new CodeTypeReference("ISchemaElement"));
                typeDeclaration.Members.Add(outputXmlMethod);
                codeNamespace.Types.Add(typeDeclaration);
            }
        }

        /// <summary>
        /// Processes an XmlSchemaComplexType into corresponding types.
        /// </summary>
        /// <param name="complexType">XmlSchemaComplexType to be processed.</param>
        /// <param name="codeNamespace">CodeNamespace to be used when outputting code.</param>
        private static void ProcessComplexType(XmlSchemaComplexType complexType, CodeNamespace codeNamespace)
        {
            CodeMemberMethod outputXmlMethod = new CodeMemberMethod();
            outputXmlMethod.Attributes = MemberAttributes.Public;
            outputXmlMethod.ImplementationTypes.Add("ISchemaElement");
            outputXmlMethod.Name = "OutputXml";
            outputXmlMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlTextWriter", "writer"));
            GenerateSummaryComment(outputXmlMethod.Comments, outputXmlComment);

            CodeMemberMethod setAttributeMethod = new CodeMemberMethod();
            setAttributeMethod.Attributes = MemberAttributes.Public;
            setAttributeMethod.ImplementationTypes.Add("ISetAttributes");
            setAttributeMethod.Name = "SetAttribute";
            setAttributeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "name"));
            setAttributeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "value"));
            setAttributeMethod.PrivateImplementationType = new CodeTypeReference("ISetAttributes");

            string documentation = GetDocumentation(complexType.Annotation);

            ProcessSimpleContent(complexType.Name, (XmlSchemaSimpleContentExtension)complexType.ContentModel.Content, documentation, codeNamespace, outputXmlMethod, setAttributeMethod, true);
        }

        /// <summary>
        /// Processes an XmlSchemaComplexType into corresponding types.
        /// </summary>
        /// <param name="typeName">Name to use for the type being output.</param>
        /// <param name="elementNamespace">Namespace of the xml element.</param>
        /// <param name="complexType">XmlSchemaComplexType to be processed.</param>
        /// <param name="documentation">Documentation for the element.</param>
        /// <param name="codeNamespace">CodeNamespace to be used when outputting code.</param>
        private static void ProcessComplexType(string typeName, string elementNamespace, XmlSchemaComplexType complexType, string documentation, CodeNamespace codeNamespace)
        {
            CodeMemberMethod outputXmlMethod = new CodeMemberMethod();
            outputXmlMethod.Attributes = MemberAttributes.Public;
            outputXmlMethod.ImplementationTypes.Add("ISchemaElement");
            outputXmlMethod.Name = "OutputXml";
            outputXmlMethod.Parameters.Add(new CodeParameterDeclarationExpression("XmlTextWriter", "writer"));
            outputXmlMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteStartElement", new CodeSnippetExpression(String.Concat("\"", typeName, "\"")), new CodeSnippetExpression(String.Concat("\"", elementNamespace, "\""))));
            GenerateSummaryComment(outputXmlMethod.Comments, outputXmlComment);

            CodeMemberMethod setAttributeMethod = new CodeMemberMethod();
            setAttributeMethod.Attributes = MemberAttributes.Public;
            setAttributeMethod.ImplementationTypes.Add("ISetAttributes");
            setAttributeMethod.Name = "SetAttribute";
            setAttributeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "name"));
            setAttributeMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "value"));
            setAttributeMethod.PrivateImplementationType = new CodeTypeReference("ISetAttributes");

            if (complexType.ContentModel == null)
            {
                CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(typeName);
                typeDeclaration.Attributes = MemberAttributes.Public;
                typeDeclaration.IsClass = true;
                CodeIterationStatement childEnumStatement = null;

                if (documentation != null)
                {
                    GenerateSummaryComment(typeDeclaration.Comments, documentation);
                }

                if (complexType.Particle != null)
                {
                    CodeMemberField childrenField = new CodeMemberField("ElementCollection", "children");
                    typeDeclaration.Members.Add(childrenField);

                    CodeMemberProperty childrenProperty = new CodeMemberProperty();
                    childrenProperty.Attributes = MemberAttributes.Public;
                    childrenProperty.ImplementationTypes.Add("IParentElement");
                    childrenProperty.Name = "Children";
                    childrenProperty.Type = new CodeTypeReference("IEnumerable");
                    childrenProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "children")));
                    typeDeclaration.Members.Add(childrenProperty);

                    CodeMemberProperty filterChildrenProperty = new CodeMemberProperty();
                    filterChildrenProperty.Attributes = MemberAttributes.Public;
                    filterChildrenProperty.ImplementationTypes.Add("IParentElement");
                    filterChildrenProperty.Name = "Item";
                    filterChildrenProperty.Parameters.Add(new CodeParameterDeclarationExpression(typeof(Type), "childType"));
                    filterChildrenProperty.Type = new CodeTypeReference("IEnumerable");
                    filterChildrenProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "children"), "Filter", new CodeVariableReferenceExpression("childType"))));
                    typeDeclaration.Members.Add(filterChildrenProperty);

                    CodeMemberMethod addChildMethod = new CodeMemberMethod();
                    addChildMethod.Attributes = MemberAttributes.Public;
                    addChildMethod.ImplementationTypes.Add("IParentElement");
                    addChildMethod.Name = "AddChild";
                    addChildMethod.Parameters.Add(new CodeParameterDeclarationExpression("ISchemaElement", "child"));
                    CodeExpressionStatement addChildStatement = new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "children"), "AddElement", new CodeVariableReferenceExpression("child")));
                    addChildMethod.Statements.Add(addChildStatement);
                    CodeAssignStatement setParentStatement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("child"), "ParentElement"), new CodeThisReferenceExpression());
                    addChildMethod.Statements.Add(setParentStatement);
                    typeDeclaration.Members.Add(addChildMethod);

                    CodeMemberMethod removeChildMethod = new CodeMemberMethod();
                    removeChildMethod.Attributes = MemberAttributes.Public;
                    removeChildMethod.ImplementationTypes.Add("IParentElement");
                    removeChildMethod.Name = "RemoveChild";
                    removeChildMethod.Parameters.Add(new CodeParameterDeclarationExpression("ISchemaElement", "child"));
                    CodeExpressionStatement removeChildStatement = new CodeExpressionStatement(new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "children"), "RemoveElement", new CodeVariableReferenceExpression("child")));
                    removeChildMethod.Statements.Add(removeChildStatement);
                    CodeAssignStatement nullParentStatement = new CodeAssignStatement(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("child"), "ParentElement"), new CodePrimitiveExpression(null));
                    removeChildMethod.Statements.Add(nullParentStatement);
                    typeDeclaration.Members.Add(removeChildMethod);

                    CodeMemberMethod createChildMethod = new CodeMemberMethod();
                    createChildMethod.Attributes = MemberAttributes.Public;
                    createChildMethod.ImplementationTypes.Add("ICreateChildren");
                    createChildMethod.Name = "CreateChild";
                    createChildMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "childName"));
                    createChildMethod.PrivateImplementationType = new CodeTypeReference("ICreateChildren");
                    createChildMethod.ReturnType = new CodeTypeReference("ISchemaElement");
                    createChildMethod.Statements.Add(new CodeVariableDeclarationStatement("ISchemaElement", "childValue", new CodePrimitiveExpression(null)));

                    CodeConstructor typeConstructor = new CodeConstructor();
                    typeConstructor.Attributes = MemberAttributes.Public;

                    CodeVariableReferenceExpression collectionVariable = null;

                    XmlSchemaChoice schemaChoice = complexType.Particle as XmlSchemaChoice;
                    if (schemaChoice != null)
                    {
                        collectionVariable = ProcessSchemaGroup(schemaChoice, typeConstructor, createChildMethod);
                    }
                    else
                    {
                        XmlSchemaSequence schemaSequence = complexType.Particle as XmlSchemaSequence;
                        if (schemaSequence != null)
                        {
                            collectionVariable = ProcessSchemaGroup(schemaSequence, typeConstructor, createChildMethod);
                        }
                    }

                    typeConstructor.Statements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "children"), collectionVariable));
                    typeDeclaration.Members.Add(typeConstructor);

                    CodeConditionStatement childNameNotFound = new CodeConditionStatement();
                    childNameNotFound.Condition = new CodeBinaryOperatorExpression(new CodePrimitiveExpression(null), CodeBinaryOperatorType.ValueEquality, new CodeVariableReferenceExpression("childValue"));
                    childNameNotFound.TrueStatements.Add(new CodeThrowExceptionStatement(new CodeObjectCreateExpression("InvalidOperationException", new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("String"), "Concat", new CodeVariableReferenceExpression("childName"), new CodeSnippetExpression("\" is not a valid child name.\"")))));
                    createChildMethod.Statements.Add(childNameNotFound);

                    createChildMethod.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("childValue")));
                    typeDeclaration.Members.Add(createChildMethod);

                    childEnumStatement = new CodeIterationStatement();
                    childEnumStatement.InitStatement = new CodeVariableDeclarationStatement("IEnumerator", "enumerator", new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "children"), "GetEnumerator"));
                    childEnumStatement.TestExpression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("enumerator"), "MoveNext");
                    childEnumStatement.Statements.Add(new CodeVariableDeclarationStatement("ISchemaElement", "childElement", new CodeCastExpression("ISchemaElement", new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("enumerator"), "Current"))));
                    childEnumStatement.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("childElement"), "OutputXml", new CodeVariableReferenceExpression("writer")));
                    childEnumStatement.IncrementStatement = new CodeExpressionStatement(new CodeSnippetExpression(""));

                    typeDeclaration.BaseTypes.Add(new CodeTypeReference("IParentElement"));
                    typeDeclaration.BaseTypes.Add(new CodeTypeReference("ICreateChildren"));
                }

                // TODO: Handle xs:anyAttribute here.
                foreach (XmlSchemaAttribute schemaAttribute in complexType.Attributes)
                {
                    ProcessAttribute(schemaAttribute, typeDeclaration, outputXmlMethod, setAttributeMethod);
                }

                if (childEnumStatement != null)
                {
                    outputXmlMethod.Statements.Add(childEnumStatement);
                }

                typeDeclaration.BaseTypes.Add(new CodeTypeReference("ISchemaElement"));
                typeDeclaration.BaseTypes.Add(new CodeTypeReference("ISetAttributes"));

                CodeMemberField parentField = new CodeMemberField("ISchemaElement", "parentElement");
                typeDeclaration.Members.Add(parentField);

                CodeMemberProperty parentProperty = new CodeMemberProperty();
                parentProperty.Attributes = MemberAttributes.Public;
                parentProperty.ImplementationTypes.Add("ISchemaElement");
                parentProperty.Name = "ParentElement";
                parentProperty.Type = new CodeTypeReference("ISchemaElement");
                parentProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "parentElement")));
                parentProperty.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "parentElement"), new CodeVariableReferenceExpression("value")));
                typeDeclaration.Members.Add(parentProperty);

                typeDeclaration.Members.Add(outputXmlMethod);
                typeDeclaration.Members.Add(setAttributeMethod);
                codeNamespace.Types.Add(typeDeclaration);
            }
            else
            {
                ProcessSimpleContent(typeName, (XmlSchemaSimpleContentExtension)complexType.ContentModel.Content, documentation, codeNamespace, outputXmlMethod, setAttributeMethod, false);
            }

            outputXmlMethod.Statements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteEndElement"));
        }

        /// <summary>
        /// Processes an XmlSchemaGroupBase element.
        /// </summary>
        /// <param name="schemaGroup">Element group to process.</param>
        /// <param name="constructor">Constructor to which statements should be added.</param>
        /// <param name="createChildMethod">Method used for creating children on read-in.</param>
        /// <returns>A reference to the local variable containing the collection.</returns>
        private static CodeVariableReferenceExpression ProcessSchemaGroup(XmlSchemaGroupBase schemaGroup, CodeConstructor constructor, CodeMemberMethod createChildMethod)
        {
            return ProcessSchemaGroup(schemaGroup, constructor, createChildMethod, 0);
        }

        /// <summary>
        /// Processes an XmlSchemaGroupBase element.
        /// </summary>
        /// <param name="schemaGroup">Element group to process.</param>
        /// <param name="constructor">Constructor to which statements should be added.</param>
        /// <param name="createChildMethod">Method used for creating children on read-in.</param>
        /// <param name="depth">Depth to which this collection is nested.</param>
        /// <returns>A reference to the local variable containing the collection.</returns>
        private static CodeVariableReferenceExpression ProcessSchemaGroup(XmlSchemaGroupBase schemaGroup, CodeConstructor constructor, CodeMemberMethod createChildMethod, int depth)
        {
            string collectionName = String.Format("childCollection{0}", depth);
            CodeVariableReferenceExpression collectionVariableReference = new CodeVariableReferenceExpression(collectionName);
            CodeVariableDeclarationStatement collectionStatement = new CodeVariableDeclarationStatement("ElementCollection", collectionName);
            if (schemaGroup is XmlSchemaChoice)
            {
                collectionStatement.InitExpression = new CodeObjectCreateExpression("ElementCollection", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("ElementCollection.CollectionType"), "Choice"));
            }
            else
            {
                collectionStatement.InitExpression = new CodeObjectCreateExpression("ElementCollection", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("ElementCollection.CollectionType"), "Sequence"));
            }
            constructor.Statements.Add(collectionStatement);

            foreach (XmlSchemaObject obj in schemaGroup.Items)
            {
                XmlSchemaElement schemaElement = obj as XmlSchemaElement;
                if (schemaElement != null)
                {
                    if (schemaGroup is XmlSchemaChoice)
                    {
                        CodeMethodInvokeExpression addItemInvoke = new CodeMethodInvokeExpression(collectionVariableReference, "AddItem", new CodeObjectCreateExpression("ElementCollection.ChoiceItem", new CodeTypeOfExpression(schemaElement.RefName.Name)));
                        constructor.Statements.Add(addItemInvoke);
                    }
                    else
                    {
                        CodeMethodInvokeExpression addItemInvoke = new CodeMethodInvokeExpression(collectionVariableReference, "AddItem", new CodeObjectCreateExpression("ElementCollection.SequenceItem", new CodeTypeOfExpression(schemaElement.RefName.Name)));
                        constructor.Statements.Add(addItemInvoke);
                    }

                    CodeConditionStatement createChildIf = new CodeConditionStatement();
                    createChildIf.Condition = new CodeBinaryOperatorExpression(new CodeSnippetExpression(String.Concat("\"", schemaElement.RefName.Name, "\"")), CodeBinaryOperatorType.ValueEquality, new CodeVariableReferenceExpression("childName"));
                    createChildIf.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("childValue"), new CodeObjectCreateExpression(schemaElement.RefName.Name)));
                    createChildMethod.Statements.Add(createChildIf);

                    continue;
                }

                XmlSchemaAny schemaAny = obj as XmlSchemaAny;
                if (schemaAny != null)
                {
                    if (schemaGroup is XmlSchemaChoice)
                    {
                        CodeMethodInvokeExpression addItemInvoke = new CodeMethodInvokeExpression(collectionVariableReference, "AddItem", new CodeObjectCreateExpression("ElementCollection.ChoiceItem", new CodeTypeOfExpression("ISchemaElement")));
                        constructor.Statements.Add(addItemInvoke);
                    }
                    else
                    {
                        CodeMethodInvokeExpression addItemInvoke = new CodeMethodInvokeExpression(collectionVariableReference, "AddItem", new CodeObjectCreateExpression("ElementCollection.SequenceItem", new CodeTypeOfExpression("ISchemaElement"), new CodeSnippetExpression("0"), new CodeSnippetExpression("-1")));
                        constructor.Statements.Add(addItemInvoke);
                    }

                    continue;
                }

                XmlSchemaGroupBase schemaGroupBase = obj as XmlSchemaGroupBase;
                if (schemaGroupBase != null)
                {
                    CodeVariableReferenceExpression nestedCollectionReference = ProcessSchemaGroup(schemaGroupBase, constructor, createChildMethod, depth + 1);
                    CodeMethodInvokeExpression addCollectionInvoke = new CodeMethodInvokeExpression(collectionVariableReference, "AddCollection", nestedCollectionReference);
                    constructor.Statements.Add(addCollectionInvoke);

                    continue;
                }
            }

            return collectionVariableReference;
        }

        /// <summary>
        /// Processes an XmlSchemaSimpleContentExtension into corresponding types.
        /// </summary>
        /// <param name="typeName">Name of the type being generated.</param>
        /// <param name="simpleContent">XmlSchemaSimpleContentExtension being processed.</param>
        /// <param name="documentation">Documentation for the simple content.</param>
        /// <param name="codeNamespace">CodeNamespace to be used when outputting code.</param>
        /// <param name="outputXmlMethod">Method to use when outputting Xml.</param>
        /// <param name="setAttributeMethod">Method to use when setting an attribute.</param>
        /// <param name="abstractClass">If true, generate an abstract class.</param>
        private static void ProcessSimpleContent(string typeName, XmlSchemaSimpleContentExtension simpleContent, string documentation, CodeNamespace codeNamespace, CodeMemberMethod outputXmlMethod, CodeMemberMethod setAttributeMethod, bool abstractClass)
        {
            CodeTypeDeclaration typeDeclaration = new CodeTypeDeclaration(typeName);
            typeDeclaration.Attributes = MemberAttributes.Public;
            typeDeclaration.IsClass = true;

            if (documentation != null)
            {
                GenerateSummaryComment(typeDeclaration.Comments, documentation);
            }

            if (abstractClass)
            {
                typeDeclaration.TypeAttributes = System.Reflection.TypeAttributes.Abstract | System.Reflection.TypeAttributes.Public;
            }

            // TODO: Handle xs:anyAttribute here.
            foreach (XmlSchemaAttribute schemaAttribute in simpleContent.Attributes)
            {
                ProcessAttribute(schemaAttribute, typeDeclaration, outputXmlMethod, setAttributeMethod);
            }

            // This needs to come last, so that the generation code generates the inner content after the attributes.
            string contentDocumentation = GetDocumentation(simpleContent.Annotation);
            GenerateFieldAndProperty("Content", (string)simpleTypeNamesToClrTypeNames[simpleContent.BaseTypeName.Name], typeDeclaration, outputXmlMethod, setAttributeMethod, null, contentDocumentation, true, false);

            typeDeclaration.BaseTypes.Add(new CodeTypeReference("ISchemaElement"));
            typeDeclaration.BaseTypes.Add(new CodeTypeReference("ISetAttributes"));

            CodeMemberField parentField = new CodeMemberField("ISchemaElement", "parentElement");
            typeDeclaration.Members.Add(parentField);

            CodeMemberProperty parentProperty = new CodeMemberProperty();
            parentProperty.Attributes = MemberAttributes.Public;
            parentProperty.ImplementationTypes.Add("ISchemaElement");
            parentProperty.Name = "ParentElement";
            parentProperty.Type = new CodeTypeReference("ISchemaElement");
            parentProperty.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "parentElement")));
            parentProperty.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), "parentElement"), new CodeVariableReferenceExpression("value")));
            typeDeclaration.Members.Add(parentProperty);

            typeDeclaration.Members.Add(outputXmlMethod);
            typeDeclaration.Members.Add(setAttributeMethod);
            codeNamespace.Types.Add(typeDeclaration);
        }

        /// <summary>
        /// Processes an attribute, generating the required field and property. Potentially generates
        /// an enum for an attribute restriction.
        /// </summary>
        /// <param name="attribute">Attribute element being processed.</param>
        /// <param name="typeDeclaration">CodeTypeDeclaration to be used when outputting code.</param>
        /// <param name="outputXmlMethod">Member method for the OutputXml method.</param>
        /// <param name="setAttributeMethod">Member method for the SetAttribute method.</param>
        private static void ProcessAttribute(XmlSchemaAttribute attribute, CodeTypeDeclaration typeDeclaration, CodeMemberMethod outputXmlMethod, CodeMemberMethod setAttributeMethod)
        {
            string attributeName = attribute.Name;
            string rawAttributeType = attribute.SchemaTypeName.Name;
            string attributeType = null;
            EnumDeclaration enumDeclaration = null;
            if (rawAttributeType == null || rawAttributeType.Length == 0)
            {
                XmlSchemaSimpleTypeRestriction simpleTypeRestriction = null;

                bool listType = false;
                XmlSchemaSimpleTypeList simpleTypeList = attribute.SchemaType.Content as XmlSchemaSimpleTypeList;
                if (simpleTypeList != null)
                {
                    attributeType = simpleTypeList.ItemTypeName.Name;

                    if (attributeType == String.Empty)
                    {
                        simpleTypeRestriction = simpleTypeList.ItemType.Content as XmlSchemaSimpleTypeRestriction;
                        listType = true;
                    }
                    else
                    {
                        EnumDeclaration declaration = (EnumDeclaration)typeNamesToEnumDeclarations[attributeType];
                        declaration.Flags = true;
                    }
                }

                if (simpleTypeRestriction == null)
                {
                    simpleTypeRestriction = attribute.SchemaType.Content as XmlSchemaSimpleTypeRestriction;
                }

                if (simpleTypeRestriction != null)
                {
                    attributeType = String.Concat(attributeName, "Type");

                    bool enumRestriction = false;
                    CodeTypeDeclaration enumTypeDeclaration = new CodeTypeDeclaration(attributeType);
                    enumTypeDeclaration.Attributes = MemberAttributes.Public;
                    enumTypeDeclaration.IsEnum = true;
                    enumDeclaration = new EnumDeclaration(attributeType, enumTypeDeclaration);

                    foreach (XmlSchemaFacet facet in simpleTypeRestriction.Facets)
                    {
                        XmlSchemaEnumerationFacet enumFacet = facet as XmlSchemaEnumerationFacet;
                        if (enumFacet != null)
                        {
                            enumRestriction = true;
                            string enumValue = MakeEnumValue(enumFacet.Value);
                            enumDeclaration.AddValue(enumFacet.Value);
                            CodeMemberField memberField = new CodeMemberField(typeof(int), enumValue);
                            enumTypeDeclaration.Members.Add(memberField);
                            
                            string enumItemDocumentation = GetDocumentation(enumFacet.Annotation);
                            if (enumItemDocumentation != null)
                            {
                                GenerateSummaryComment(memberField.Comments, enumItemDocumentation);
                            }
                        }

                        XmlSchemaPatternFacet patternFacet = facet as XmlSchemaPatternFacet;
                        if (patternFacet != null)
                        {
                            attributeType = (string)simpleTypeNamesToClrTypeNames[simpleTypeRestriction.BaseTypeName.Name];
                        }
                    }

                    if (enumRestriction)
                    {
                        typeDeclaration.Members.Add(enumTypeDeclaration);
                    }
                    else
                    {
                        enumDeclaration = null;
                    }
                }

                if (listType && enumDeclaration != null)
                {
                    enumDeclaration.Flags = true;
                }
            }
            else
            {
                attributeType = (string)simpleTypeNamesToClrTypeNames[rawAttributeType];
            }

            string documentation = GetDocumentation(attribute.Annotation);

            // TODO: Handle required fields.
            GenerateFieldAndProperty(attributeName, attributeType, typeDeclaration, outputXmlMethod, setAttributeMethod, enumDeclaration, documentation, false, false);
        }

        /// <summary>
        /// Gets the first sentence of a documentation element and returns it as a string.
        /// </summary>
        /// <param name="annotation">The annotation in which to look for a documentation element.</param>
        /// <returns>The string representing the first sentence, or null if none found.</returns>
        private static string GetDocumentation(XmlSchemaAnnotation annotation)
        {
            string documentation = null;
            
            if (annotation != null && annotation.Items != null)
            {
                foreach (XmlSchemaObject obj in annotation.Items)
                {
                    XmlSchemaDocumentation schemaDocumentation = obj as XmlSchemaDocumentation;
                    if (schemaDocumentation != null)
                    {
                        if (schemaDocumentation.Markup.Length > 0)
                        {
                            XmlText text = schemaDocumentation.Markup[0] as XmlText;
                            if (text != null)
                            {
                                documentation = text.Value;
                            }
                        }
                        break;
                    }
                }
            }

            if (documentation != null)
            {
                documentation = documentation.Trim();
            }
            return documentation;
        }

        /// <summary>
        /// Makes a valid enum value out of the passed in value. May remove spaces, add 'Item' to the
        /// start if it begins with an integer, or strip out punctuation.
        /// </summary>
        /// <param name="enumValue">Enum value to be processed.</param>
        /// <returns>Enum value with invalid characters removed.</returns>
        private static string MakeEnumValue(string enumValue)
        {
            if (Char.IsDigit(enumValue[0]))
            {
                enumValue = String.Concat("Item", enumValue);
            }

            StringBuilder newValue = new StringBuilder();
            for (int i = 0; i < enumValue.Length; ++i)
            {
                if (!Char.IsPunctuation(enumValue[i]) && !Char.IsSymbol(enumValue[i]) && !Char.IsWhiteSpace(enumValue[i]))
                {
                    newValue.Append(enumValue[i]);
                }
            }

            return newValue.ToString();
        }

        /// <summary>
        /// Generates the private field and public property for a piece of data.
        /// </summary>
        /// <param name="propertyName">Name of the property being generated.</param>
        /// <param name="typeName">Name of the type for the property.</param>
        /// <param name="typeDeclaration">Type declaration into which the field and property should be placed.</param>
        /// <param name="outputXmlMethod">Member method for the OutputXml method.</param>
        /// <param name="setAttributeMethod">Member method for the SetAttribute method.</param>
        /// <param name="enumDeclaration">EnumDeclaration, which is null unless called from a locally defined enum attribute.</param>
        /// <param name="documentation">Comment string to be placed on the property.</param>
        /// <param name="nestedContent">If true, the field will be placed in nested content when outputting to XML.</param>
        /// <param name="requiredField">If true, the generated serialization code will throw if the field is not set.</param>
        private static void GenerateFieldAndProperty(string propertyName, string typeName, CodeTypeDeclaration typeDeclaration, CodeMemberMethod outputXmlMethod, CodeMemberMethod setAttributeMethod, EnumDeclaration enumDeclaration, string documentation, bool nestedContent, bool requiredField)
        {
            string fieldName = String.Concat(propertyName.Substring(0, 1).ToLower(), propertyName.Substring(1), "Field");
            string fieldNameSet = String.Concat(fieldName, "Set");
            Type type = GetClrTypeByXmlName(typeName);
            CodeMemberField fieldMember;
            if (type == null)
            {
                fieldMember = new CodeMemberField(typeName, fieldName);
            }
            else
            {
                fieldMember = new CodeMemberField(type, fieldName);
            }
            fieldMember.Attributes = MemberAttributes.Private;
            typeDeclaration.Members.Add(fieldMember);
            typeDeclaration.Members.Add(new CodeMemberField(typeof(bool), fieldNameSet));

            CodeMemberProperty propertyMember = new CodeMemberProperty();
            propertyMember.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            if (documentation != null)
            {
                GenerateSummaryComment(propertyMember.Comments, documentation);
            }
            propertyMember.Name = propertyName;
            if (type == null)
            {
                propertyMember.Type = new CodeTypeReference(typeName);
            }
            else
            {
                propertyMember.Type = new CodeTypeReference(type);
            }

            CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement();
            returnStatement.Expression = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
            propertyMember.GetStatements.Add(returnStatement);

            CodeAssignStatement assignmentStatement = new CodeAssignStatement();
            propertyMember.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldNameSet), new CodePrimitiveExpression(true)));
            assignmentStatement.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName);
            assignmentStatement.Right = new CodePropertySetValueReferenceExpression();
            propertyMember.SetStatements.Add(assignmentStatement);

            CodeConditionStatement fieldSetStatement = new CodeConditionStatement();
            fieldSetStatement.Condition = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldNameSet);

            CodeAssignStatement fieldSetAttrStatement = new CodeAssignStatement();
            fieldSetAttrStatement.Left = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldNameSet);
            fieldSetAttrStatement.Right = new CodePrimitiveExpression(true);

            CodeConditionStatement attributeNameMatchStatement = new CodeConditionStatement();
            attributeNameMatchStatement.Condition = new CodeBinaryOperatorExpression(new CodeSnippetExpression(String.Concat("\"", propertyName, "\"")), CodeBinaryOperatorType.IdentityEquality, new CodeVariableReferenceExpression("name"));

            string clrTypeName = (string)simpleTypeNamesToClrTypeNames[typeName];
            switch (clrTypeName)
            {
                case "string":
                    if (nestedContent)
                    {
                        fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteString", new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
                        attributeNameMatchStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeVariableReferenceExpression("value")));
                    }
                    else
                    {
                        fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteAttributeString", new CodeSnippetExpression(String.Concat("\"", propertyName, "\"")), new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
                        attributeNameMatchStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeVariableReferenceExpression("value")));
                    }
                    break;
                case "bool":
                    if (nestedContent)
                    {
                        fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteString", new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), "ToString", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"))));
                    }
                    else
                    {
                        fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteAttributeString", new CodeSnippetExpression(String.Concat("\"", propertyName, "\"")), new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), "ToString", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"))));
                    }
                    attributeNameMatchStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Convert"), "ToBoolean", new CodeVariableReferenceExpression("value"), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"))));
                    break;
                case "int":
                case "long":
                    if (nestedContent)
                    {
                        fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteString", new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), "ToString", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"))));
                    }
                    else
                    {
                        fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteAttributeString", new CodeSnippetExpression(String.Concat("\"", propertyName, "\"")), new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), "ToString", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"))));
                    }
                    attributeNameMatchStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Convert"), "ToInt32", new CodeVariableReferenceExpression("value"), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"))));
                    break;
                default:
                    if (typeName == "DateTime")
                    {
                        if (nestedContent)
                        {
                            fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteString", new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), "ToString", new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"))));
                        }
                        else
                        {
                            fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteAttributeString", new CodeSnippetExpression(String.Concat("\"", propertyName, "\"")), new CodeMethodInvokeExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), "ToString", new CodePrimitiveExpression("yyyy-MM-ddTHH:mm:ss"), new CodePropertyReferenceExpression(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"), "DateTimeFormat"))));
                        }
                        attributeNameMatchStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeMethodInvokeExpression(new CodeTypeReferenceExpression("Convert"), "ToDateTime", new CodeVariableReferenceExpression("value"), new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("CultureInfo"), "InvariantCulture"))));
                        break;
                    }

                    if (enumDeclaration == null)
                    {
                        GenerateOutputForEnum(fieldSetStatement, attributeNameMatchStatement, (EnumDeclaration)typeNamesToEnumDeclarations[typeName], fieldName, propertyName);
                    }
                    else
                    {
                        GenerateOutputForEnum(fieldSetStatement, attributeNameMatchStatement, enumDeclaration, fieldName, propertyName);
                    }
                    break;
            }

            attributeNameMatchStatement.TrueStatements.Add(fieldSetAttrStatement);

            // TODO: Add throw to falseStatements if required field not set.
            outputXmlMethod.Statements.Add(fieldSetStatement);
            setAttributeMethod.Statements.Add(attributeNameMatchStatement);

            typeDeclaration.Members.Add(propertyMember);
        }

        /// <summary>
        /// Generates output for an enum type. Will generate a switch statement for normal enums, and if statements
        /// for a flags enum.
        /// </summary>
        /// <param name="fieldSetStatement">If statement to add statements to.</param>
        /// <param name="attributeNameMatchStatement">If statement to add statements to.</param>
        /// <param name="enumDeclaration">Enum declaration for this field. Could be locally defined enum or global.</param>
        /// <param name="fieldName">Name of the private field.</param>
        /// <param name="propertyName">Name of the property (and XML attribute).</param>
        private static void GenerateOutputForEnum(CodeConditionStatement fieldSetStatement, CodeConditionStatement attributeNameMatchStatement, EnumDeclaration enumDeclaration, string fieldName, string propertyName)
        {
            if (enumDeclaration.Flags)
            {
                CodeVariableDeclarationStatement outputValueVariable = new CodeVariableDeclarationStatement(typeof(string), "outputValue", new CodeSnippetExpression("\"\""));
                fieldSetStatement.TrueStatements.Add(outputValueVariable);

                CodeVariableDeclarationStatement splitValueVariable = new CodeVariableDeclarationStatement(typeof(string[]), "splitValue", new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("value"), "Split", new CodeSnippetExpression("' '")));
                attributeNameMatchStatement.TrueStatements.Add(splitValueVariable);

                CodeIterationStatement valueEnumStatement = new CodeIterationStatement();
                valueEnumStatement.InitStatement = new CodeVariableDeclarationStatement("IEnumerator", "enumerator", new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("splitValue"), "GetEnumerator"));
                valueEnumStatement.TestExpression = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("enumerator"), "MoveNext");
                valueEnumStatement.IncrementStatement = new CodeExpressionStatement(new CodeSnippetExpression(""));
                valueEnumStatement.Statements.Add(new CodeVariableDeclarationStatement(typeof(string), "currentValue", new CodeCastExpression(typeof(string), new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("enumerator"), "Current"))));
                attributeNameMatchStatement.TrueStatements.Add(valueEnumStatement);
                
                foreach (string key in enumDeclaration.Values)
                {
                    CodeConditionStatement enumIfStatement = new CodeConditionStatement();
                    enumIfStatement.Condition = new CodeBinaryOperatorExpression(new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), CodeBinaryOperatorType.BitwiseAnd, new CodePropertyReferenceExpression(new CodeSnippetExpression(enumDeclaration.Name), MakeEnumValue(key))), CodeBinaryOperatorType.IdentityInequality, new CodeSnippetExpression("0"));
                    CodeConditionStatement lengthIfStatement = new CodeConditionStatement();
                    lengthIfStatement.Condition = new CodeBinaryOperatorExpression(new CodePropertyReferenceExpression(new CodeVariableReferenceExpression("outputValue"), "Length"), CodeBinaryOperatorType.IdentityInequality, new CodeSnippetExpression("0"));
                    lengthIfStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("outputValue"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("outputValue"), CodeBinaryOperatorType.Add, new CodeSnippetExpression("\" \""))));
                    enumIfStatement.TrueStatements.Add(lengthIfStatement);
                    enumIfStatement.TrueStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("outputValue"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("outputValue"), CodeBinaryOperatorType.Add, new CodeSnippetExpression(String.Concat("\"", key, "\"")))));
                    fieldSetStatement.TrueStatements.Add(enumIfStatement);

                    CodeConditionStatement valueIfStatement = new CodeConditionStatement();
                    valueIfStatement.Condition = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("currentValue"), CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression(String.Concat("\"", key, "\"")));
                    valueIfStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), CodeBinaryOperatorType.BitwiseAnd, new CodePropertyReferenceExpression(new CodeSnippetExpression(enumDeclaration.Name), MakeEnumValue(key)))));
                    valueEnumStatement.Statements.Add(valueIfStatement);
                }

                fieldSetStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteAttributeString", new CodeSnippetExpression(String.Concat("\"", propertyName, "\"")), new CodeSnippetExpression(String.Concat("outputValue"))));
            }
            else
            {
                foreach (string key in enumDeclaration.Values)
                {
                    CodeConditionStatement enumOutStatement = new CodeConditionStatement();
                    enumOutStatement.Condition = new CodeBinaryOperatorExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), CodeBinaryOperatorType.ValueEquality, new CodePropertyReferenceExpression(new CodeSnippetExpression(enumDeclaration.Name), MakeEnumValue(key)));
                    enumOutStatement.TrueStatements.Add(new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("writer"), "WriteAttributeString", new CodeSnippetExpression(String.Concat("\"", propertyName, "\"")), new CodeSnippetExpression(String.Concat("\"", key, "\""))));
                    fieldSetStatement.TrueStatements.Add(enumOutStatement);

                    CodeConditionStatement enumInStatement = new CodeConditionStatement();
                    enumInStatement.Condition = new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("value"), CodeBinaryOperatorType.ValueEquality, new CodeSnippetExpression(String.Concat("\"", key, "\"")));
                    enumInStatement.TrueStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodePropertyReferenceExpression(new CodeSnippetExpression(enumDeclaration.Name), MakeEnumValue(key))));
                    attributeNameMatchStatement.TrueStatements.Add(enumInStatement);
                }
            }
        }

        /// <summary>
        /// Generates a summary comment.
        /// </summary>
        /// <param name="comments">Comments collection to add the comments to.</param>
        /// <param name="content">Content of the comment.</param>
        private static void GenerateSummaryComment(CodeCommentStatementCollection comments, string content)
        {
            using (StringWriter sw = new StringWriter())
            {
                XmlTextWriter writer = null;

                // create the comment as xml to ensure proper escaping of special xml characters
                try
                {
                    writer = new XmlTextWriter(sw);
                    writer.Indentation = 0;

                    writer.WriteStartElement("summary");
                    writer.WriteString(Environment.NewLine);

                    string nextComment;
                    int newlineIndex = content.IndexOf(Environment.NewLine);
                    int offset = 0;
                    while (newlineIndex != -1)
                    {
                        nextComment = content.Substring(offset, newlineIndex - offset).Trim();
                        writer.WriteString(nextComment);
                        writer.WriteString(Environment.NewLine);
                        offset = newlineIndex + Environment.NewLine.Length;
                        newlineIndex = content.IndexOf(Environment.NewLine, offset);
                    }
                    nextComment = content.Substring(offset).Trim();
                    writer.WriteString(nextComment);
                    writer.WriteString(Environment.NewLine);

                    writer.WriteEndElement();
                }
                finally
                {
                    if (null != writer)
                    {
                        writer.Close();
                    }
                }

                // create the comment statements (one per line of xml)
                using (StringReader sr = new StringReader(sw.ToString()))
                {
                    string line;

                    while (null != (line = sr.ReadLine()))
                    {
                        comments.Add(new CodeCommentStatement(line, true));
                    }
                }
            }
        }

        /// <summary>
        /// Gets the CLR type for simple XML type.
        /// </summary>
        /// <param name="typeName">Plain text name of type.</param>
        /// <returns>Type corresponding to parameter.</returns>
        private static Type GetClrTypeByXmlName(string typeName)
        {
            switch (typeName)
            {
                case "bool":
                    return typeof(bool);
                case "int":
                    return typeof(int);
                case "long":
                    return typeof(long);
                case "string":
                    return typeof(string);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Processes an XmlSchemaSimpleType into corresponding types.
        /// </summary>
        /// <param name="simpleType">XmlSchemaSimpleType to be processed.</param>
        /// <param name="codeNamespace">CodeNamespace to be used when outputting code.</param>
        private static void ProcessSimpleType(XmlSchemaSimpleType simpleType, CodeNamespace codeNamespace)
        {
            ProcessSimpleType(simpleType.Name, simpleType, codeNamespace);
        }

        /// <summary>
        /// Processes an XmlSchemaSimpleType into corresponding code.
        /// </summary>
        /// <param name="typeName">Type name to use.</param>
        /// <param name="simpleType">XmlSchemaSimpleType to be processed.</param>
        /// <param name="codeNamespace">CodeNamespace to be used when outputting code.</param>
        private static void ProcessSimpleType(string typeName, XmlSchemaSimpleType simpleType, CodeNamespace codeNamespace)
        {
            XmlSchemaSimpleTypeRestriction simpleTypeRestriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
            if (simpleTypeRestriction != null)
            {
                CodeTypeDeclaration typeDeclaration = null;
                EnumDeclaration enumDeclaration = null;
                foreach (XmlSchemaFacet facet in simpleTypeRestriction.Facets)
                {
                    XmlSchemaEnumerationFacet enumFacet = facet as XmlSchemaEnumerationFacet;
                    if (enumFacet != null)
                    {
                        if (enumDeclaration == null)
                        {
                            typeDeclaration = new CodeTypeDeclaration(typeName);
                            typeDeclaration.Attributes = MemberAttributes.Public;
                            typeDeclaration.IsEnum = true;
                            enumDeclaration = new EnumDeclaration(typeName, typeDeclaration);
                            typeNamesToEnumDeclarations.Add(typeName, enumDeclaration);
                            codeNamespace.Types.Add(typeDeclaration);

                            simpleTypeNamesToClrTypeNames.Add(typeName, typeName);
                            
                            string typeDocumentation = GetDocumentation(simpleType.Annotation);
                            if (typeDocumentation != null)
                            {
                                GenerateSummaryComment(typeDeclaration.Comments, typeDocumentation);
                            }
                        }
                        
                        string enumValue = MakeEnumValue(enumFacet.Value);
                        enumDeclaration.AddValue(enumFacet.Value);
                        CodeMemberField memberField = new CodeMemberField(typeof(int), enumValue);
                        typeDeclaration.Members.Add(memberField);
                        string documentation = GetDocumentation(enumFacet.Annotation);
                        if (documentation != null)
                        {
                            GenerateSummaryComment(memberField.Comments, documentation);
                        }
                    }
                }

                if (typeDeclaration == null)
                {
                    string baseTypeName = simpleTypeRestriction.BaseTypeName.Name;
                    if (baseTypeName == "nonNegativeInteger" || baseTypeName == "integer")
                    {
                        simpleTypeNamesToClrTypeNames.Add(typeName, "int");
                    }
                    else if (baseTypeName == "string" || baseTypeName == "NMTOKEN")
                    {
                        simpleTypeNamesToClrTypeNames.Add(typeName, "string");
                    }
                }
            }
        }

        /// <summary>
        /// Class representing an enum declaration.
        /// </summary>
        internal class EnumDeclaration
        {
            private string enumTypeName;
            private CodeTypeDeclaration declaration;
            private bool flags;
            private StringCollection enumValues;

            /// <summary>
            /// Creates a new enum declaration with the given name.
            /// </summary>
            /// <param name="enumTypeName">Name of the type for the enum.</param>
            /// <param name="declaration">Type of the enum.</param>
            public EnumDeclaration(string enumTypeName, CodeTypeDeclaration declaration)
            {
                this.enumTypeName = enumTypeName;
                this.declaration = declaration;
                this.enumValues = new StringCollection();
            }

            /// <summary>
            /// Gets the enumeration values.
            /// </summary>
            /// <value>The enumeration values.</value>
            public ICollection Values
            {
                get { return this.enumValues; }
            }

            /// <summary>
            /// Gets the enumeration name.
            /// </summary>
            /// <value>The enumeration name.</value>
            public string Name
            {
                get { return this.enumTypeName; }
            }

            /// <summary>
            /// Gets or sets the enumeration flags.
            /// </summary>
            /// <value>The enumeration flags.</value>
            public bool Flags
            {
                get
                {
                    return this.flags;
                }

                set
                {
                    if (value && !this.flags)
                    {
                        this.flags = value;
                        this.declaration.CustomAttributes.Add(new CodeAttributeDeclaration("Flags"));
                        CodeMemberField noneField = new CodeMemberField(typeof(int), "None");
                        noneField.InitExpression = new CodeSnippetExpression("0");
                        this.declaration.Members.Insert(0, noneField);

                        int enumValue = 0;
                        foreach (CodeMemberField enumField in this.declaration.Members)
                        {
                            enumField.InitExpression = new CodeSnippetExpression(enumValue.ToString());
                            if (enumValue == 0)
                            {
                                enumValue = 1;
                            }
                            else
                            {
                                enumValue *= 2;
                            }
                        }
                    }
                    else if (!value)
                    {
                        this.flags = value;
                        this.declaration.CustomAttributes.Clear();
                    }
                }
            }

            /// <summary>
            /// Add a value to the enumeration.
            /// </summary>
            /// <param name="enumValue">The value to add.</param>
            public void AddValue(string enumValue)
            {
                this.enumValues.Add(enumValue);
            }
        }
    }
}