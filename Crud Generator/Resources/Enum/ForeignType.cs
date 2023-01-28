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
        UmPraUm,
        [Description("Um pra muitos")]
        UmPraMuitos,
        [Description("Muitos pra um")]
        MuitosPraUm,
        [Description("Muitos pra muitos")]
        MuitosPraMuitos
    }
}
