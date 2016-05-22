using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AppSyndication.AccountService.Web.Extensions;
using AppSyndication.AccountService.Web.Models;
using BrockAllen.MembershipReboot;
using FireGiant.MembershipReboot.AzureStorage;
using IdentityModel;
using Microsoft.AspNet.Mvc;

namespace AppSyndication.AccountService.Web.Controllers
{
    [Route("account/update/[action]")]
    public class AccountUpdateController : Controller
    {
        private readonly AtsUserService _userService;

        public AccountUpdateController(AtsUserService userService)
        {
            _userService = userService;
        }

#if DEBUG
        public string Create()
        {
            var user = _userService.GetByUsername("robmen");

            if (user != null)
            {
                return "Account already existed";
            }

            _userService.CreateAccount("robmen", null, "rob@robmensching.com", claims: new[]
            {
                new Claim(JwtClaimTypes.Name, "Rob Mensching"),
                new Claim(JwtClaimTypes.Role, "admin"),
            });

            return "Check your email";
        }
#endif

        [Route("{id}")]
        public ActionResult Email(string id)
        {
            var account = _userService.GetByVerificationKey(id);

            if (account == null)
            {
                return this.View("Error");
            }

            var vm = new ChangeEmailUsingResetKey()
            {
                Email = account.Email,
                Username = account.Username,
                Key = id
            };

            if (account.HasPassword())
            {
                return this.View(vm);
            }

            return this.View("Register", vm);
        }

        [HttpPost]
        [Route("{id?}")]
        [ValidateAntiForgeryToken]
        public ActionResult Email(ChangeEmailUsingResetKey model)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    var account = _userService.GetByVerificationKey(model.Key);

                    if (account == null)
                    {
                        this.ModelState.AddModelError(String.Empty, "Error registering account. The key might be invalid.");
                    }
                    else if (account.HasPassword())
                    {
                        _userService.VerifyEmailFromKey(model.Key, model.Password);

                        return this.RedirectToAction("Success", new { id = "email" });
                    }
                    else if (!account.IsAccountVerified)
                    {
                        _userService.SetPassword(account.ID, model.Password);

                        _userService.SetConfirmedEmail(account.ID, account.Email);

                        return this.RedirectToAction("Created");
                    }
                    else
                    {
                        return this.View("Error");
                    }
                }
                catch (ValidationException ex)
                {
                    this.ModelState.AddModelError("", ex.Message);
                }
            }

            return this.View(model);
        }

        public ActionResult Reset()
        {
            // If the user is already signed in, send a password reset request to the
            // email address we have associated with their account.
            //
            if (this.User.Authenticated())
            {
                try
                {
                    _userService.ResetPassword(this.User.Id());
                    return this.RedirectToAction("Sent");
                }
                catch (ValidationException)
                {
                    // The user is signed in with an unknown account id. Not clear how this
                    // might happen so tell the user to email support.
                    //
                    return this.View("Error");
                }
            }

            // Otherwise, prompt the user for their email address so we can send
            // the password reset request to them.
            //
            return this.View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reset(RequestPasswordUpdate model)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    _userService.ResetPassword(model.Email);
                    return this.RedirectToAction("Sent");
                }
                catch (ValidationException ex)
                {
                    this.ModelState.AddModelError(String.Empty, ex.Message);
                }
            }

            return this.View();
        }

        [Route("{id}")]
        public ActionResult Password(string id)
        {
            var account = _userService.GetByVerificationKey(id);

            if (account == null)
            {
                return this.View("Error");
            }

            if (account.HasPassword())
            {
                return this.View(new ChangePasswordUsingResetKey { Key = id });
            }

            var vm = new ChangeEmailUsingResetKey()
            {
                Email = account.Email,
                Username = account.Username,
                Key = id
            };

            return this.View("Register", vm);
        }

        [HttpPost]
        [Route("{id?}")]
        [ValidateAntiForgeryToken]
        public ActionResult Password(ChangePasswordUsingResetKey model)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    AtsUser account;
                    if (_userService.ChangePasswordFromResetKey(model.Key, model.Password, out account))
                    {
                        // Really wish we could do this but we must send the user through sign-on service to be signed in.
                        //_authenticationService.SignIn(account);

                        return this.RedirectToAction("Success", new { id = "password" });
                    }
                    else
                    {
                        this.ModelState.AddModelError(String.Empty, "Error changing password. The key might be invalid.");
                    }
                }
                catch (ValidationException ex)
                {
                    this.ModelState.AddModelError(String.Empty, ex.Message);
                }
            }

            return this.View(model);
        }

        public ActionResult Sent()
        {
            return this.View();
        }

        [Route("{id?}")]
        public ActionResult Success(string id)
        {
            switch (id.ToLowerInvariant())
            {
                case "password":
                    this.ViewBag.UpdatedTitleCase = "Password";
                    this.ViewBag.UpdatedLowerCase = "password";
                    break;

                case "email":
                    this.ViewBag.UpdatedTitleCase = "Email";
                    this.ViewBag.UpdatedLowerCase = "email";
                    break;

                default:
                    this.ViewBag.UpdatedTitleCase = "Account";
                    this.ViewBag.UpdatedLowerCase = "account";
                    break;
            }

            return this.View();
        }

        public ActionResult Created()
        {
            return this.View();
        }

        [Route("{id}")]
        public ActionResult Cancel(string id)
        {
            try
            {
                bool closed;
                _userService.CancelVerification(id, out closed);

                if (closed)
                {
                    return this.View("Closed");
                }
                else
                {
                    return this.View("Cancel");
                }
            }
            catch (ValidationException ex)
            {
                this.ModelState.AddModelError(String.Empty, ex.Message);
            }

            return this.View("Error");
        }
    }
}
