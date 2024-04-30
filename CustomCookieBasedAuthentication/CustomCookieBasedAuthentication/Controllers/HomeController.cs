using CustomCookieBasedAuthentication.Data;
using CustomCookieBasedAuthentication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CustomCookieBasedAuthentication.Controllers
{
    public class HomeController : Controller
    {
        private readonly CookieContext _cookie;

        public HomeController(CookieContext cookie)
        {
            _cookie = cookie;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SignIn()
        {
            return View(new UserSignInModel());
        }
        [HttpPost]
        public async Task<IActionResult> SignIn(UserSignInModel model)
        {
            var user = await _cookie.AppUsers.SingleOrDefaultAsync(x => x.UserName == model.UserName);
            if (user != null)
            {
                var role = _cookie.AppRoles.Where(x => x.AppUserRoles.Any(x => x.UserId == user.Id)).Select(x=>x.Definition).ToList();
                var claims = new List<Claim>
                {
                 new Claim(ClaimTypes.Name, model.UserName)
                 };
                foreach (var item in role)
                {
                    claims.Add(new Claim(ClaimTypes.Role, item));
                }
                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.Remember
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "kullanıcı adı veya şifre hatalı");
            return View(model);
        }
        public IActionResult Admin()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }
        public async Task< IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index");
        }

    }
}
