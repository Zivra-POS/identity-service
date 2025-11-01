using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace IdentityService.IntegrationTests.Helpers;

public static class HttpResponseExtensions
{
    public static async Task<T> ShouldDeserializeTo<T>(this HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };
        options.Converters.Add(new JsonStringEnumConverter());

        try
        {
            var result = JsonSerializer.Deserialize<T>(content, options);
            result.Should().NotBeNull($"Gagal deserialize ke {typeof(T).Name}. Content: {content}");
            return result!;
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Gagal deserialize ke {typeof(T).Name}. Content: {content}", ex);
        }
    }



    public static void ShouldHaveStatusCode(this HttpResponseMessage response, HttpStatusCode expectedStatusCode)
    {
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    public static void ShouldBeSuccessStatusCode(this HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    public static void ShouldHaveContentType(this HttpResponseMessage response, string expectedContentType)
    {
        response.Content.Headers.ContentType?.MediaType.Should().Be(expectedContentType);
    }

    public static void ShouldHaveCookie(this HttpResponseMessage response, string cookieName)
    {
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookies = response.Headers.GetValues("Set-Cookie");
        cookies.Should().Contain(cookie => cookie.StartsWith($"{cookieName}="));
    }
}

public static class MultipartFormDataHelper
{
    public static MultipartFormDataContent CreateFormData<T>(T data, Dictionary<string, byte[]>? files = null)
    {
        var formData = new MultipartFormDataContent();
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(data);
            if (value == null)
                continue;

            var propType = property.PropertyType;

            // If the property is an IFormFile (or derived), add it as file content
            if (typeof(Microsoft.AspNetCore.Http.IFormFile).IsAssignableFrom(propType) || value is Microsoft.AspNetCore.Http.IFormFile)
            {
                var formFile = (Microsoft.AspNetCore.Http.IFormFile)value;
                using var ms = new MemoryStream();
                formFile.CopyTo(ms);
                var bytes = ms.ToArray();
                var fileContent = new ByteArrayContent(bytes);
                var contentType = string.IsNullOrWhiteSpace(formFile.ContentType) ? "application/octet-stream" : formFile.ContentType;
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
                // Use property name as name and preserve file name
                formData.Add(fileContent, property.Name, formFile.FileName ?? property.Name);
                continue;
            }

            // If the property is a string, treat as scalar
            if (propType == typeof(string))
            {
                formData.Add(new StringContent(value.ToString() ?? string.Empty), property.Name);
                continue;
            }

            // If property is enumerable (array, list, etc.) but not string, add each element as a separate form entry
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(propType))
            {
                // Skip byte[] (likely used for files if accidentally present)
                if (propType == typeof(byte[]))
                {
                    formData.Add(new StringContent(Convert.ToBase64String((byte[])value)), property.Name);
                    continue;
                }

                var enumerable = (System.Collections.IEnumerable)value;
                foreach (var item in enumerable)
                {
                    if (item == null) continue;
                    formData.Add(new StringContent(item.ToString() ?? string.Empty), property.Name);
                }

                continue;
            }

            // Default scalar handling (ints, GUIDs, bools, DateTime, etc.)
            formData.Add(new StringContent(Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty), property.Name);
        }

        if (files != null)
        {
            foreach (var file in files)
            {
                var fileContent = new ByteArrayContent(file.Value);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
                formData.Add(fileContent, file.Key, $"{file.Key}.jpg");
            }
        }

        return formData;
    }
}
