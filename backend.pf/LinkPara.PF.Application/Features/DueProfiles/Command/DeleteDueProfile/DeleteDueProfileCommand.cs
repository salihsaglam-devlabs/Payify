using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.Merchants.Command.DeleteMerchant;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Features.DueProfiles.Command.DeleteDueProfile
{
    public class DeleteDueProfileCommand : IRequest
    {
        public Guid Id { get; set; }
    }
    public class DeleteDueProfileCommandHandler : IRequestHandler<DeleteDueProfileCommand>
    {
        private readonly IDueProfileService _dueProfileService;

        public DeleteDueProfileCommandHandler(IDueProfileService dueProfileService)
        {
            _dueProfileService = dueProfileService;
        }
        public async Task<Unit> Handle(DeleteDueProfileCommand request, CancellationToken cancellationToken)
        {
            await _dueProfileService.DeleteAsync(request);

            return Unit.Value;
        }
    }
}
