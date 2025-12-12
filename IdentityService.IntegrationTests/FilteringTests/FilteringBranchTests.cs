using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using IdentityService.Core.Entities;
using Xunit;
using ZivraFramework.Core.Filtering.Entities;
using ZivraFramework.Core.Filtering;

namespace IdentityService.IntegrationTests.FilteringTests;

public class FilteringBranchTests
{
    [Fact]
    public void ApplyFiltering_Branch_Contains_Works()
    {
        var storeId = Guid.NewGuid();
        var data = new List<Branch>
        {
            new Branch { Id = Guid.NewGuid(), StoreId = storeId, Name = "Garu Selatan" },
            new Branch { Id = Guid.NewGuid(), StoreId = storeId, Name = "Toko Lain" },
            new Branch { Id = Guid.NewGuid(), StoreId = storeId, Name = "Garu Utara" },
            new Branch { Id = Guid.NewGuid(), StoreId = Guid.NewGuid(), Name = "Garu Luar" }
        };

        var req = new QueryRequest
        {
            Filters = new List<QueryFilter>
            {
                new QueryFilter { Field = "Name", Operator = "contains", Value = "Garu" }
            },
            Page = 1,
            PageSize = 10
        };

        var q = data.AsQueryable().ApplyFiltering(req).Where(b => b.StoreId == storeId).ToList();

        q.Count.Should().Be(2);
        q.All(b => b.Name.Contains("Garu")).Should().BeTrue();
    }

    [Fact]
    public void ApplyFiltering_Branch_NotIn_Works()
    {
        var storeId = Guid.NewGuid();
        var b1 = Guid.NewGuid();
        var b2 = Guid.NewGuid();
        var data = new List<Branch>
        {
            new Branch { Id = b1, StoreId = storeId, Name = "A" },
            new Branch { Id = b2, StoreId = storeId, Name = "B" },
            new Branch { Id = Guid.NewGuid(), StoreId = storeId, Name = "C" }
        };

        var req = new QueryRequest
        {
            Filters = new List<QueryFilter>
            {
                new QueryFilter { Field = "Id", Operator = "notin", Values = new List<object> { b1.ToString(), b2.ToString() } }
            },
            Page = 1,
            PageSize = 10
        };

        var q = data.AsQueryable().ApplyFiltering(req).ToList();

        q.Count.Should().Be(1);
        q[0].Name.Should().Be("C");
    }
}

