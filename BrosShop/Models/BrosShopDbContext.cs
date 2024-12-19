using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace BrosShop.Models;

public partial class BrosShopDbContext : DbContext
{
    //private readonly string _connectionString;
    //public BrosShopDbContext()
    //{
    //    var configuration = new ConfigurationBuilder()
    //        .SetBasePath(Directory.GetCurrentDirectory())
    //        .AddJsonFile("appsettings.json")
    //        .Build();

    //    _connectionString = configuration.GetConnectionString("DefaultConnection");
    //}

    public BrosShopDbContext(string connectionString)
            : base(new DbContextOptionsBuilder<BrosShopDbContext>()
                .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
                .Options)
    {
    }

    public virtual DbSet<Administrator> Administrators { get; set; }

    public virtual DbSet<BrosShopCategory> BrosShopCategories { get; set; }

    public virtual DbSet<BrosShopColor> BrosShopColors { get; set; }

    public virtual DbSet<BrosShopImage> BrosShopImages { get; set; }

    public virtual DbSet<BrosShopOrder> BrosShopOrders { get; set; }

    public virtual DbSet<BrosShopOrderComposition> BrosShopOrderCompositions { get; set; }

    public virtual DbSet<BrosShopProduct> BrosShopProducts { get; set; }

    public virtual DbSet<BrosShopProductAttribute> BrosShopProductAttributes { get; set; }

    public virtual DbSet<BrosShopSize> BrosShopSizes { get; set; }

    public virtual DbSet<BrosShopUser> BrosShopUsers { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    optionsBuilder.UseMySql(_connectionString, ServerVersion.AutoDetect(_connectionString));
    //}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.BrosShopAdministratorId).HasName("PRIMARY");

            entity.Property(e => e.BrosShopAdministratorId).HasColumnName("BrosShop_AdministratorId");
            entity.Property(e => e.BrosShopIsAdmin)
                .HasDefaultValueSql("b'0'")
                .HasColumnType("bit(1)")
                .HasColumnName("BrosShop_IsAdmin");
            entity.Property(e => e.BrosShopLogin)
                .HasMaxLength(45)
                .HasColumnName("BrosShop_Login");
            entity.Property(e => e.BrosShopPassword)
                .HasMaxLength(45)
                .HasColumnName("BrosShop_Password");
        });

        modelBuilder.Entity<BrosShopCategory>(entity =>
        {
            entity.HasKey(e => e.BrosShopCategoryId).HasName("PRIMARY");

            entity.ToTable("BrosShop_Category");

            entity.Property(e => e.BrosShopCategoryId).HasColumnName("BrosShop_CategoryId");
            entity.Property(e => e.BrosShopCategoryTitle)
                .HasMaxLength(45)
                .HasColumnName("BrosShop_CategoryTitle");
        });

        modelBuilder.Entity<BrosShopColor>(entity =>
        {
            entity.HasKey(e => e.ColorId).HasName("PRIMARY");

            entity.ToTable("BrosShop_Color");

            entity.Property(e => e.ColorTitle).HasMaxLength(15);
        });

        modelBuilder.Entity<BrosShopImage>(entity =>
        {
            entity.HasKey(e => e.BrosShopImagesId).HasName("PRIMARY");

            entity.ToTable("BrosShop_Images");

            entity.HasIndex(e => e.BrosShopProductId, "BrosShop_FK_Images_idx");

            entity.Property(e => e.BrosShopImagesId).HasColumnName("BrosShop_ImagesId");
            entity.Property(e => e.BrosShopImageTitle)
                .HasMaxLength(45)
                .HasColumnName("BrosShop_ImageTitle");
            entity.Property(e => e.BrosShopProductId).HasColumnName("BrosShop_ProductId");

            entity.HasOne(d => d.BrosShopProduct).WithMany(p => p.BrosShopImages)
                .HasForeignKey(d => d.BrosShopProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BrosShop_FK_Images");
        });

        modelBuilder.Entity<BrosShopOrder>(entity =>
        {
            entity.HasKey(e => e.BrosShopOrderId).HasName("PRIMARY");

            entity.ToTable("BrosShop_Order");

            entity.HasIndex(e => e.BrosShopUserId, "fk_Usersfk_idx");

            entity.Property(e => e.BrosShopOrderId).HasColumnName("BrosShop_OrderId");
            entity.Property(e => e.BrosShopDateTimeOrder)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("BrosShop_DateTimeOrder");
            entity.Property(e => e.BrosShopTypeOrder)
                .HasDefaultValueSql("'касса'")
                .HasColumnType("enum('веб-сайт','касса','WB')")
                .HasColumnName("BrosShop_TypeOrder");
            entity.Property(e => e.BrosShopUserId).HasColumnName("BrosShop_UserId");

            entity.HasOne(d => d.BrosShopUser).WithMany(p => p.BrosShopOrders)
                .HasForeignKey(d => d.BrosShopUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BrosShop_FK_Users");
        });

        modelBuilder.Entity<BrosShopOrderComposition>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("BrosShop_OrderComposition");

            entity.HasIndex(e => e.BrosShopAttributesId, "BrosShop_OrderComposition_ibfk_1_idx");

            entity.HasIndex(e => e.BrosShopOrderId, "BrosShop_OrderComposition_ibfk_2");

            entity.Property(e => e.BrosShopAttributesId).HasColumnName("BrosShop_AttributesId");
            entity.Property(e => e.BrosShopCost)
                .HasPrecision(9, 2)
                .HasColumnName("BrosShop_Cost");
            entity.Property(e => e.BrosShopOrderId).HasColumnName("BrosShop_OrderId");
            entity.Property(e => e.BrosShopQuantity)
                .HasDefaultValueSql("'1'")
                .HasColumnName("BrosShop_Quantity");

            entity.HasOne(d => d.BrosShopAttributes).WithMany()
                .HasForeignKey(d => d.BrosShopAttributesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_BrosShop_OrderComposition_1");

            entity.HasOne(d => d.BrosShopOrder).WithMany()
                .HasForeignKey(d => d.BrosShopOrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BrosShop_OrderComposition_ibfk_2");
        });

        modelBuilder.Entity<BrosShopProduct>(entity =>
        {
            entity.HasKey(e => e.BrosShopProductId).HasName("PRIMARY");

            entity.ToTable("BrosShop_Product");

            entity.HasIndex(e => e.BrosShopCategoryId, "BrosShop_FK_Category_idx");

            entity.Property(e => e.BrosShopProductId).HasColumnName("BrosShop_ProductId");
            entity.Property(e => e.BrosShopCategoryId).HasColumnName("BrosShop_CategoryId");
            entity.Property(e => e.BrosShopDescription)
                .HasMaxLength(500)
                .HasColumnName("BrosShop_Description");
            entity.Property(e => e.BrosShopDiscountPercent)
                .HasDefaultValueSql("'0'")
                .HasColumnName("BrosShop_DiscountPercent");
            entity.Property(e => e.BrosShopPrice)
                .HasPrecision(7, 2)
                .HasColumnName("BrosShop_Price");
            entity.Property(e => e.BrosShopPurcharesePrice)
                .HasPrecision(7, 2)
                .HasColumnName("BrosShop_PurcharesePrice");
            entity.Property(e => e.BrosShopTitle)
                .HasMaxLength(100)
                .HasColumnName("BrosShop_Title");
            entity.Property(e => e.BrosShopWbarticul).HasColumnName("BrosShop_WBArticul");

            entity.HasOne(d => d.BrosShopCategory).WithMany(p => p.BrosShopProducts)
                .HasForeignKey(d => d.BrosShopCategoryId)
                .HasConstraintName("BrosShop_FK_Category");
        });

        modelBuilder.Entity<BrosShopProductAttribute>(entity =>
        {
            entity.HasKey(e => e.BrosShopAttributesId).HasName("PRIMARY");

            entity.ToTable("BrosShop_Product_Attributes");

            entity.HasIndex(e => e.BrosShopProductId, "BrosShop_Product_Attributes_ibfk_1");

            entity.HasIndex(e => e.BrosShopSizeId, "BrosShop_Size_Attributes_1_idx");

            entity.HasIndex(e => e.BrosShopColorId, "index3");

            entity.Property(e => e.BrosShopAttributesId).HasColumnName("BrosShop_AttributesId");
            entity.Property(e => e.BrosShopColorId).HasColumnName("BrosShop_ColorId");
            entity.Property(e => e.BrosShopCount)
                .HasDefaultValueSql("'1'")
                .HasColumnName("BrosShop_Count");
            entity.Property(e => e.BrosShopProductId).HasColumnName("BrosShop_ProductId");
            entity.Property(e => e.BrosShopSizeId).HasColumnName("BrosShop_Size");

            entity.HasOne(d => d.BrosShopColor).WithMany(p => p.BrosShopProductAttributes)
                .HasForeignKey(d => d.BrosShopColorId)
                .HasConstraintName("BrosShop_Color_Attributes_1");

            entity.HasOne(d => d.BrosShopProduct).WithMany(p => p.BrosShopProductAttributes)
                .HasForeignKey(d => d.BrosShopProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("BrosShop_Product_Attributes_ibfk_1");

            entity.HasOne(d => d.BrosShopSize).WithMany(p => p.BrosShopProductAttributes)
                .HasForeignKey(d => d.BrosShopSizeId)
                .HasConstraintName("BrosShop_Size_Attributes_1");
        });

        modelBuilder.Entity<BrosShopSize>(entity =>
        {
            entity.HasKey(e => e.SizeId).HasName("PRIMARY");

            entity.ToTable("BrosShop_Size");

            entity.Property(e => e.Size).HasMaxLength(10);
        });

        modelBuilder.Entity<BrosShopUser>(entity =>
        {
            entity.HasKey(e => e.BrosShopUserId).HasName("PRIMARY");

            entity.ToTable("BrosShop_User");

            entity.HasIndex(e => e.BrosShopUsername, "BrosShop_Username_UNIQUE").IsUnique();

            entity.Property(e => e.BrosShopUserId).HasColumnName("BrosShop_UserId");
            entity.Property(e => e.BrosShopEmail)
                .HasMaxLength(100)
                .HasColumnName("BrosShop_Email");
            entity.Property(e => e.BrosShopFullName)
                .HasMaxLength(100)
                .HasColumnName("BrosShop_FullName");
            entity.Property(e => e.BrosShopPassword)
                .HasMaxLength(255)
                .HasColumnName("BrosShop_Password");
            entity.Property(e => e.BrosShopPhoneNumber)
                .HasMaxLength(11)
                .HasColumnName("BrosShop_PhoneNumber");
            entity.Property(e => e.BrosShopRegistrationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("BrosShop_RegistrationDate");
            entity.Property(e => e.BrosShopUsername)
                .HasMaxLength(50)
                .HasColumnName("BrosShop_Username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
