﻿// <auto-generated />
using eshop.api.order.Models.Order;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace eshop.api.order.Migrations
{
    [DbContext(typeof(OrderContext))]
    [Migration("20180329045359_IntialMigration")]
    partial class IntialMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("eshop.api.order.Models.Article.ArticleOrdered", b =>
                {
                    b.Property<string>("ArticleId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("OrderId");

                    b.Property<int>("Quantity");

                    b.HasKey("ArticleId");

                    b.HasIndex("OrderId");

                    b.ToTable("ArticleOrdereds");
                });

            modelBuilder.Entity("eshop.api.order.Models.Order.Order", b =>
                {
                    b.Property<string>("OrderId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("CustomerId");

                    b.Property<string>("Status");

                    b.HasKey("OrderId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("eshop.api.order.Models.Article.ArticleOrdered", b =>
                {
                    b.HasOne("eshop.api.order.Models.Order.Order")
                        .WithMany("ArticleOrdered")
                        .HasForeignKey("OrderId");
                });
#pragma warning restore 612, 618
        }
    }
}
