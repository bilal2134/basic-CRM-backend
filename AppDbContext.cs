﻿using CRM.Model;
using Microsoft.EntityFrameworkCore;

namespace CRM
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
    }
}
