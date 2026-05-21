using LiveMetricStack.Application.Applications;
using LiveMetricStack.WebApi.Contracts.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CreateAppContract = LiveMetricStack.WebApi.Contracts.Applications.CreateApplicationRequest;
using UpdateStatusContract = LiveMetricStack.WebApi.Contracts.Applications.UpdateApplicationStatusRequest;

namespace LiveMetricStack.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/applications")]
public class ApplicationsController(IApplicationsService applicationsService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ApplicationDto>>> GetAll(CancellationToken cancellationToken)
    {
        var applications = await applicationsService.GetAllAsync(cancellationToken);
        return Ok(applications);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApplicationDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var application = await applicationsService.GetByIdAsync(id, cancellationToken);
        if (application is null)
        {
            return NotFound(new { message = "Application not found." });
        }

        return Ok(application);
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationDto>> Create(CreateAppContract request, CancellationToken cancellationToken)
    {
        var application = await applicationsService.CreateAsync(new LiveMetricStack.Application.Applications.CreateApplicationRequest
        {
            Name = request.Name,
            Environment = request.Environment,
            Status = request.Status
        }, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<ApplicationDto>> UpdateStatus(Guid id, UpdateStatusContract request, CancellationToken cancellationToken)
    {
        var application = await applicationsService.UpdateStatusAsync(id, new LiveMetricStack.Application.Applications.UpdateApplicationStatusRequest
        {
            Status = request.Status
        }, cancellationToken);

        if (application is null)
        {
            return NotFound(new { message = "Application not found." });
        }

        return Ok(application);
    }
}
