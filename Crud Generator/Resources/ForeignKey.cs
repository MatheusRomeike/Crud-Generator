using Crud_Generator.Resources.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crud_Generator.Resources
{
    public class ForeignKey
    {
        public string ReferencesRelationName { get; set; }
        public string ReferencesSqlName { get; set; }
        public ForeignType ForeignType { get; set; }
    }
}
