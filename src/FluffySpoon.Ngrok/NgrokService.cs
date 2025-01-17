﻿using FluffySpoon.Ngrok.Models;
using NgrokApi;

namespace FluffySpoon.Ngrok;

public class NgrokService : INgrokService 
{
    private readonly INgrokDownloader _downloader;
    private readonly INgrokProcess _process;
    private readonly IEnumerable<INgrokLifetimeHook> _hooks;
    private readonly INgrokApiClient _ngrok;

    private bool _isInitialized;
    
    private readonly HashSet<TunnelResponse> _activeTunnels;

    public IReadOnlyCollection<TunnelResponse> ActiveTunnels => _activeTunnels;

    public NgrokService(
        INgrokDownloader downloader,
        INgrokProcess process,
        IEnumerable<INgrokLifetimeHook> hooks,
        INgrokApiClient ngrok)
    {
        _downloader = downloader;
        _process = process;
        _hooks = hooks;
        _ngrok = ngrok;

        _activeTunnels = new HashSet<TunnelResponse>();
    }
    
    public async Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
    {
        while (!ActiveTunnels.Any() && !cancellationToken.IsCancellationRequested)
            await Task.Delay(25, cancellationToken);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return;
        
        _isInitialized = true;
        
        await _downloader.DownloadExecutableAsync(cancellationToken);
        _process.Start();
    }

    public async Task<TunnelResponse> StartAsync(string name,
        Uri host,
        CancellationToken cancellationToken)
    {
        await InitializeAsync(cancellationToken);
        
        var tunnel = await GetOrCreateTunnelAsync(name, host, cancellationToken);

        _activeTunnels.Add(tunnel);
        
        await Task.WhenAll(_hooks
            .ToArray()
            .Select(x => x
                .OnCreatedAsync(tunnel, cancellationToken)));

        return tunnel;
    }

    private async Task<TunnelResponse> GetOrCreateTunnelAsync(string name, Uri host, CancellationToken cancellationToken)
    {
        var existingTunnels = await _ngrok.GetTunnelsAsync(cancellationToken);
        var existingTunnel = existingTunnels.FirstOrDefault(x => new Uri(x.Config.Address) == host);
        if (existingTunnel != null)
            return existingTunnel;
        
        return await _ngrok.CreateTunnelAsync(
            name,
            host,
            cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var hooks = _hooks.ToArray();
        var activeTunnels = _activeTunnels.ToArray();

        _process.Stop();
        _activeTunnels.Clear();
        
        await Task.WhenAll(activeTunnels
            .Select(tunnel => Task.WhenAll(hooks
                .Select(hook => hook.OnDestroyedAsync(tunnel, cancellationToken)))));
    }
}