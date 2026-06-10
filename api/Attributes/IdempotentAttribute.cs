using System;

namespace SinformWcApi.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class IdempotentAttribute : Attribute
{
}
