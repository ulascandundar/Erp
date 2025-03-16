using Erp.Domain.Entities;
using Erp.Domain.Entities.NoSqlEntities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Erp.Infrastructure.Configurations;

/// <summary>
/// PostgreSQL jsonb veri tipi için extension metodlar
/// </summary>
public static class JsonbExtensions
{
    /// <summary>
    /// Entity modellerinde jsonb veri tiplerini yapılandırır
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    public static void ConfigureJsonbTypes(this ModelBuilder modelBuilder)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // User.Customer jsonb yapılandırması
        modelBuilder.Entity<User>()
            .Property(e => e.Customer)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<Customer>(v, options) ?? new Customer());

        // OrderItem.RawMaterialConsumptionReport jsonb yapılandırması
        modelBuilder.Entity<OrderItem>()
            .Property(e => e.RawMaterialConsumptionReport)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<List<RawMaterialConsumption>>(v, options) ?? new List<RawMaterialConsumption>());
    }

    /// <summary>
    /// Belirli bir entity ve property için jsonb veri tipini yapılandırır
    /// </summary>
    /// <typeparam name="TEntity">Entity tipi</typeparam>
    /// <typeparam name="TProperty">Property tipi</typeparam>
    /// <param name="modelBuilder">Model builder</param>
    /// <param name="propertyExpression">Property seçici lambda ifadesi</param>
    public static void ConfigureJsonbProperty<TEntity, TProperty>(
        this ModelBuilder modelBuilder,
        System.Linq.Expressions.Expression<Func<TEntity, TProperty>> propertyExpression)
        where TEntity : class
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        modelBuilder.Entity<TEntity>()
            .Property(propertyExpression)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, options),
                v => JsonSerializer.Deserialize<TProperty>(v, options) ?? Activator.CreateInstance<TProperty>());
    }
} 