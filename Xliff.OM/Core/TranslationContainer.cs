﻿namespace Localization.Xliff.OM.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using Localization.Xliff.OM.Attributes;
    using Localization.Xliff.OM.Converters;
    using Localization.Xliff.OM.Core.XmlNames;
    using Localization.Xliff.OM.Extensibility;
    using Localization.Xliff.OM.Indicators;
    using Localization.Xliff.OM.Modules.ChangeTracking;
    using Localization.Xliff.OM.Modules.FormatStyle;
    using Localization.Xliff.OM.Modules.Metadata;
    using Localization.Xliff.OM.Modules.SizeRestriction;
    using Localization.Xliff.OM.Modules.Validation;
    using Localization.Xliff.OM.XmlNames;
    using FsXmlNames = Localization.Xliff.OM.Modules.FormatStyle.XmlNames;
    using SizeXmlNames = Localization.Xliff.OM.Modules.SizeRestriction.XmlNames;

    /// <summary>
    /// This class represents a base class for containers that contain content to be translated (namely
    /// <see cref="Unit"/> and <see cref="Group"/>).
    /// </summary>
    /// <seealso cref="XliffElement"/>
    /// <seealso cref="IExtensible"/>
    /// <seealso cref="IFormatStyleAttributes"/>
    /// <seealso cref="IMetadataStorage"/>
    /// <seealso cref="INoteContainer"/>
    /// <seealso cref="ISelectable"/>
    /// <seealso cref="ISelectNavigable"/>
    /// <seealso cref="ISizeRestrictionAttributes"/>
    [SchemaChild(
                 NamespacePrefixes.ChangeTrackingModule,
                 NamespaceValues.ChangeTrackingModule,
                 Modules.ChangeTracking.XmlNames.ElementNames.ChangeTrack,
                 typeof(ChangeTrack))]
    [SchemaChild(
                 NamespacePrefixes.MetadataModule,
                 NamespaceValues.MetadataModule,
                 Modules.Metadata.XmlNames.ElementNames.Metadata,
                 typeof(MetadataContainer))]
    [SchemaChild(NamespaceValues.Core, ElementNames.Note, typeof(Note))]
    [SchemaChild(
                 NamespacePrefixes.SizeRestrictionModule,
                 NamespaceValues.SizeRestrictionModule,
                 Modules.SizeRestriction.XmlNames.ElementNames.ProfileData,
                 typeof(ProfileData))]
    [SchemaChild(
                 NamespacePrefixes.ValidationModule,
                 NamespaceValues.ValidationModule,
                 Modules.Validation.XmlNames.ElementNames.Validation,
                 typeof(Validation))]
    public abstract class TranslationContainer : XliffElement,
                                                 IExtensible,
                                                 IFormatStyleAttributes,
                                                 IMetadataStorage,
                                                 INoteContainer,
                                                 ISelectable,
                                                 ISelectNavigable,
                                                 ISizeRestrictionAttributes
    {
        #region Member Variables
        /// <summary>
        /// The list of extensions that store custom data.
        /// </summary>
        private readonly Lazy<List<IExtension>> extensions;

        /// <summary>
        /// Indicates whether notes were already stored to avoid storing multiple instances. The notes container is
        /// not exposed directly so it starts out empty (not null).
        /// </summary>
        private bool storedNotes;

        /// <summary>
        /// The container for change tracking information associated with a sibling element, or a child of a sibling
        /// element, to the change track module within the scope of the enclosing element.
        /// </summary>
        private ChangeTrack changes;

        /// <summary>
        /// The container for metadata associated with the enclosing element.
        /// </summary>
        private MetadataContainer metadata;

        /// <summary>
        /// A container that stores notes.
        /// </summary>
        private NoteContainer noteContainer;

        /// <summary>
        /// A container for data needed by the specified profile.
        /// </summary>
        private ProfileData profileData;

        /// <summary>
        /// Validation rules that can be applied to target text both globally and locally.
        /// </summary>
        private Validation validationRules;
        #endregion Member Variables

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslationContainer"/> class.
        /// </summary>
        /// <param name="id">The Id of the container.</param>
        /// <param name="canOverrideNotes">True if notes can be replaced during deserialization, false if they cannot.
        /// </param>
        internal TranslationContainer(string id, bool canOverrideNotes)
        {
            this.RegisterElementInformation(ElementInformationFromReflection.Create(this));
            this.EnableAttribute(TranslationContainer.PropertyNames.EquivalentStorage, false);

            this.extensions = new Lazy<List<IExtension>>();
            this.noteContainer = new NoteContainer();
            Utilities.SetParent(this.noteContainer, this);

            this.Id = id;
            this.SubFormatStyle = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.storedNotes = !canOverrideNotes;
        }

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether the source content can be segmented.
        /// </summary>
        [Converter(typeof(BoolConverter))]
        [DefaultValue(true)]
        [InheritValue(Inheritance.Parent)]
        [SchemaEntity(AttributeNames.CanResegment, Requirement.Optional)]
        public bool CanResegment
        {
            get { return (bool)this.GetPropertyValue(TranslationContainer.PropertyNames.CanResegment); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.CanResegment); }
        }

        /// <summary>
        /// Gets or sets the container for change tracking information associated with a sibling element, or a child of
        /// a sibling element, to the change track module within the scope of the Unit.
        /// </summary>
        public ChangeTrack Changes
        {
            get
            {
                return this.changes;
            }

            set
            {
                ArgValidator.ParentIsNull(value);
                Utilities.SetParent(this.changes, null);
                Utilities.SetParent(value, this);
                this.changes = value;
            }
        }

        /// <summary>
        /// Gets or sets a means to specify how much storage space an inline element will use in the native format.
        /// This property is not supported on this object so a NotSupportedException will always be thrown.
        /// </summary>
        [SchemaEntity(
                      NamespacePrefixes.SizeRestrictionModule,
                      NamespaceValues.SizeRestrictionModule,
                      SizeXmlNames.AttributeNames.EquivalentStorage,
                      Requirement.Optional)]
        public string EquivalentStorage
        {
            get
            {
                string message;

                message = string.Format(
                                        Standard.Properties.Resources.XliffElement_PropertyNotSupported_Format,
                                        TranslationContainer.PropertyNames.EquivalentStorage);
                throw new NotSupportedException(message);
            }

            set
            {
                string message;

                message = string.Format(
                                        Standard.Properties.Resources.XliffElement_PropertyNotSupported_Format,
                                        TranslationContainer.PropertyNames.EquivalentStorage);
                throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Gets the list of registered extensions on the object.
        /// </summary>
        IList<IExtension> IExtensible.Extensions
        {
            get { return this.extensions.Value; }
        }

        /// <summary>
        /// Gets or sets basic formatting information using a predefined subset of HTML formatting elements. It enables
        /// the generation of HTML pages or snippets for preview and review purposes.
        /// </summary>
        [Converter(typeof(FormatStyleValueConverter))]
        [SchemaEntity(
                      NamespacePrefixes.FormatStyleModule,
                      NamespaceValues.FormatStyleModule,
                      FsXmlNames.AttributeNames.FormatStyle,
                      Requirement.Optional)]
        public FormatStyleValue? FormatStyle
        {
            get { return (FormatStyleValue?)this.GetPropertyValue(TranslationContainer.PropertyNames.FormatStyle); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.FormatStyle); }
        }

        /// <summary>
        /// Gets a value indicating whether the provider has children.
        /// </summary>
        protected override bool HasChildren
        {
            get
            {
                return base.HasChildren ||
                    this.HasNotes ||
                    (this.Changes != null) ||
                    (this.Metadata != null) ||
                    (this.ProfileData != null) ||
                    (this.ValidationRules != null);
            }
        }

        /// <summary>
        /// Gets a value indicating whether extensions are registered on the object.
        /// </summary>
        bool IExtensible.HasExtensions
        {
            get { return this.extensions.IsValueCreated && (this.extensions.Value.Count > 0); }
        }

        /// <summary>
        /// Gets a value indicating whether the object contains notes.
        /// </summary>
        public bool HasNotes
        {
            get { return this.noteContainer.HasNotes; }
        }

        /// <summary>
        /// Gets or sets the Id of the container.
        /// </summary>
        [SchemaEntity(AttributeNames.Id, Requirement.Required)]
        public string Id
        {
            get
            {
                return (string)this.GetPropertyValue(TranslationContainer.PropertyNames.Id);
            }

            set
            {
                value = string.IsNullOrEmpty(value) ? null : value;
                ArgValidator.Create(value, TranslationContainer.PropertyNames.Id).IsValidId(this);
                this.SetPropertyValue(value, TranslationContainer.PropertyNames.Id);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the element represents a leaf fragment in a selector path. If so, the
        /// selector path shouldn't contain any other fragments after this fragment.
        /// </summary>
        public bool IsLeafFragment
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the container for metadata associated with the enclosing <see cref="TranslationContainer"/>.
        /// </summary>
        public MetadataContainer Metadata
        {
            get
            {
                return this.metadata;
            }

            set
            {
                ArgValidator.ParentIsNull(value);
                Utilities.SetParent(this.metadata, null);
                Utilities.SetParent(value, this);
                this.metadata = value;
            }
        }

        /// <summary>
        /// Gets or sets the name of the resource.
        /// </summary>
        [SchemaEntity(AttributeNames.Name, Requirement.Optional)]
        public string Name
        {
            get { return (string)this.GetPropertyValue(TranslationContainer.PropertyNames.Name); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.Name); }
        }

        /// <summary>
        /// Gets the list of notes associated with the object.
        /// </summary>
        public IList<Note> Notes
        {
            get { return this.noteContainer.Notes; }
        }

        /// <summary>
        /// Gets or sets the container for data needed by the specified profile to check the part of the XLIFF document
        /// that is a sibling or descendant of a sibling of this element.
        /// </summary>
        public ProfileData ProfileData
        {
            get
            {
                return this.profileData;
            }

            set
            {
                ArgValidator.ParentIsNull(value);
                Utilities.SetParent(this.profileData, null);
                Utilities.SetParent(value, this);
                this.profileData = value;
            }
        }

        /// <summary>
        /// Gets the selector Id of the item.
        /// </summary>
        public abstract string SelectorId { get; }

        /// <summary>
        /// Gets the full path of the item from the root document.
        /// </summary>
        public string SelectorPath
        {
            get { return this.BuildSelectorPath(); }
        }

        /// <summary>
        /// Gets or sets profile specific information to inline elements so that size information can be decoupled from
        /// the native format or represented when the native data is not available in the XLIFF document.
        /// Interpretation of the value is dependent on selected <see cref="Profiles.GeneralProfile"/>. It must
        /// represent information related to how the element it is attached to contributes to the size of the text or
        /// entity in which it occurs or represents.
        /// </summary>
        [SchemaEntity(
                      NamespacePrefixes.SizeRestrictionModule,
                      NamespaceValues.SizeRestrictionModule,
                      SizeXmlNames.AttributeNames.SizeInfo,
                      Requirement.Optional)]
        public string SizeInfo
        {
            get { return (string)this.GetPropertyValue(TranslationContainer.PropertyNames.SizeInfo); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.SizeInfo); }
        }

        /// <summary>
        /// Gets or sets a reference to data that provide the same information that could be otherwise put in a sizeInfo
        /// attribute. The reference must point to an element in a <see cref="ProfileData"/> element that is a sibling
        /// to the element this attribute is attached to or a sibling to one of its ancestors. 
        /// </summary>
        [SchemaEntity(
                      NamespacePrefixes.SizeRestrictionModule,
                      NamespaceValues.SizeRestrictionModule,
                      SizeXmlNames.AttributeNames.SizeInfoReference,
                      Requirement.Optional)]
        public string SizeInfoReference
        {
            get { return (string)this.GetPropertyValue(TranslationContainer.PropertyNames.SizeInfoReference); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.SizeInfoReference); }
        }

        /// <summary>
        /// Gets or sets the size restriction to apply to the collection descendants of the element it is defined on.
        /// Interpretation of the value is dependent on the selected <see cref="Profiles.GeneralProfile"/>. It must
        /// represent the restriction to apply to the indicated sub part of the document.
        /// </summary>
        [SchemaEntity(
                      NamespacePrefixes.SizeRestrictionModule,
                      NamespaceValues.SizeRestrictionModule,
                      SizeXmlNames.AttributeNames.SizeRestriction,
                      Requirement.Optional)]
        public string SizeRestriction
        {
            get { return (string)this.GetPropertyValue(TranslationContainer.PropertyNames.SizeRestriction); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.SizeRestriction); }
        }

        /// <summary>
        /// Gets or sets the directionality of the source content.
        /// </summary>
        [Converter(typeof(ContentDirectionalityConverter))]
        [DefaultValue(ContentDirectionality.Auto)]
        [InheritValue(Inheritance.Parent)]
        [SchemaEntity(AttributeNames.SourceDirectionality, Requirement.Optional)]
        public ContentDirectionality SourceDirectionality
        {
            get
            {
                const string Name = TranslationContainer.PropertyNames.SourceDirectionality;
                return (ContentDirectionality)this.GetPropertyValue(Name);
            }

            set
            {
                this.SetPropertyValue(value, TranslationContainer.PropertyNames.SourceDirectionality);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how to handle whitespace.
        /// </summary>
        [Converter(typeof(PreservationConverter))]
        [DefaultValue(Preservation.Default)]
        [InheritValue(Inheritance.Parent)]
        [SchemaEntity(NamespacePrefixes.Xml, null, AttributeNames.SpacePreservation, Requirement.Optional)]
        public Preservation Space
        {
            get { return (Preservation)this.GetPropertyValue(TranslationContainer.PropertyNames.Space); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.Space); }
        }

        /// <summary>
        /// Gets or sets the storage restriction to apply to the collection descendants of the element it is defined on.
        /// Interpretation of the value is dependent on the selected <see cref="Profiles.StorageProfile"/>. It must
        /// represent the restriction to apply to the indicated sub part of the document.
        /// </summary>
        [SchemaEntity(
                      NamespacePrefixes.SizeRestrictionModule,
                      NamespaceValues.SizeRestrictionModule,
                      SizeXmlNames.AttributeNames.StorageRestriction,
                      Requirement.Optional)]
        public string StorageRestriction
        {
            get { return (string)this.GetPropertyValue(TranslationContainer.PropertyNames.StorageRestriction); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.StorageRestriction); }
        }

        /// <summary>
        /// Gets extra HTML attributes to use along with the HTML element declared in the FormatStyle attribute. The
        /// key is the HTML attribute name and the value is the value for that attribute.
        /// </summary>
        [Converter(typeof(SubFormatStyleConverter))]
        [HasValueIndicator(typeof(DictionaryValueIndicator<string, string>))]
        [SchemaEntity(
                      NamespacePrefixes.FormatStyleModule,
                      NamespaceValues.FormatStyleModule,
                      FsXmlNames.AttributeNames.SubFormatStyle,
                      Requirement.Optional)]
        public IDictionary<string, string> SubFormatStyle
        {
            get { return (IDictionary<string, string>)this.GetPropertyValue(TranslationContainer.PropertyNames.SubFormatStyle); }
            private set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.SubFormatStyle); }
        }

        /// <summary>
        /// Gets a value indicating whether attribute extensions are supported by the object.
        /// </summary>
        bool IExtensible.SupportsAttributeExtensions
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether element extensions are supported by the object.
        /// </summary>
        bool IExtensible.SupportsElementExtensions
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the object supports the EquivalentStorage property.
        /// </summary>
        public virtual bool SupportsEquivalentStorage
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the object supports the SizeInfo property.
        /// </summary>
        public virtual bool SupportsSizeInfo
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the object supports the SizeInfoReference property.
        /// </summary>
        public virtual bool SupportsSizeInfoReference
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the object supports the SizeRestriction property.
        /// </summary>
        public virtual bool SupportsSizeRestriction
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the object supports the StorageRestriction property.
        /// </summary>
        public virtual bool SupportsStorageRestriction
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets the directionality of the target content.
        /// </summary>
        [Converter(typeof(ContentDirectionalityConverter))]
        [DefaultValue(ContentDirectionality.Auto)]
        [InheritValue(Inheritance.Parent)]
        [SchemaEntity(AttributeNames.TargetDirectionality, Requirement.Optional)]
        public ContentDirectionality TargetDirectionality
        {
            get
            {
                const string Name = TranslationContainer.PropertyNames.TargetDirectionality;
                return (ContentDirectionality)this.GetPropertyValue(Name);
            }

            set
            {
                this.SetPropertyValue(value, TranslationContainer.PropertyNames.TargetDirectionality);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the source content is intended to be translated.
        /// </summary>
        [Converter(typeof(BoolConverter))]
        [DefaultValue(true)]
        [InheritValue(Inheritance.Parent)]
        [SchemaEntity(AttributeNames.Translate, Requirement.Optional)]
        public bool Translate
        {
            get { return (bool)this.GetPropertyValue(TranslationContainer.PropertyNames.Translate); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.Translate); }
        }

        /// <summary>
        /// Gets or sets the type of the element.
        /// </summary>
        /// <remarks>If the value is not null, the format of the value must be "prefix:value" where prefix must
        /// not be "xlf" and prefix and value must not be empty strings.</remarks>
        [SchemaEntity(AttributeNames.Type, Requirement.Optional)]
        [SuppressMessage("StyleCop.CSharp.DocumentationRules",
                         "SA1650:ElementDocumentationMustBeSpelledCorrectly",
                         Justification = "Remark contains literals described in the specification.")]
        public string Type
        {
            get { return (string)this.GetPropertyValue(TranslationContainer.PropertyNames.Type); }
            set { this.SetPropertyValue(value, TranslationContainer.PropertyNames.Type); }
        }

        /// <summary>
        /// Gets or sets validation rules that can be applied to target text both globally and locally
        /// </summary>
        public Validation ValidationRules
        {
            get
            {
                return this.validationRules;
            }

            set
            {
                ArgValidator.ParentIsNull(value);
                Utilities.SetParent(this.validationRules, null);
                Utilities.SetParent(value, this);
                this.validationRules = value;
            }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Adds a note with the specified text.
        /// </summary>
        /// <param name="note">The text of the note to add.</param>
        /// <returns>The newly created note.</returns>
        public Note AddNote(string note)
        {
            return this.noteContainer.AddNote(note);
        }

        /// <summary>
        /// Gets the child <see cref="XliffElement"/>s contained within this object.
        /// </summary>
        /// <returns>A list of child elements stored as a pair consisting of the XLIFF Name for the child and
        /// the child itself.</returns>
        protected override List<ElementInfo> GetChildren()
        {
            List<ElementInfo> result;

            result = base.GetChildren();
            if (this.HasNotes)
            {
                XmlNameInfo name;

                name = new XmlNameInfo(NamespaceValues.Core, ElementNames.Notes);
                result.Add(new ElementInfo(name, this.noteContainer));
            }

            if (this.Changes != null)
            {
                XmlNameInfo name;

                name = new XmlNameInfo(
                                       NamespacePrefixes.ChangeTrackingModule,
                                       NamespaceValues.ChangeTrackingModule,
                                       Modules.ChangeTracking.XmlNames.ElementNames.ChangeTrack);
                result.Add(new ElementInfo(name, this.Changes));
            }

            if (this.Metadata != null)
            {
                XmlNameInfo name;

                name = new XmlNameInfo(
                                       NamespacePrefixes.MetadataModule,
                                       NamespaceValues.MetadataModule,
                                       Modules.Metadata.XmlNames.ElementNames.Metadata);
                result.Add(new ElementInfo(name, this.Metadata));
            }

            if (this.ProfileData != null)
            {
                XmlNameInfo name;

                name = new XmlNameInfo(
                                       NamespacePrefixes.SizeRestrictionModule,
                                       NamespaceValues.SizeRestrictionModule,
                                       Modules.SizeRestriction.XmlNames.ElementNames.ProfileData);
                result.Add(new ElementInfo(name, this.ProfileData));
            }

            if (this.ValidationRules != null)
            {
                XmlNameInfo name;

                name = new XmlNameInfo(
                                       NamespacePrefixes.ValidationModule,
                                       NamespaceValues.ValidationModule,
                                       Modules.Validation.XmlNames.ElementNames.Validation);
                result.Add(new ElementInfo(name, this.ValidationRules));
            }

            return result;
        }

        /// <summary>
        /// Selects an item matching the selection query.
        /// </summary>
        /// <param name="path">The selection query.</param>
        /// <returns>The object that was selected from the query path, or null if no match was found.</returns>
        /// <example>The value of <paramref name="path"/> might look something like "#g=group1/f=file1/u=unit1/n=note1"
        /// which is a relative path from the current object, not a full path from the document root.</example>
        public ISelectable Select(string path)
        {
            ArgValidator.Create(path, "path").IsNotNull().StartsWith(Utilities.Constants.SelectorPathIndictator);
            return this.SelectElement(Utilities.RemoveSelectorIndicator(path));
        }

        /// <summary>
        /// Stores the <see cref="XliffElement"/> as a child of this <see cref="XliffElement"/>.
        /// </summary>
        /// <param name="child">The child to add.</param>
        /// <returns>True if the child was stored, otherwise false.</returns>
        protected override bool StoreChild(ElementInfo child)
        {
            bool result;

            result = true;
            if (child.Element is ChangeTrack)
            {
                Utilities.ThrowIfPropertyNotNull(this, PropertyNames.Changes, this.Changes);
                this.Changes = (ChangeTrack)child.Element;
            }
            else if (child.Element is MetadataContainer)
            {
                Utilities.ThrowIfPropertyNotNull(this, PropertyNames.Metadata, this.Metadata);
                this.Metadata = (MetadataContainer)child.Element;
            }
            else if (child.Element is NoteContainer)
            {
                if (this.storedNotes)
                {
                    Utilities.ThrowIfPropertyNotNull(this, "Notes", this.noteContainer);
                }

                this.noteContainer = child.Element as NoteContainer;
                Utilities.SetParent(this.noteContainer, this);
                this.storedNotes = true;
            }
            else if (child.Element is ProfileData)
            {
                Utilities.ThrowIfPropertyNotNull(this, PropertyNames.ProfileData, this.ProfileData);
                this.ProfileData = (ProfileData)child.Element;
            }
            else if (child.Element is Validation)
            {
                Utilities.ThrowIfPropertyNotNull(this, PropertyNames.ValidationRules, this.ValidationRules);
                this.ValidationRules = (Validation)child.Element;
            }
            else
            {
                result = base.StoreChild(child);
            }

            return result;
        }
        #endregion Methods

        /// <summary>
        /// This class contains the names of properties in this class.
        /// </summary>
        private static class PropertyNames
        {
            /// <summary>
            /// The name of the <see cref="TranslationContainer.CanResegment"/> property.
            /// </summary>
            public const string CanResegment = "CanResegment";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.Changes"/> property.
            /// </summary>
            public const string Changes = "Changes";

            /// <summary>
            /// The name of the EquivalentStorage property.
            /// </summary>
            public const string EquivalentStorage = "EquivalentStorage";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.FormatStyle"/> property.
            /// </summary>
            public const string FormatStyle = "FormatStyle";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.Id"/> property.
            /// </summary>
            public const string Id = "Id";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.Metadata"/> property.
            /// </summary>
            public const string Metadata = "Metadata";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.Name"/> property.
            /// </summary>
            public const string Name = "Name";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.ProfileData"/> property.
            /// </summary>
            public const string ProfileData = "ProfileData";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.SizeInfo"/> property.
            /// </summary>
            public const string SizeInfo = "SizeInfo";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.SizeInfoReference"/> property.
            /// </summary>
            public const string SizeInfoReference = "SizeInfoReference";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.SizeRestriction"/> property.
            /// </summary>
            public const string SizeRestriction = "SizeRestriction";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.SourceDirectionality"/> property.
            /// </summary>
            public const string SourceDirectionality = "SourceDirectionality";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.Space"/> property.
            /// </summary>
            public const string Space = "Space";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.StorageRestriction"/> property.
            /// </summary>
            public const string StorageRestriction = "StorageRestriction";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.SubFormatStyle"/> property.
            /// </summary>
            public const string SubFormatStyle = "SubFormatStyle";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.TargetDirectionality"/> property.
            /// </summary>
            public const string TargetDirectionality = "TargetDirectionality";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.Translate"/> property.
            /// </summary>
            public const string Translate = "Translate";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.Type"/> property.
            /// </summary>
            public const string Type = "Type";

            /// <summary>
            /// The name of the <see cref="TranslationContainer.ValidationRules"/> property.
            /// </summary>
            public const string ValidationRules = "ValidationRules";
        }
    }
}