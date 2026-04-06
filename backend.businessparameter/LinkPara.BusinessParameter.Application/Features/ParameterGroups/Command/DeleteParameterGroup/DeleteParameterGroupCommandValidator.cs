using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.DeleteParameterGroup;
    public class DeleteParameterGroupCommandValidator : AbstractValidator<DeleteParameterGroupCommand>
    {
        public DeleteParameterGroupCommandValidator()
        {
            RuleFor(x => x.Id).NotNull().NotEmpty();
        }
    }
