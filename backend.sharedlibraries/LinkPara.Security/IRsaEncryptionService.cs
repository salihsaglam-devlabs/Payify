using LinkPara.Security.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Security
{
    public interface IRsaEncryptionService : IEncryptionService
    {
        RsaConfig GenerateKey();
    }
}
