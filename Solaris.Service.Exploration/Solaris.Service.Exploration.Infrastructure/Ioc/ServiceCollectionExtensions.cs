using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Solaris.Service.Exploration.Core.Extensions;

namespace Solaris.Service.Exploration.Infrastructure.Ioc
{
    public static class ServiceCollectionExtensions
    {
        private const string SOLUTION_NAME = "Solaris";
        private static IServiceCollection Services { get; set; }
        private static List<Assembly> Assemblies { get; }
        
        static ServiceCollectionExtensions()
        {
            Assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(t => t.FullName.Contains(SOLUTION_NAME))
                .ToList();
        }

        public static void InjectForNamespace(this IServiceCollection collection, string nameSpace)
        {
            Services = collection;
            InjectDependenciesForAssembly(Assemblies.FirstOrDefault(t => nameSpace.Contains(t.GetName().Name)), nameSpace);
        }

        private static void InjectDependenciesForAssembly(Assembly assembly, string nameSpace)
        {
            if (assembly == null)
                return;

            var typesToRegister = assembly.GetTypes()
                .Where(x => x.Namespace != null && x.Namespace.Contains(nameSpace, StringComparison.InvariantCultureIgnoreCase))
                .ToList();
            typesToRegister.ForEach(RegisterType);
        }

        private static void RegisterType(Type typeToRegister)
        {
            var registrationType = typeToRegister.GetCustomAttribute<RegistrationKindAttribute>();
            if (registrationType == null)
            {
                if (typeToRegister.GetInterfaces().Any())
                    Services.AddScoped(typeToRegister.GetInterfaces().First(), typeToRegister);
                return;
            }


            switch (registrationType.Type)
            {
                case RegistrationType.Singleton:
                    if (registrationType.AsSelf)
                        Services.AddSingleton(typeToRegister);
                    else
                        Services.AddSingleton(typeToRegister.GetInterfaces().First(), typeToRegister);
                    break;
                case RegistrationType.Scoped:
                    if (registrationType.AsSelf)
                        Services.AddScoped(typeToRegister);
                    else
                        Services.AddScoped(typeToRegister.GetInterfaces().First(), typeToRegister);
                    break;
                case RegistrationType.Transient:
                    if (registrationType.AsSelf)
                        Services.AddTransient(typeToRegister);
                    else
                        Services.AddTransient(typeToRegister.GetInterfaces().First(), typeToRegister);
                    break;
                default:
                    Services.AddScoped(typeToRegister.GetInterfaces().First(), typeToRegister);
                    break;
            }
        }

        public static void InjectRabbitMq(this IServiceCollection collection)
        {
            // var assembly = Assembly.Load("Solaris.Web.SolarApi.Infrastructure");
            // assembly.GetTypesForPath("Solaris.Web.SolarApi.Infrastructure.Rabbit").Select(t => t.UnderlyingSystemType).ToList().ForEach(RegisterType);
            // collection.BuildServiceProvider().GetRequiredService<RabbiHandler>();
        }
    }
}