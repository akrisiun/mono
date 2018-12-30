using System;
using System.Diagnostics;

namespace JetBrains.Annotations
{
    //
    // Summary:
    //     Indicates that the value of the marked element could be null sometimes, so the
    //     check for null is necessary before its usage.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate | AttributeTargets.GenericParameter)]
    [Conditional("JETBRAINS_ANNOTATIONS")]
    internal sealed class CanBeNullAttribute : Attribute
    {
        public CanBeNullAttribute() { }
    }

    //
    // Summary:
    //     Indicates that the value of the marked element could never be null.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter | AttributeTargets.Delegate | AttributeTargets.GenericParameter)]
    [Conditional("JETBRAINS_ANNOTATIONS")]
    internal sealed class NotNullAttribute : Attribute
    {
        public NotNullAttribute() { }
    }

    internal sealed class ContractAnnotation : Attribute
    {
        public ContractAnnotation(string name) { }
    }
    
    internal sealed class InvokerParameterName: Attribute
    {
        public InvokerParameterName() { }
    }
    
    internal sealed class NoEnumeration: Attribute
    {
        public NoEnumeration() { }
    }

    internal sealed class PublicAPI : Attribute
    {
        public PublicAPI() { }
    }


}