﻿namespace BackgroundAndHostedServices.Dtos;

public class EndpointResponse
{
	public string? Message { get; set; }
	public string? ServiceName { get; set; }
	public string? CurrentTime { get; set; }
}