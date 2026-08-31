using AutoMapper;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchedulerEngine.Api.Models;
using SchedulerEngine.Api.Security;
using SchedulerEngine.Service.Features.Commands;
using SchedulerEngine.Service.Features.Queries;

namespace SchedulerEngine.Api.Controllers;

/// <summary>
/// TMF dışı endpoint - Auth/Admin ile aynı konvansiyon: api/v1/ prefix'i.
/// Sadece X-Api-Key header'ı geçerli olan dış servisler (FinYo, DocDes vb.)
/// erişebilir - JWT/cookie login'e hiç bakmaz (bkz. ApiKeyAuthenticationHandler).
/// </summary>
[Authorize(AuthenticationSchemes = ApiKeyAuthConstants.SchemeName)]
[Route("api/v1/jobs")]
[ApiController]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public JobsController(IMapper mapper, IMediator mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    /// <summary>
    /// ApiKeyAuthenticationHandler'ın set ettiği "credential_id" claim'ini okur.
    /// Job çalışırken (ExternalTaskJob) hangi Organization'a ait olduğunu,
    /// callback URL'ini ve outbound anahtarı bununla çözer.
    /// </summary>
    private Guid GetCallerCredentialId()
    {
        var raw = User.FindFirst("credential_id")?.Value
            ?? throw new InvalidOperationException("credential_id claim'i bulunamadı - ApiKeyAuthenticationHandler'ın doğru çalıştığından emin olun.");

        return Guid.Parse(raw);
    }

    /// <summary>Job'u hemen kuyruğa alır.</summary>
    [HttpPost("enqueue")]
    [ProducesResponseType(typeof(EnqueueJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Enqueue([FromBody] ExternalTaskRequestModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var command = _mapper.Map<EnqueueExternalTaskCommand>(model) with { CallerCredentialId = GetCallerCredentialId() };
        var result = await _mediator.Send(command);
        var response = _mapper.Map<EnqueueJobResponse>(result);

        return Ok(response);
    }

    /// <summary>Job'u belirli bir gecikmeyle çalıştırır.</summary>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(ScheduleJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Schedule([FromBody] ScheduleExternalTaskRequestModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var command = _mapper.Map<ScheduleExternalTaskCommand>(model) with { CallerCredentialId = GetCallerCredentialId() };
        var result = await _mediator.Send(command);
        var response = _mapper.Map<ScheduleJobResponse>(result);

        return Ok(response);
    }

    /// <summary>Cron ifadesiyle tekrarlayan bir job tanımlar/günceller.</summary>
    [HttpPost("recurring")]
    [ProducesResponseType(typeof(RecurringJobResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddOrUpdateRecurring([FromBody] RecurringJobRequestModel model)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var command = _mapper.Map<AddOrUpdateRecurringJobCommand>(model) with { CallerCredentialId = GetCallerCredentialId() };
        var result = await _mediator.Send(command);
        var response = _mapper.Map<RecurringJobResponse>(result);

        return Ok(response);
    }

    /// <summary>Mevcut bir recurring job'ı kaldırır.</summary>
    [HttpDelete("recurring/{recurringJobId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRecurring([FromRoute] string recurringJobId)
    {
        await _mediator.Send(new RemoveRecurringJobCommand
        {
            RecurringJobId = recurringJobId,
            CallerCredentialId = GetCallerCredentialId()
        });
        return NoContent();
    }

    /// <summary>Çağıran servisin kendi kaydettirdiği recurring job'ları listeler.</summary>
    [HttpGet("recurring")]
    [ProducesResponseType(typeof(List<RecurringJobListItemResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyRecurringJobs([FromQuery] bool includeRemoved = false)
    {
        var result = await _mediator.Send(new GetMyRecurringJobsQuery(GetCallerCredentialId(), includeRemoved));
        var response = _mapper.Map<List<RecurringJobListItemResponse>>(result);

        return Ok(response);
    }
}