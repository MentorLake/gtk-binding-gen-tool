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
    
    
    /// <summary>
    /// <para>Position of the documentation in the original source code</para>
    /// </summary>
    [DescriptionAttribute("Position of the documentation in the original source code")]
    [GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.1.1174.0")]
    [SerializableAttribute()]
    [XmlTypeAttribute("source-position", Namespace="http://www.gtk.org/introspection/core/1.0", AnonymousType=true)]
    [DebuggerStepThroughAttribute()]
    [DesignerCategoryAttribute("code")]
    [XmlRootAttribute("source-position", Namespace="http://www.gtk.org/introspection/core/1.0")]
    public partial class SourcePosition
    {
        
        /// <summary>
        /// <para>File name of the source of the documentation</para>
        /// </summary>
        [DescriptionAttribute("File name of the source of the documentation")]
        [RequiredAttribute(AllowEmptyStrings=true)]
        [XmlAttributeAttribute("filename")]
        public string Filename { get; set; }
        
        /// <summary>
        /// <para>The first line of the documentation in the source code</para>
        /// </summary>
        [DescriptionAttribute("The first line of the documentation in the source code")]
        [RequiredAttribute(AllowEmptyStrings=true)]
        [XmlAttributeAttribute("line")]
        public string Line { get; set; }
        
        /// <summary>
        /// <para>The first column of the documentation in the source code</para>
        /// </summary>
        [DescriptionAttribute("The first column of the documentation in the source code")]
        [XmlAttributeAttribute("column")]
        public string Column { get; set; }
    }
}
