using FluentValidation;
using LinkPara.Identity.Application.Features.Screens.Queries.GetScreensByUserId;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Identity.Application.Features.Screens.Queries.GetScreensByRoleId
{
    public class GetScreensByRoleIdQueryValidator : AbstractValidator<GetScreensByRoleIdQuery>
    {
        public GetScreensByRoleIdQueryValidator()
        {

        }
    }
}