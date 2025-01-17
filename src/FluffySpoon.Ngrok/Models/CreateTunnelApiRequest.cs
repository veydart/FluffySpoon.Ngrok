﻿using Newtonsoft.Json;

namespace FluffySpoon.Ngrok.Models;

public class CreateTunnelApiRequest
{
	public string Name { get; set; } = null!;

	[JsonProperty("addr")] 
	public string Address { get; set; } = null!;

	[JsonProperty("proto")] 
	public string Protocol { get; set; } = null!;

    [JsonProperty("bind_tls")]
    public bool BindTls { get; set; } = true;

    public string Subdomain { get; set; } = null!;

	public string HostHeader { get; set; } = null!;
}