using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using System.Globalization;
using LinkPara.SharedModels.Persistence;
using LinkPara.Security;

namespace LinkPara.Emoney.Infrastructure.Services
{
    public class WalletNumberGenerator : IWalletNumberGenerator
    {
        private readonly IGenericRepository<Wallet> _repository;
        private readonly ISecureRandomGenerator _randomGenerator;

        public WalletNumberGenerator(IGenericRepository<Wallet> repository,
            ISecureRandomGenerator randomGenerator)
        {
            _repository = repository;
            _randomGenerator = randomGenerator;
        }

        public string Generate()
        {
            var any = false;
            var walletNumber = string.Empty;
            do
            {
                walletNumber = _randomGenerator.GenerateSecureRandomNumber(10).ToString(CultureInfo.InvariantCulture);
                any = _repository.GetAll().Any(s => s.WalletNumber == walletNumber);
            }
            while (any);

            return walletNumber;
        }
    }
}
