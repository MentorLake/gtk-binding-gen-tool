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
    [XmlTypeAttribute("repository", Namespace="http://www.gtk.org/introspection/core/1.0", AnonymousType=true)]
    [DebuggerStepThroughAttribute()]
    [DesignerCategoryAttribute("code")]
    [XmlRootAttribute("repository", Namespace="http://www.gtk.org/introspection/core/1.0")]
    public partial class Repository
    {
        
        [XmlIgnoreAttribute()]
        private List<Include> _include;
        
        [XmlElementAttribute("include")]
        public List<Include> Include
        {
            get
            {
                return _include;
            }
            private set
            {
                _include = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the Include collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool IncludeSpecified
        {
            get
            {
                return (this.Include.Count != 0);
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Initializes a new instance of the <see cref="Repository" /> class.</para>
        /// </summary>
        public Repository()
        {
            this._include = new List<Include>();
            this._include1 = new List<MentorLake.Gir.C.Include>();
            this._package = new List<Package>();
            this._namespace = new List<Namespace>();
        }
        
        [XmlIgnoreAttribute()]
        private List<MentorLake.Gir.C.Include> _include1;
        
        [XmlElementAttribute("include", Namespace="http://www.gtk.org/introspection/c/1.0")]
        public List<MentorLake.Gir.C.Include> Include1
        {
            get
            {
                return _include1;
            }
            private set
            {
                _include1 = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the Include1 collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool Include1Specified
        {
            get
            {
                return (this.Include1.Count != 0);
            }
        }
        
        [XmlIgnoreAttribute()]
        private List<Package> _package;
        
        [XmlElementAttribute("package")]
        public List<Package> Package
        {
            get
            {
                return _package;
            }
            private set
            {
                _package = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the Package collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool PackageSpecified
        {
            get
            {
                return (this.Package.Count != 0);
            }
        }
        
        [XmlIgnoreAttribute()]
        private List<Namespace> _namespace;
        
        [XmlElementAttribute("namespace")]
        public List<Namespace> Namespace
        {
            get
            {
                return _namespace;
            }
            private set
            {
                _namespace = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the Namespace collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool NamespaceSpecified
        {
            get
            {
                return (this.Namespace.Count != 0);
            }
        }
        
        /// <summary>
        /// <para>version number of the repository</para>
        /// </summary>
        [DescriptionAttribute("version number of the repository")]
        [XmlAttributeAttribute("version")]
        public string Version { get; set; }
        
        [XmlAttributeAttribute("identifier-prefixes", Namespace="http://www.gtk.org/introspection/c/1.0", Form=XmlSchemaForm.Qualified)]
        public string IdentifierPrefixes { get; set; }
        
        [XmlAttributeAttribute("symbol-prefixes", Namespace="http://www.gtk.org/introspection/c/1.0", Form=XmlSchemaForm.Qualified)]
        public string SymbolPrefixes { get; set; }
    }
}
