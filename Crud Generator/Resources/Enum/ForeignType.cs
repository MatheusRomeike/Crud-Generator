using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crud_Generator.Resources.Enum
{
    public enum ForeignType
    {
        [Description("Um pra um")]
        UmPraUm = 1,
        [Description("Um pra muitos")]
        UmPraMuitos = 2,
        [Description("Muitos pra um")]
        MuitosPraUm = 3,
        [Description("Muitos pra muitos")]
        MuitosPraMuitos = 4
    }
}
