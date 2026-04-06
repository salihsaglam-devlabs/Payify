using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.DomainEvents;

namespace LinkPara.Emoney.Domain.Events;

public class WalletCreatedEvent : DomainEvent
{
    public WalletCreatedEvent(Wallet wallet)
    {
        Wallet = wallet;
    }
    public Wallet Wallet { get; private set; }
}
