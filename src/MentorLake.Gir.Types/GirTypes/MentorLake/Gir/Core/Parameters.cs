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
    /// <para>parameters element of a callable, that is in general parameters of a function or similar</para>
    /// </summary>
    [DescriptionAttribute("parameters element of a callable, that is in general parameters of a function or " +
        "similar")]
    [GeneratedCodeAttribute("XmlSchemaClassGenerator", "2.1.1174.0")]
    [SerializableAttribute()]
    [XmlTypeAttribute("parameters", Namespace="http://www.gtk.org/introspection/core/1.0", AnonymousType=true)]
    [DebuggerStepThroughAttribute()]
    [DesignerCategoryAttribute("code")]
    [XmlRootAttribute("parameters", Namespace="http://www.gtk.org/introspection/core/1.0")]
    public partial class Parameters
    {
        
        [XmlIgnoreAttribute()]
        private List<BaseParam> _parameter;
        
        [XmlElementAttribute("parameter")]
        public List<BaseParam> Parameter
        {
            get
            {
                return _parameter;
            }
            private set
            {
                _parameter = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the Parameter collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool ParameterSpecified
        {
            get
            {
                return (this.Parameter.Count != 0);
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Initializes a new instance of the <see cref="Parameters" /> class.</para>
        /// </summary>
        public Parameters()
        {
            this._parameter = new List<BaseParam>();
            this._instanceParameter = new List<BaseParam>();
        }
        
        [XmlIgnoreAttribute()]
        private List<BaseParam> _instanceParameter;
        
        [XmlElementAttribute("instance-parameter")]
        public List<BaseParam> InstanceParameter
        {
            get
            {
                return _instanceParameter;
            }
            private set
            {
                _instanceParameter = value;
            }
        }
        
        /// <summary>
        /// <para xml:lang="en">Gets a value indicating whether the InstanceParameter collection is empty.</para>
        /// </summary>
        [XmlIgnoreAttribute()]
        public bool InstanceParameterSpecified
        {
            get
            {
                return (this.InstanceParameter.Count != 0);
            }
        }
    }
}
