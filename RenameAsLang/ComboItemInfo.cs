using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenameAsLang
{
    class ComboItemInfo
    {
        EncodingInfo ei_;
        public ComboItemInfo(EncodingInfo ei)
        {
            ei_ = ei;
        }
        public override string ToString()
        {
            return ei_.DisplayName;
        }
        public EncodingInfo EncInfo
        {
            get { return ei_; }
        }
    }
}
