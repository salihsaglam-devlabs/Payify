using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface IDataEncryptionKeyService
    {
        Task<string> GetDataEncryptionKeyAsync();
    }
}
