using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using backend.Email;
using backend.Models;
using backend.Models.Dtos;
using backend.Stripe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Stripe;

namespace backend.Controllers;
[ApiController]
[Route("shop")]
public class ShopController : ControllerBase {
    private readonly IEmailSender _emailSender;

    private readonly UserManager<SC> _userManager;
    private readonly SignInManager<SC> _signInManager;

    private readonly IMapper _mapper;
    private readonly IStripeAppService _stripe;

    private readonly IConfiguration _configuration;

    public ShopController(IEmailSender emailSender, UserManager<SC> userManager, SignInManager<SC> signInManager, IMapper mapper, IStripeAppService stripe, IConfiguration configuration)
    {
        _emailSender = emailSender;
        _userManager = userManager;
        _signInManager = signInManager;
        _mapper = mapper;
        _stripe = stripe;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<IActionResult> Register([FromBody] ShopDto sd) {
        Shop shop = _mapper.Map<ShopDto, Shop>(sd);
        _emailSender.SendEmail(new EmailMessage(new string[] { shop.Email! }, "Bevestig uw e-mail", string.Format("")));
        IdentityResult ir = await _userManager.CreateAsync(shop, sd.Password);
        if (ir.Succeeded) {
            string token = await _userManager.GenerateEmailConfirmationTokenAsync(shop);
            Console.WriteLine(shop.Id);
            Console.WriteLine(WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token)));
            return Ok();
        } 
        else return StatusCode(422, ir.Errors.Select(x => x.Code).First());
    }
    [HttpGet("onboard/{id}/{token}")]
    public async Task<IActionResult> Onboard(string id, string token) {
        Shop? shop = await _userManager.FindByIdAsync(id) as Shop;
        if (shop == null) return BadRequest();
        Account account = _stripe.CreateExpressAccount(shop.Email!);
        shop.StripeId = account.Id;
        AccountLink accountLink = _stripe.CreateAccountLink(account.Id, id, token);
        IdentityResult uir = await _userManager.UpdateAsync(shop);
        if (uir.Succeeded)  return Ok(accountLink.Url);
        return BadRequest(uir.Errors);
    }
    [HttpGet("onboard-complete/{id}/{token}")]
    public async Task<IActionResult> OnboardComplete(string id, string token) {
        Shop? shop = await _userManager.FindByIdAsync(id) as Shop;
        if (shop == null) return BadRequest();
        if (shop.StripeId == null) return BadRequest(new {
            Code = 0,
            Title = "Stripe gegevens zijn onbekend",
            Subtitle = "Probeer alstublieft opnieuw met stripe te koppelen door op de knop in uw bevestingsmail te klikken"
        });
        Account stripeAccount = _stripe.GetAccount(shop.StripeId);
        if (!stripeAccount.PayoutsEnabled) {
            AccountLink accountLink = _stripe.CreateAccountLink(stripeAccount.Id, id, token);
            return BadRequest(new {
                Code = 1,
                Title = "Stripe gegevens zijn incompleet",
                Subtitle = "Probeer alstublieft opnieuw met stripe te koppelen door op de knop hieronder te klikken",
                Url = accountLink.Url
            });   
        } 
        IdentityResult ir = await _userManager.ConfirmEmailAsync(shop, Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token)));
        if (ir.Succeeded) return Ok();
        return StatusCode(422, ir.Errors);
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto ld) {
        Shop? s = await _userManager.FindByEmailAsync(ld.Email) as Shop;  
        if (s == null) return BadRequest("Ongeldige login gegevens");
        Microsoft.AspNetCore.Identity.SignInResult sir = await _signInManager.PasswordSignInAsync(s, ld.Password, false, true);
        if (sir.Succeeded) {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("Token:Secret")!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, s.Id.ToString()),
                    new Claim(ClaimTypes.Role, "shop")
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encryptedtoken = tokenHandler.WriteToken(token);
            return Ok(encryptedtoken);
        }
        return BadRequest("Ongeldige login gegevens");
    }
    
}