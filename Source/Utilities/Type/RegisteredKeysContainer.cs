using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Utilities.Type
{
    public class RegisteredKeysContainer
    {
        public List<BotKeyPair> RegisteredKeys;

        public RegisteredKeysContainer()
        {
            RegisteredKeys = new List<BotKeyPair>();
        }
    }
}
