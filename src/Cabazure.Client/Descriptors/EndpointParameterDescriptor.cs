﻿namespace Cabazure.Client.SourceGenerator.Descriptors;

public record EndpointParameter(
    string Name,
    string ParameterName,
    string ParameterType,
    string? FormatString);
