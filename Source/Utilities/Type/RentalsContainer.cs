using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Serialization;

namespace Utilities.Type
{
    [XmlRoot("RentalContainers")]
    public class RentalContainers
    {
        [XmlArray("RentalContainerList")]
        [XmlArrayItem("RentalContainer")]
        public List<RentalContainer> RentalContainerList { get; set; }

        public RentalContainers()
        {
            RentalContainerList = new List<RentalContainer>();
        }
    }
}
