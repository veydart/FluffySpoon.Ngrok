﻿using FluffySpoon.Ngrok.Models;
using NgrokApi;

namespace FluffySpoon.Ngrok;

public interface INgrokService
{
    IReadOnlyCollection<TunnelResponse> ActiveTunnels { get; }
    
    Task WaitUntilReadyAsync(CancellationToken cancellationToken = default);
    
    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<TunnelResponse> StartAsync(string name,
        Uri host, 
        CancellationToken cancellationToken = default);

    Task StopAsync(CancellationToken cancellationToken = default);
}
