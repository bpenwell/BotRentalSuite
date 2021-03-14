using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System;

namespace Utilities
{
    public class Entry
    {
        public object Key;
        public object Value;
        public Entry()
        {
        }

        public Entry(object key, object value)
        {
            Key = key;
            Value = value;
        }
    }

}