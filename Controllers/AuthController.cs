using Asp.Versioning;
using LumenEstoque.DTOs.IdentityDTOs;
using LumenEstoque.Models;
using LumenEstoque.Services.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LumenEstoque.Controllers;

[Route("api/v{version:apiVersion}/auth")]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        ITokenService tokenService,
        IConfiguration config,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova role no sistema.
    /// </summary>
    /// <remarks>
    /// Requer permissão de <c>Admin</c>.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/auth/CreateRole?roleName=Admin
    ///
    /// </remarks>
    /// <param name="roleName">Nome da role.</param>
    /// <returns>Status da criação da role.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("CreateRole")]
    public async Task<IActionResult> CreateRole(string roleName)
    {
        var roleExist = await _roleManager.RoleExistsAsync(roleName);

        if (!roleExist)
        {
            var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));

            if (roleResult.Succeeded)
            {
                _logger.LogInformation(1, "Roles Added");
                return StatusCode(StatusCodes.Status200OK,
                    new ResponseDTO { Status = "Success", Message = $"Role {roleName} added successfully" });
            }

            _logger.LogInformation(2, "Error");
            return StatusCode(StatusCodes.Status400BadRequest,
                new ResponseDTO { Status = "Error", Message = $"Issue adding the new {roleName} role" });
        }

        return StatusCode(StatusCodes.Status400BadRequest,
            new ResponseDTO { Status = "Error", Message = "Role already exist." });
    }

    /// <summary>
    /// Adiciona um usuário a uma role.
    /// </summary>
    /// <remarks>
    /// Requer permissão de <c>Admin</c>.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/auth/AddUserToRole?email=user@email.com&amp;roleName=Admin
    ///
    /// </remarks>
    /// <param name="email">Email do usuário.</param>
    /// <param name="roleName">Nome da role.</param>
    /// <returns>Status da operação.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("AddUserToRole")]
    public async Task<IActionResult> AddUserToRole(string email, string roleName)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user != null)
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (result.Succeeded)
            {
                _logger.LogInformation(1, $"User {user.Email} added to the {roleName} role");
                return StatusCode(StatusCodes.Status200OK,
                    new ResponseDTO { Status = "Success", Message = $"User {user.Email} added to the {roleName} role" });
            }

            _logger.LogInformation(1, $"Error: Unable to add user {user.Email} to the {roleName} role");
            return StatusCode(StatusCodes.Status400BadRequest,
                new ResponseDTO { Status = "Error", Message = $"Error adding user to role" });
        }

        return BadRequest(new { error = "Unable to find user" });
    }

    /// <summary>
    /// Realiza login e retorna tokens de autenticação.
    /// </summary>
    /// <remarks>
    /// Retorna um JWT válido e um refresh token.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/auth/login
    ///
    /// </remarks>
    /// <param name="model">Credenciais do usuário.</param>
    /// <returns>Token de acesso, refresh token e expiração.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var user = await _userManager.FindByNameAsync(model.Username!);

        if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password!))
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));

            var token = _tokenService.GenerateAccessToken(authClaims, _config);
            var refreshToken = _tokenService.GenerateRefreshToken();

            _ = int.TryParse(_config["JWT:RefreshTokenValidityInMinutes"],
                out int refreshTokenValidityInMinutes);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddMinutes(refreshTokenValidityInMinutes);

            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = refreshToken,
                Expiration = token.ValidTo
            });
        }

        return Unauthorized();
    }

    /// <summary>
    /// Registra um novo usuário no sistema.
    /// </summary>
    /// <remarks>
    /// Cria uma nova conta com username, email e senha.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/auth/register
    ///
    /// </remarks>
    /// <param name="model">Dados do usuário.</param>
    /// <returns>Status da criação.</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO model)
    {
        var userExists = await _userManager.FindByNameAsync(model.Username!);

        if (userExists != null)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ResponseDTO { Status = "Error", Message = "User already exists!" });
        }

        ApplicationUser user = new()
        {
            Email = model.Email,
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = model.Username
        };

        var result = await _userManager.CreateAsync(user, model.Password!);

        if (!result.Succeeded)
        {
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ResponseDTO { Status = "Error", Message = "User creation failed." });
        }

        return Ok(new ResponseDTO { Status = "Success", Message = "User created successfully!" });
    }

    /// <summary>
    /// Gera um novo access token a partir de um refresh token válido.
    /// </summary>
    /// <remarks>
    /// Utilizado quando o token de acesso expira.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/auth/refresh-token
    ///
    /// </remarks>
    /// <param name="tokenModel">Access token expirado e refresh token.</param>
    /// <returns>Novo token de acesso e refresh token.</returns>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(TokenModelDTO tokenModel)
    {
        if (tokenModel is null)
            return BadRequest("Invalid client request");

        var principal = _tokenService.GetPrincipalFromExpiredToken(tokenModel.AccessToken!, _config);

        if (principal == null)
            return BadRequest("Invalid access token/refresh token");

        var user = await _userManager.FindByNameAsync(principal.Identity!.Name!);

        if (user == null || user.RefreshToken != tokenModel.RefreshToken ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return BadRequest("Invalid access token/refresh token");
        }

        var newAccessToken = _tokenService.GenerateAccessToken(principal.Claims.ToList(), _config);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            Token = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
            RefreshToken = newRefreshToken
        });
    }

    /// <summary>
    /// Revoga o refresh token de um usuário.
    /// </summary>
    /// <remarks>
    /// Requer permissão de <c>Admin</c>.
    ///
    /// Exemplo de requisição:
    ///
    ///     POST /api/v1/auth/revoke/username
    ///
    /// </remarks>
    /// <param name="username">Nome do usuário.</param>
    /// <returns>Status da operação.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost("revoke/{username}")]
    public async Task<IActionResult> Revoke(string username)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user == null)
            return BadRequest("Invalid user name");

        user.RefreshToken = null;
        await _userManager.UpdateAsync(user);

        return NoContent();
    }
}