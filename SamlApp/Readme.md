# How to create SAML Aware .NET Apps


1. Creare a new ASP.NET MVC app with *Individual Account* as Authentication, this wil use `Microsoft.Aspnet.Identity` package.

2. Install `Kentor.AuthServices.Owin` nuget packae. 

3. Add reference to `System.IdentityModel 4.0.00`.

4. Put *Signing Certificate* from your IdP (e.g. *Kentor.AuthService.StubIdp.cer*) in `App_Data`. folder*

5. For `Single Log Out` put a *Service Certificate* (e.g. *Kentor.AuthService.Tests.pfx*) in `App_Data` folder.*

6. For separation of SAML configuration, create a new class file named `SamlConfig.cs` under `App_Start` and paste following code.  

````
using Kentor.AuthServices;
using Kentor.AuthServices.Configuration;
using Kentor.AuthServices.Metadata;
using Kentor.AuthServices.Owin;
using Kentor.AuthServices.WebSso;
using System;
using System.IdentityModel.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Web.Hosting;

namespace SamlApp
{
    public static class SamlConfig
    {
        internal static KentorAuthServicesAuthenticationOptions CreateAuthServicesOptions()
        {

            var spOptions = CreateSPOptions();
            var authServicesOptions = new KentorAuthServicesAuthenticationOptions(false)
            {
                SPOptions = spOptions
            };

            var idp = new IdentityProvider(new EntityId("http://stubidp.kentor.se/Metadata"), spOptions)
            {
                AllowUnsolicitedAuthnResponse = true,
                Binding = Saml2BindingType.HttpRedirect,
                SingleSignOnServiceUrl = new Uri("http://stubidp.kentor.se")
            };

            idp.SigningKeys.AddConfiguredKey(
                new X509Certificate2(
                    HostingEnvironment.MapPath(
                        "~/App_Data/Kentor.AuthServices.StubIdp.cer")));

            authServicesOptions.IdentityProviders.Add(idp);          

            return authServicesOptions;
        }

        private static SPOptions CreateSPOptions()
        {
            var spOptions = new SPOptions
            {
                EntityId = new EntityId("http://localhost:9059/AuthServices"),
                ReturnUrl = new Uri("http://localhost:9059/Account/ExternalLoginCallback")
            };

            var attributeConsumingService = new AttributeConsumingService("AuthServices")
            {
                IsDefault = true,
            };

            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("urn:someName")
                {
                    FriendlyName = "Some Name",
                    IsRequired = true,
                    NameFormat = RequestedAttribute.AttributeNameFormatUri
                });

            attributeConsumingService.RequestedAttributes.Add(
                new RequestedAttribute("Minimal"));

            spOptions.AttributeConsumingServices.Add(attributeConsumingService);

            spOptions.ServiceCertificates.Add(new X509Certificate2(
                AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/App_Data/Kentor.AuthServices.Tests.pfx"));

            return spOptions;
        }
    }
}
````
7. Change following values:  
  
   i. http://stubidp.kentor.se/Metadata => Your IdP Metadata url (not a local Metadata xml file)    
   ii. http://stubidp.kentor.se => SSO Url of your IdP
   iii. http://localhost:9059/AuthServices => https://<your app url>/AuthServices
   iv. http://localhost:9059/Account/ExternalLoginCallback => https://<your app url>/Account/ExternalLoginCallback
   v. "~/App_Data/Kentor.AuthServices.StubIdp.cer" => "<path of your IdP's signing certificate >"
   vi. AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/App_Data/Kentor.AuthServices.Tests.pfx") => "<path of your signing certicate."

8. Add following line in `Startup.cs` or `Startup.Auth.cs` at the end of `ConfigureAuth(IAppBuilder app)` method.
*Certificates can also be put in a common place on an application server.