﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. 

using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Properties;
using Microsoft.OpenApi.Services;
using Microsoft.OpenApi.Validations.Rules;
using Xunit;

namespace Microsoft.OpenApi.Validations.Tests
{
    public class OpenApiHeaderValidationTests
    {
        [Fact]
        public void ValidateExampleShouldNotHaveDataTypeMismatchForSimpleSchema()
        {
            // Arrange
            IEnumerable<OpenApiError> errors;
            var header = new OpenApiHeader()
            {
                Required = true,
                Example = new OpenApiInteger(55),
                Schema = new OpenApiSchema()
                {
                    Type = "string",
                }
            };

            // Act
            var validator = new OpenApiValidator(ValidationRuleSet.GetDefaultRuleSet());
            var walker = new OpenApiWalker(validator);
            walker.Walk(header);

            errors = validator.Errors;
            var warnings = validator.Warnings;
            bool result = !warnings.Any();

            // Assert
            result.Should().BeFalse();
            warnings.Select(e => e.Message).Should().BeEquivalentTo(new[]
            {
                RuleHelpers.DataTypeMismatchedErrorMessage
            });
            warnings.Select(e => e.Pointer).Should().BeEquivalentTo(new[]
            {
                "#/example",
            });
        }

        [Fact]
        public void ValidateExamplesShouldNotHaveDataTypeMismatchForSimpleSchema()
        {
            // Arrange
            IEnumerable<OpenApiError> warnings;

            var header = new OpenApiHeader()
            {
                Required = true,
                Schema = new OpenApiSchema()
                {
                    Type = "object",
                    AdditionalProperties = new OpenApiSchema()
                    {
                        Type = "integer",
                    }
                },
                Examples =
                    {
                        ["example0"] = new OpenApiExample()
                        {
                            Value = new OpenApiString("1"),
                        },
                        ["example1"] = new OpenApiExample()
                        {
                           Value = new OpenApiObject()
                            {
                                ["x"] = new OpenApiInteger(2),
                                ["y"] = new OpenApiString("20"),
                                ["z"] = new OpenApiString("200")
                            }
                        },
                        ["example2"] = new OpenApiExample()
                        {
                            Value =
                            new OpenApiArray()
                            {
                                new OpenApiInteger(3)
                            }
                        },
                        ["example3"] = new OpenApiExample()
                        {
                            Value = new OpenApiObject()
                            {
                                ["x"] = new OpenApiInteger(4),
                                ["y"] = new OpenApiInteger(40),
                            }
                        },
                    }
            };

            // Act
            var validator = new OpenApiValidator(ValidationRuleSet.GetDefaultRuleSet());
            var walker = new OpenApiWalker(validator);
            walker.Walk(header);

            warnings = validator.Warnings;
            bool result = !warnings.Any();

            // Assert
            result.Should().BeFalse();
            warnings.Select(e => e.Message).Should().BeEquivalentTo(new[]
            {
                RuleHelpers.DataTypeMismatchedErrorMessage,
                RuleHelpers.DataTypeMismatchedErrorMessage,
                RuleHelpers.DataTypeMismatchedErrorMessage,
            });
            warnings.Select(e => e.Pointer).Should().BeEquivalentTo(new[]
            {
                // #enum/0 is not an error since the spec allows
                // representing an object using a string.
                "#/examples/example1/value/y",
                "#/examples/example1/value/z",
                "#/examples/example2/value"
            });
        }
    }
}
