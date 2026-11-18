using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BonVentApi.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Owner,Manager,Receptionist")]
[Route("api/v1/manage/[controller]")]
public abstract class AdminControllerBase : ControllerBase { }