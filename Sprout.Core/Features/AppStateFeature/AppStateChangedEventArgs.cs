using System;
using System.Collections.Generic;
using System.Text;

namespace Sprout.Core.Features.AppStateFeature;

public class AppStateChangedEventArgs : EventArgs
{
    public string Key { get; }
    public object? Value { get; }
    public AppStateChangedEventArgs(string key, object? value)
    {
        Key = key;
        Value = value;
    }
}