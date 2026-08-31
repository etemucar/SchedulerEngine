using MediatR;
using SchedulerEngine.Service.Dtos.Requests;
using SchedulerEngine.Service.Dtos.Responses;
using SchedulerEngine.Core.Common.Behaviors;

namespace SchedulerEngine.Service.Features.Commands;


public class RegisterCommand : IRequest<AuthResult>, ITransactionalRequest
{
    public string GivenName    { get; set; } = null!;
    public string FamilyName   { get; set; } = null!;
    public string Identifier   { get; set; } = null!; // email veya telefon
    public string Password     { get; set; } = null!;
    public int    LanguageId   { get; set; }
}