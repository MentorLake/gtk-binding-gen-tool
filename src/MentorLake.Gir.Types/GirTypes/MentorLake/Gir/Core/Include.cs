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
    [XmlTypeAttribute("include", Namespace="http://www.gtk.org/introspection/core/1.0", AnonymousType=true)]
    [DebuggerStepThroughAttribute()]
    [DesignerCategoryAttribute("code")]
    [XmlRootAttribute("include", Namespace="http://www.gtk.org/introspection/core/1.0")]
    public partial class Include
    {
        
        /// <summary>
        /// <para>name of the dependant namespace to include</para>
        /// </summary>
        [DescriptionAttribute("name of the dependant namespace to include")]
        [RequiredAttribute(AllowEmptyStrings=true)]
        [XmlAttributeAttribute("name")]
        public string Name { get; set; }
        
        /// <summary>
        /// <para>version of the dependant namespace to use</para>
        /// </summary>
        [DescriptionAttribute("version of the dependant namespace to use")]
        [XmlAttributeAttribute("version")]
        public string Version { get; set; }
    }
}
