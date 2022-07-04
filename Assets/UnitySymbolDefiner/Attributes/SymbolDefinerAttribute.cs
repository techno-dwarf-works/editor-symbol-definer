using System;
using System.Diagnostics;

namespace UnitySymbolDefiner.Attributes
{
    [Conditional("UNITY_EDITOR")]
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class SymbolDefinerAttribute : Attribute
    {
        public bool AutoDefine { get; }

        public SymbolDefinerAttribute(bool autoDefine)
        {
            AutoDefine = autoDefine;
        }

        public SymbolDefinerAttribute()
        {
            AutoDefine = false;
        }
    }
}