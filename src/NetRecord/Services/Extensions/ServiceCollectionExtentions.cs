using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetRecord.Interfaces;
using NetRecord.Utils.Enums;

namespace NetRecord.Services.Extensions;

public static class ServiceCollectionExtentions
{
    /// <summary>
    /// Overwrite or replace any existing IHttpClientFactories and their clients, and recreate them within NetRecord
    /// </summary>
    /// <param name="configuration">The Service configuration that will be used</param>
    [Obsolete("Not Currently Implemented")]
    public static IServiceCollection ReplaceHttpWithNetRecord(
        this IServiceCollection services,
        NetRecordConfiguration configuration
    )
    {
        // TODO: This would be a SUPER useful thing to have, so that people can call their own configure services like normal, and have NetRecord automatically replace all their clients
        throw new NotImplementedException("Still needing implementation on http replacement");
    }

    /// <summary>
    /// Overwrite or replace any existing IHttpClientFactories and their clients, and recreate them within NetRecord
    /// </summary>
    /// <param name="recordMode">The ServiceMode to be used by NetRecord deciding how it will handle requests</param>
    /// <param name="recordingsDir">The path from the solution root to the desired recording directory, as an expression so reflection can be used to change it dynamically</param>
    /// <param name="configurationSettingFunc">An optional expression that will be used to set the optional values within the service configuration</param>
    [Obsolete("Not Currently implemented")]
    public static IServiceCollection ReplaceHttpWithNetRecord(
        this IServiceCollection services,
        ServiceMode recordMode,
        Func<string> recordingsDir,
        Action<NetRecordConfiguration>? configurationSettingFunc = null
    )
    {
        var configuration = new NetRecordConfiguration
        {
            Mode = recordMode,
            RecordingsDir = recordingsDir,
        };
        configurationSettingFunc?.Invoke(configuration);

        return services.ReplaceHttpWithNetRecord(configuration);
    }

    /// <summary>
    /// Overwrite or replace any existing IHttpClientFactories and their clients, and recreate them within NetRecord
    /// </summary>
    /// <param name="recordMode">The ServiceMode to be used by NetRecord deciding how it will handle requests</param>
    /// <param name="recordingsDir">The path from the solution root to the desired recording directory, as an expression so reflection can be used to change it dynamically</param>
    /// <param name="configurationSettingFunc">An optional expression that will be used to set the optional values within the service configuration</param>
    [Obsolete("Not Currently implemented")]
    public static IServiceCollection ReplaceHttpWithNetRecord(
        this IServiceCollection services,
        ServiceMode recordMode,
        string recordingsDir,
        Action<NetRecordConfiguration>? configurationSettingFunc = null
    )
    {
        return services.ReplaceHttpWithNetRecord(
            recordMode,
            () => recordingsDir,
            configurationSettingFunc
        );
    }

    /// <summary>
    /// Adds a passed httpClient to the services
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="httpClient">The httpClient to add to the services</param>
    /// <param name="configuration">The configuration to add to the services</param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        NetRecordHttpClient httpClient,
        NetRecordConfiguration configuration
    )
    {
        return services.AddNetRecordHttpClient(clientName, httpClient.BaseAddress!, configuration);
    }

    /// <summary>
    /// Adds a passed httpClient to the services
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="httpClient">The httpClient to add to the services</param>
    /// <param name="recordMode">The record mode to decide how requests are handled</param>
    /// <param name="recordingsDir">The path from the solution root to save files too</param>
    /// <param name="configurationSettingFunc">A lambda input to set any optional setting values for the configuration</param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        NetRecordHttpClient httpClient,
        ServiceMode recordMode,
        Func<string> recordingsDir,
        Action<NetRecordConfiguration>? configurationSettingFunc = null
    )
    {
        var configuration = new NetRecordConfiguration
        {
            Mode = recordMode,
            RecordingsDir = recordingsDir,
        };
        configurationSettingFunc?.Invoke(configuration);

        return services.AddNetRecordHttpClient(clientName, httpClient, configuration);
    }

    /// <summary>
    /// Adds a passed httpClient to the services
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="httpClient">The httpClient to add to the services</param>
    /// <param name="recordMode">The record mode to decide how requests are handled</param>
    /// <param name="recordingsDir">The path from the solution root to save files too</param>
    /// <param name="configurationSettingFunc">A lambda input to set any optional setting values for the configuration</param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        NetRecordHttpClient httpClient,
        ServiceMode recordMode,
        string recordingsDir,
        Action<NetRecordConfiguration>? configurationSettingFunc = null
    )
    {
        return services.AddNetRecordHttpClient(
            clientName,
            httpClient,
            recordMode,
            () => recordingsDir,
            configurationSettingFunc
        );
    }

    /// <summary>
    /// Adds an httpClient to the services by name and URL
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="baseAddress">The baseAddress for the httpClient</param>
    /// <param name="configuration">The configuration to add to the services</param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        string baseAddress,
        NetRecordConfiguration configuration
    )
    {
        var uri = new Uri(baseAddress);

        return services.AddNetRecordHttpClient(clientName, uri, configuration);
    }

    /// <summary>
    /// Adds an httpClient to the services by name and URL
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="baseAddress">The baseAddress for the httpClient</param>
    /// <param name="recordMode">The record mode to decide how requests are handled</param>
    /// <param name="recordingsDir">The path from the solution root to save files too</param>
    /// <param name="configurationSettingFunc">A lambda input to set any optional setting values for the configuration</param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        string baseAddress,
        ServiceMode recordMode,
        Func<string> recordingsDir,
        Action<NetRecordConfiguration>? configurationSettingFunc = null
    )
    {
        var configuration = new NetRecordConfiguration
        {
            Mode = recordMode,
            RecordingsDir = recordingsDir,
        };
        configurationSettingFunc?.Invoke(configuration);

        return services.AddNetRecordHttpClient(clientName, baseAddress, configuration);
    }

    /// <summary>
    /// Adds an httpClient to the services by name and URL
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="baseAddress">The baseAddress for the httpClient</param>
    /// <param name="recordMode"></param>
    /// <param name="recordingsDir"></param>
    /// <param name="configurationSettingFunc"></param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        string baseAddress,
        ServiceMode recordMode,
        string recordingsDir,
        Action<NetRecordConfiguration>? configurationSettingFunc = null
    )
    {
        return services.AddNetRecordHttpClient(
            clientName,
            baseAddress,
            recordMode,
            () => recordingsDir,
            configurationSettingFunc
        );
    }

    /// <summary>
    /// Adds an httpClient to the services by name and Uri
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="baseAddress">The baseAddress for the httpClient</param>
    /// <param name="configuration">The configuration to add to the services</param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        Uri baseAddress,
        NetRecordConfiguration configuration
    )
    {
        configuration.ClientName = clientName;

        services.TryAddSingleton<IHttpClientFactory, NetRecordFactory>();
        services.AddSingleton<INetRecordConfiguration>(configuration);
        services.AddSingleton<INetRecordClientOptions>(
            new NetRecordClientOptions(clientName, baseAddress)
        );

        return services;
    }

    /// <summary>
    /// Adds an httpClient to the services by name and Uri
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="baseAddress">The baseAddress for the httpClient</param>
    /// <param name="recordMode">The record mode to decide how requests are handled</param>
    /// <param name="recordingsDir">The path from the solution root to save files too</param>
    /// <param name="configurationSettingFunc">A lambda input to set any optional setting values for the configuration</param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        Uri baseAddress,
        ServiceMode recordMode,
        Func<string> recordingsDir,
        Action<NetRecordConfiguration>? configurationSettingFunc = null
    )
    {
        var configuration = new NetRecordConfiguration
        {
            Mode = recordMode,
            RecordingsDir = recordingsDir,
        };
        configurationSettingFunc?.Invoke(configuration);

        return services.AddNetRecordHttpClient(clientName, baseAddress, configuration);
    }

    /// <summary>
    /// Adds an httpClient to the services by name and Uri
    /// </summary>
    /// <param name="clientName">The name of the httpClient to add</param>
    /// <param name="baseAddress">The baseAddress for the httpClient</param>
    /// <param name="recordMode">The record mode to decide how requests are handled</param>
    /// <param name="recordingsDir">The path from the solution root to save files too</param>
    /// <param name="configurationSettingFunc">A lambda input to set any optional setting values for the configuration</param>
    public static IServiceCollection AddNetRecordHttpClient(
        this IServiceCollection services,
        string clientName,
        Uri baseAddress,
        ServiceMode recordMode,
        string recordingsDir,
        Action<NetRecordConfiguration>? configurationSettingFunc = null
    )
    {
        return services.AddNetRecordHttpClient(
            clientName,
            baseAddress,
            recordMode,
            () => recordingsDir,
            configurationSettingFunc
        );
    }
}
