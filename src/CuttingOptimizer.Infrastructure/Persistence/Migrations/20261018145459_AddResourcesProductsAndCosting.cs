using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CuttingOptimizer.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResourcesProductsAndCosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourcePartRole",
                table: "RequiredCuts",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceProjectItemId",
                table: "RequiredCuts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Materials",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Aluminum");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Materials",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ThicknessMm",
                table: "Materials",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Brand",
                table: "Accessories",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "Accessories",
                type: "character varying(16)",
                maxLength: 16,
                nullable: false,
                defaultValue: "Piece");

            migrationBuilder.CreateTable(
                name: "GlassTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkshopId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlassTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GlassTypes_Workshops_WorkshopId",
                        column: x => x.WorkshopId,
                        principalTable: "Workshops",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSeeded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GlassSpecs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GlassTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ThicknessMm = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PricePerSqm = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GlassSpecs", x => x.Id);
                    table.CheckConstraint("CK_GlassSpecs_PricePerSqm", "\"PricePerSqm\" >= 0");
                    table.CheckConstraint("CK_GlassSpecs_ThicknessMm", "\"ThicknessMm\" > 0");
                    table.ForeignKey(
                        name: "FK_GlassSpecs_GlassTypes_GlassTypeId",
                        column: x => x.GlassTypeId,
                        principalTable: "GlassTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SashCount = table.Column<int>(type: "integer", nullable: true),
                    OverlapMm = table.Column<int>(type: "integer", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSeeded = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_ProductTypes_ProductTypeId",
                        column: x => x.ProductTypeId,
                        principalTable: "ProductTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    WidthMm = table.Column<int>(type: "integer", nullable: false),
                    HeightMm = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CalculationSnapshotJson = table.Column<string>(type: "text", nullable: true),
                    LastCalculatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectItems", x => x.Id);
                    table.CheckConstraint("CK_ProjectItems_HeightMm", "\"HeightMm\" > 0");
                    table.CheckConstraint("CK_ProjectItems_Quantity", "\"Quantity\" > 0");
                    table.CheckConstraint("CK_ProjectItems_WidthMm", "\"WidthMm\" > 0");
                    table.ForeignKey(
                        name: "FK_ProjectItems_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectItems_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateAccessorySlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    QuantityRule = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Qty = table.Column<int>(type: "integer", nullable: false),
                    DefaultAccessoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateAccessorySlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateAccessorySlots_Accessories_DefaultAccessoryId",
                        column: x => x.DefaultAccessoryId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TemplateAccessorySlots_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateGlassSlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PaneCount = table.Column<int>(type: "integer", nullable: false),
                    WidthCoefW = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    HeightCoefW = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    OffsetWMm = table.Column<int>(type: "integer", nullable: false),
                    WidthCoefH = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    HeightCoefH = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    OffsetHMm = table.Column<int>(type: "integer", nullable: false),
                    Required = table.Column<bool>(type: "boolean", nullable: false),
                    UseSashWidthDivision = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateGlassSlots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateGlassSlots_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TemplateParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductVariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    QtyPerUnit = table.Column<int>(type: "integer", nullable: false),
                    WidthCoef = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    HeightCoef = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false),
                    OffsetMm = table.Column<int>(type: "integer", nullable: false),
                    UseSashWidthDivision = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultAluminumMaterialId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateParts_Materials_DefaultAluminumMaterialId",
                        column: x => x.DefaultAluminumMaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TemplateParts_ProductVariants_ProductVariantId",
                        column: x => x.ProductVariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectItemInspections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    LocationLabel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    MeasuredWidthMm = table.Column<int>(type: "integer", nullable: true),
                    MeasuredHeightMm = table.Column<int>(type: "integer", nullable: true),
                    InspectedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    InspectedBy = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectItemInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectItemInspections_ProjectItems_ProjectItemId",
                        column: x => x.ProjectItemId,
                        principalTable: "ProjectItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectItemAccessoryChoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateAccessorySlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuantityOverride = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectItemAccessoryChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectItemAccessoryChoices_Accessories_AccessoryId",
                        column: x => x.AccessoryId,
                        principalTable: "Accessories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectItemAccessoryChoices_ProjectItems_ProjectItemId",
                        column: x => x.ProjectItemId,
                        principalTable: "ProjectItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectItemAccessoryChoices_TemplateAccessorySlots_Template~",
                        column: x => x.TemplateAccessorySlotId,
                        principalTable: "TemplateAccessorySlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectItemGlassChoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateGlassSlotId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlassSpecId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectItemGlassChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectItemGlassChoices_GlassSpecs_GlassSpecId",
                        column: x => x.GlassSpecId,
                        principalTable: "GlassSpecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectItemGlassChoices_ProjectItems_ProjectItemId",
                        column: x => x.ProjectItemId,
                        principalTable: "ProjectItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectItemGlassChoices_TemplateGlassSlots_TemplateGlassSlo~",
                        column: x => x.TemplateGlassSlotId,
                        principalTable: "TemplateGlassSlots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectItemPartChoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplatePartId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectItemPartChoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectItemPartChoices_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectItemPartChoices_ProjectItems_ProjectItemId",
                        column: x => x.ProjectItemId,
                        principalTable: "ProjectItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectItemPartChoices_TemplateParts_TemplatePartId",
                        column: x => x.TemplatePartId,
                        principalTable: "TemplateParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TemplatePartAllowedMaterials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplatePartId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatePartAllowedMaterials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplatePartAllowedMaterials_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TemplatePartAllowedMaterials_TemplateParts_TemplatePartId",
                        column: x => x.TemplatePartId,
                        principalTable: "TemplateParts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequiredCuts_SourceProjectItemId",
                table: "RequiredCuts",
                column: "SourceProjectItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GlassSpecs_GlassTypeId",
                table: "GlassSpecs",
                column: "GlassTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_GlassTypes_WorkshopId_Name",
                table: "GlassTypes",
                columns: new[] { "WorkshopId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductTypes_Kind",
                table: "ProductTypes",
                column: "Kind");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductTypeId_Code",
                table: "ProductVariants",
                columns: new[] { "ProductTypeId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemAccessoryChoices_AccessoryId",
                table: "ProjectItemAccessoryChoices",
                column: "AccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemAccessoryChoices_ProjectItemId_TemplateAccessory~",
                table: "ProjectItemAccessoryChoices",
                columns: new[] { "ProjectItemId", "TemplateAccessorySlotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemAccessoryChoices_TemplateAccessorySlotId",
                table: "ProjectItemAccessoryChoices",
                column: "TemplateAccessorySlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemGlassChoices_GlassSpecId",
                table: "ProjectItemGlassChoices",
                column: "GlassSpecId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemGlassChoices_ProjectItemId_TemplateGlassSlotId",
                table: "ProjectItemGlassChoices",
                columns: new[] { "ProjectItemId", "TemplateGlassSlotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemGlassChoices_TemplateGlassSlotId",
                table: "ProjectItemGlassChoices",
                column: "TemplateGlassSlotId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemInspections_ProjectItemId",
                table: "ProjectItemInspections",
                column: "ProjectItemId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemPartChoices_MaterialId",
                table: "ProjectItemPartChoices",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemPartChoices_ProjectItemId_TemplatePartId",
                table: "ProjectItemPartChoices",
                columns: new[] { "ProjectItemId", "TemplatePartId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItemPartChoices_TemplatePartId",
                table: "ProjectItemPartChoices",
                column: "TemplatePartId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItems_ProductVariantId",
                table: "ProjectItems",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectItems_ProjectId",
                table: "ProjectItems",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAccessorySlots_DefaultAccessoryId",
                table: "TemplateAccessorySlots",
                column: "DefaultAccessoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateAccessorySlots_ProductVariantId",
                table: "TemplateAccessorySlots",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateGlassSlots_ProductVariantId",
                table: "TemplateGlassSlots",
                column: "ProductVariantId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplatePartAllowedMaterials_MaterialId",
                table: "TemplatePartAllowedMaterials",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplatePartAllowedMaterials_TemplatePartId_MaterialId",
                table: "TemplatePartAllowedMaterials",
                columns: new[] { "TemplatePartId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateParts_DefaultAluminumMaterialId",
                table: "TemplateParts",
                column: "DefaultAluminumMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateParts_ProductVariantId",
                table: "TemplateParts",
                column: "ProductVariantId");

            migrationBuilder.AddForeignKey(
                name: "FK_RequiredCuts_ProjectItems_SourceProjectItemId",
                table: "RequiredCuts",
                column: "SourceProjectItemId",
                principalTable: "ProjectItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RequiredCuts_ProjectItems_SourceProjectItemId",
                table: "RequiredCuts");

            migrationBuilder.DropTable(
                name: "ProjectItemAccessoryChoices");

            migrationBuilder.DropTable(
                name: "ProjectItemGlassChoices");

            migrationBuilder.DropTable(
                name: "ProjectItemInspections");

            migrationBuilder.DropTable(
                name: "ProjectItemPartChoices");

            migrationBuilder.DropTable(
                name: "TemplatePartAllowedMaterials");

            migrationBuilder.DropTable(
                name: "TemplateAccessorySlots");

            migrationBuilder.DropTable(
                name: "GlassSpecs");

            migrationBuilder.DropTable(
                name: "TemplateGlassSlots");

            migrationBuilder.DropTable(
                name: "ProjectItems");

            migrationBuilder.DropTable(
                name: "TemplateParts");

            migrationBuilder.DropTable(
                name: "GlassTypes");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "ProductTypes");

            migrationBuilder.DropIndex(
                name: "IX_RequiredCuts_SourceProjectItemId",
                table: "RequiredCuts");

            migrationBuilder.DropColumn(
                name: "SourcePartRole",
                table: "RequiredCuts");

            migrationBuilder.DropColumn(
                name: "SourceProjectItemId",
                table: "RequiredCuts");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ThicknessMm",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Brand",
                table: "Accessories");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Accessories");
        }
    }
}
