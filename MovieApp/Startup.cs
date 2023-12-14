﻿using System;
using MovieApp.Services;
using MovieLibraryEntities.Dao;
using MovieLibraryEntities.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MovieApp;

/// <summary>
///     Used for registration of new interfaces
/// </summary>
internal class Startup
{
    public IServiceProvider ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddFile("app.log");
        });

        // Add new lines of code here to register any interfaces and concrete services you create
        services.AddTransient<IMainService, MainService>();
        services.AddTransient<IRepository, Repository>();
        services.AddTransient<Helper>();
        services.AddDbContextFactory<MovieContext>();

        return services.BuildServiceProvider();
    }
}