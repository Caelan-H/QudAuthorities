using System;

namespace XRL;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CallAfterGameLoadedAttribute : Attribute
{
}