using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Guest;

[ApiController]
[Authorize(Roles = "Guest")]
[Route("api/v1/[controller]")]
public abstract class GuestControllerBase : ControllerBase { }