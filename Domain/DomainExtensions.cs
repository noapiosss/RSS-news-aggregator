using System;

using Domain.Commands;
using Domain.Database;
using Domain.Helpers;
using Domain.Helpers.Interfaces;
using MediatR;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Domain
{
    public static class DomainExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services,
            Action<IServiceProvider, DbContextOptionsBuilder> dbOptionsAction)
        {
            return services.AddMediatR(typeof(CreateUserCommand))
                .AddSingleton<ISHA256, PasswordHelper>()
                .AddDbContext<RSSNewsDbContext>(dbOptionsAction);
        }
    }
}