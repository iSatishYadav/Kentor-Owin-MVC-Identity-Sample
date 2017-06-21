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