//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// This code was generated by XmlSchemaClassGenerator version 2.1.1174.0 using the following command:
// xscgen -n |core.xsd=MentorLake.Gir.Core -n |c.xsd=MentorLake.Gir.C -n |glib.xsd=MentorLake.Gir.GLib -n |xml.xsd=MentorLake.Gir.Xml --o GirTypes --cn --uc -ct System.Collections.Generic.List`1 --sf --nh ./core.xsd
namespace MentorLake.Gir.Core
{
    using System;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    
    
    [GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.1.1174.0")]
    [SerializableAttribute()]
    [XmlTypeAttribute("Info.elements", Namespace="http://www.gtk.org/introspection/core/1.0")]
    [DebuggerStepThroughAttribute()]
    [DesignerCategoryAttribute("code")]
    [XmlIncludeAttribute(typeof(Alias))]
    [XmlIncludeAttribute(typeof(BaseFunction))]
    [XmlIncludeAttribute(typeof(Bitfield))]
    [XmlIncludeAttribute(typeof(MentorLake.Gir.GLib.Boxed))]
    [XmlIncludeAttribute(typeof(Callback))]
    [XmlIncludeAttribute(typeof(Class))]
    [XmlIncludeAttribute(typeof(Constant))]
    [XmlIncludeAttribute(typeof(Constructor))]
    [XmlIncludeAttribute(typeof(Enumeration))]
    [XmlIncludeAttribute(typeof(Field))]
    [XmlIncludeAttribute(typeof(FunctionMacro))]
    [XmlIncludeAttribute(typeof(Interface))]
    [XmlIncludeAttribute(typeof(Member))]
    [XmlIncludeAttribute(typeof(Method))]
    [XmlIncludeAttribute(typeof(MethodInline))]
    [XmlIncludeAttribute(typeof(Property))]
    [XmlIncludeAttribute(typeof(Record))]
    [XmlIncludeAttribute(typeof(MentorLake.Gir.GLib.Signal))]
    [XmlIncludeAttribute(typeof(Union))]
    [XmlIncludeAttribute(typeof(VirtualMethod))]
    public partial class InfoElements : IDocElements
    {
        
        [XmlIgnoreAttribute()]
        private List<DocVersion> _docVersion;
        
        [XmlElementAttribute("doc-version")]
        public List<DocVersion> DocVersion
        {
            get
            {
                return _docVersion;
            }
            private set
            {
                _docVersion = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the DocVersion collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool DocVersionSpecified
        {
            get
            {
                return (this.DocVersion.Count != 0);
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Initializes a new instance of the <see cref="InfoElements" /> class.</para>
        /// </summary>
        public InfoElements()
        {
            this._docVersion = new List<DocVersion>();
            this._docStability = new List<DocStability>();
            this._doc = new List<Doc>();
            this._docDeprecated = new List<DocDeprecated>();
            this._sourcePosition = new List<SourcePosition>();
            this._attribute = new List<Attribute>();
        }
        
        [XmlIgnoreAttribute()]
        private List<DocStability> _docStability;
        
        [XmlElementAttribute("doc-stability")]
        public List<DocStability> DocStability
        {
            get
            {
                return _docStability;
            }
            private set
            {
                _docStability = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the DocStability collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool DocStabilitySpecified
        {
            get
            {
                return (this.DocStability.Count != 0);
            }
        }
        
        [XmlIgnoreAttribute()]
        private List<Doc> _doc;
        
        [XmlElementAttribute("doc")]
        public List<Doc> Doc
        {
            get
            {
                return _doc;
            }
            private set
            {
                _doc = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the Doc collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool DocSpecified
        {
            get
            {
                return (this.Doc.Count != 0);
            }
        }
        
        [XmlIgnoreAttribute()]
        private List<DocDeprecated> _docDeprecated;
        
        [XmlElementAttribute("doc-deprecated")]
        public List<DocDeprecated> DocDeprecated
        {
            get
            {
                return _docDeprecated;
            }
            private set
            {
                _docDeprecated = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the DocDeprecated collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool DocDeprecatedSpecified
        {
            get
            {
                return (this.DocDeprecated.Count != 0);
            }
        }
        
        [XmlIgnoreAttribute()]
        private List<SourcePosition> _sourcePosition;
        
        [XmlElementAttribute("source-position")]
        public List<SourcePosition> SourcePosition
        {
            get
            {
                return _sourcePosition;
            }
            private set
            {
                _sourcePosition = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the SourcePosition collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool SourcePositionSpecified
        {
            get
            {
                return (this.SourcePosition.Count != 0);
            }
        }
        
        [XmlIgnoreAttribute()]
        private List<Attribute> _attribute;
        
        [XmlElementAttribute("attribute")]
        public List<Attribute> Attribute
        {
            get
            {
                return _attribute;
            }
            private set
            {
                _attribute = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the Attribute collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool AttributeSpecified
        {
            get
            {
                return (this.Attribute.Count != 0);
            }
        }
    }
}
