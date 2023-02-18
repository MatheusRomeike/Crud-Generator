using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crud_Generator.Resources
{
    public class AttributeInfo
    {
        public bool? PrimaryKey { get; set; }
        public ForeignKey? ForeignKey { get; set; }
        public string Name { get; set; }
        public string SqlName { get; set; }
        public string Type { get; set; }
        public bool Nullable { get; set; }
    }
}
